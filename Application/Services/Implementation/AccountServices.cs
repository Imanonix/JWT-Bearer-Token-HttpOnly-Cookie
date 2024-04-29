using AutoMapper;
using Microsoft.Extensions.Options;
using MimeKit;
using Application.DTOs;
using Application.ModelServices;
using Application.Services.Interfaces;
using Domain.Interfaces.Repository;
using Domain.Models;
using MailKit.Net.Smtp;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Implementation
{
    public class AccountServices : IAccountServices
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly EmailSetting _emailSetting;
        private readonly JWTSetting _jwtSetting;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountServices(IAccountRepository accountRepository, IMapper mapper, IOptions<EmailSetting> options, IOptions<JWTSetting> jWTSetting, IHttpContextAccessor httpContextAccessor)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
            _emailSetting = options.Value;
            _jwtSetting = jWTSetting.Value;
            _httpContextAccessor = httpContextAccessor;
        }



        public async Task<Tuple<byte[], byte[]>> CreatePasswordHash(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                byte[] passwordSalt = hmac.Key;
                byte[] passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return new Tuple<byte[], byte[]>(passwordSalt, passwordHash);
            }
        }

        public async Task<string> CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<RegisterDTO> GetUserByEmailAsync(string email)
        {
            var user = await _accountRepository.GetUserByEmailAsync(email);
            var register = _mapper.Map<RegisterDTO>(user);
            return register;
        }

        public async Task<RegisterDTO> GetUserById(Guid id)
        {
            var user = await _accountRepository.GetUserById(id);
            var userDTO = _mapper.Map<RegisterDTO>(user);
            return userDTO;
        }

        public async Task<RegisterDTO> RegisterAsync(RegisterDTO registerDTO)
        {
            var user = _mapper.Map<User>(registerDTO);
            Tuple<byte[], byte[]> tuple = await CreatePasswordHash(registerDTO.Password);
            user.PasswordSalt = tuple.Item1;
            user.PasswordHash = tuple.Item2;
            user.VerificationToken = await CreateRandomToken();
            
            var addeduser = await _accountRepository.AddUserAsync(user);    // id is created 
            await _accountRepository.SaveAsync();

            var emailRequest = new EmailRequest
            {
                To = registerDTO.Email,
                Subject = "verification",
                Body = $"<div style='display: flex; flex-direction: column; align-items: center; justify-content: center;'>" +
                $"<div style='background-color: rgb(182, 22, 39);  display: flex; flex-direction: column; align-items: center; justify-content: center; width:70%; padding: 20px 20px;'> <h3 style='font-weight:bold; color:white;'> Dear {user.Username} welcome to Mupik company,</h3></br>" +
                $"<p style='color:white;'>To complete your registration and start exploring Mupik's Products, please verify your email address by clicking the button below:</p> </br>" +
                $"<a style= 'background-color: green; color:white; padding: 10px 20px; text-decoration:none;'  href= https://localhost:7185/api/Account?email={user.Email}&verificationToken={user.VerificationToken}> confirmation </a></div></div>"
            };
            await SendVerificationEmailAsync(emailRequest);
            return _mapper.Map<RegisterDTO>(addeduser);
        }

        public async Task SendVerificationEmailAsync(EmailRequest emailRequest)
        {
            #region create Email
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSetting.DisplayName, _emailSetting.Email));
            email.To.Add(new MailboxAddress(emailRequest.To, emailRequest.To));
            email.Subject = emailRequest.Subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = emailRequest.Body;
            email.Body = builder.ToMessageBody();
            #endregion
            using (var smtp = new SmtpClient())
            {
                smtp.Connect(_emailSetting.Host, _emailSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate(_emailSetting.Email, _emailSetting.Password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
        }

        public async Task<bool> UserConfirmAsync(string email, string verificationToken)
        {
            var IsConfirmed = await _accountRepository.UserConfirmAsync(email, verificationToken);
            if (!IsConfirmed)
            {
                throw new Exception("user not found");
            }
            await _accountRepository.SaveAsync();
            return true;
        }

        #region Generate Json Web Token
        public async Task<string> GenerateJwtToken(string email)
        {

            var user = await _accountRepository.GetUserByEmailAsync(email);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Roles.ToString())
                        // Add more claims 
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSetting.Issuer,
                audience: _jwtSetting.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion

        public async Task AddRefreshTokenAsync(TokenModel tokenModel, string email)
        {
            await _accountRepository.AddRefreshTokenAsync(tokenModel.RefreshToken, email);
            await _accountRepository.SaveAsync();
        }
        
        public async Task<ClaimsPrincipal?> GetTokenPrincipal (string token)
        {

            var validation = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, // Validate the signing key
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key)), 
                ValidateIssuer = false, // Validate the issuer
                ValidateAudience = false, // Validate the audience
                ValidateLifetime = true, // Validate token expiration
                ValidAudience = _jwtSetting.Audience,
                ValidIssuer = _jwtSetting.Issuer
            };

            return  new JwtSecurityTokenHandler().ValidateToken(token, validation,out _);
        }

        public async Task<RegenerateRefreshTokenResult> RegenerateRefreshToken(TokenModel tokenModel, string email)
        {
            //var claim = await GetTokenPrincipal(tokenModel.RefreshToken);
            //var email = claim.Identity.Name;
            var user = await _accountRepository.GetUserByEmailAsync(email);
            if (user == null || user.RefreshToken != tokenModel.RefreshToken || user.RefreshTokenExpired < DateTime.UtcNow)
            {
                return null;
            }

            // Generate new access token
            var newAccessToken = await GenerateJwtToken(email);

            // Generate new refresh token
            var newRefreshToken = await CreateRandomToken();

            // Update user's refresh token in database
            user.RefreshToken = newRefreshToken;
            await _accountRepository.AddRefreshTokenAsync(newRefreshToken, email);
            await _accountRepository.SaveAsync();
            
            var result = new RegenerateRefreshTokenResult
            {
              IsLogedIn = true,
              Token = newAccessToken,
              RefreshToken = newRefreshToken,
              Email = email
            };
            return result;
        }

        public async Task<bool> CheckPasswordAsync(LoginDTO loginDTO)
        {
            var user = await _accountRepository.GetUserByEmailAsync(loginDTO.Email);
            using (var hmac = new HMACSHA512(user.PasswordSalt))
            {
                var passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginDTO.Password));
                if (!passwordHash.SequenceEqual(user.PasswordHash))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task SetCookie(string token, string refreshToken, string email)
        {
            #region Set Cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddMinutes(5),
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.Strict,
                Domain = "localhost",
            };

            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddHours(12),
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.Strict,
                Domain = "localhost",
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append("X-Access-Token", token, cookieOptions);
            _httpContextAccessor.HttpContext?.Response.Cookies.Append("X-Refresh-Token", refreshToken, refreshTokenCookieOptions);
            #endregion
        }
    } 
}
