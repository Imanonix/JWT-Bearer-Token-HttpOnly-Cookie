using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UserAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IAccountServices _accountServices;
        private readonly IHttpContextAccessor _contextAccessor;
        public AccountController(IAccountServices accountServices, IHttpContextAccessor contextAccessor)
        {
            _accountServices = accountServices;
            _contextAccessor = contextAccessor;
        }
        [HttpPost]
        [Route("/Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("the model is not valid");
            }
            var returnedUser = await _accountServices.GetUserByEmailAsync(registerDTO.Email);
            if (returnedUser != null)
            {
                return BadRequest("this email is allready exist");
            }
            var user = await _accountServices.RegisterAsync(registerDTO);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [HttpPost]
        [Route("/Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("the model is not valid");
            }
            var user = await _accountServices.GetUserByEmailAsync(loginDTO.Email);
            if (user == null)
            {
                return NotFound("this user is not found");
            }
            var isPasswordCorrect = await _accountServices.CheckPasswordAsync(loginDTO);
            if (!isPasswordCorrect)
            {
                return Unauthorized("data is not correct");
            }
            var token = await _accountServices.GenerateJwtToken(loginDTO.Email);
            var refreshToken = await _accountServices.CreateRandomToken();
            LoginResponseDTO response = new()
            {
                IsLogedIn = true,
                UserName = user.Username,
            };
            var refreshTokenDTO = new TokenModel()
            {
                Token = token,
                RefreshToken = refreshToken
            };
            //add refreshToken to database
            await _accountServices.SetCookie(token, refreshToken, loginDTO.Email);
            await _accountServices.AddRefreshTokenAsync(refreshTokenDTO, loginDTO.Email);
            return Ok(response);
        }


        [HttpGet]
        public async Task<IActionResult> VerifyRegistration(string email, string verificationToken)
        {
            var result = await _accountServices.UserConfirmAsync(email, verificationToken);
            return result ? Ok(result) : NotFound("user not found");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _accountServices.GetUserById(id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost("/RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            string refreshToken = Request.Cookies["X-Refresh-Token"];
            string email = Request.Cookies["X-Email"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Refresh token is missing.");
            }
            var tokenModel = new TokenModel { RefreshToken = refreshToken };
            var result = await _accountServices.RegenerateRefreshToken(tokenModel, email);
            //var claim = await _accountServices.GetTokenPrincipal(tokenModel.Token);
            //var userName = claim.Identity?.Name;

            //var claimsList = claim.Claims.Select(c => c.Value).ToList();
            //var result= await _accountServices.RegenerateRefreshToken(tokenModel, claimsList[2]);
            if (result == null)
            {
                return Unauthorized("Invalid refresh token.");
            }

            // Set new access token and refresh token in cookies
            await _accountServices.SetCookie(result.Token, result.RefreshToken, result.Email);

            return Ok();
        }
    }
}
