using Application.DTOs;
using Application.ModelServices;
using System.Security.Claims;


namespace Application.Services.Interfaces
{
    public interface IAccountServices
    {
        Task<RegisterDTO> RegisterAsync(RegisterDTO registerDTO);
        Task<RegisterDTO> GetUserByEmailAsync(string email);
        Task<RegisterDTO> GetUserById(Guid id);
        Task SendVerificationEmailAsync(EmailRequest emailRequest);
        Task<bool> UserConfirmAsync(string email, string verificationToken);
        Task<bool> CheckPasswordAsync(LoginDTO loginDTO);
        Task<string> GenerateJwtToken(string email);
        Task AddRefreshTokenAsync(TokenModel refreshTokenDTO, string Email);
        Task<Tuple<byte[], byte[]>> CreatePasswordHash(string password);  // tuple<passwordSalt, passwordHash>
        Task<string> CreateRandomToken();
        Task<ClaimsPrincipal> GetTokenPrincipal(string token);
        Task<RegenerateRefreshTokenResult> RegenerateRefreshToken(TokenModel tokenModel, string email);

        Task SetCookie(string token, string refreshToken, string email);
    }
}
