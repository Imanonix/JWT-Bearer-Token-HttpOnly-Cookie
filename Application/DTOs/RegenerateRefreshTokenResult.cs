namespace Application.DTOs
{
    public class RegenerateRefreshTokenResult
    {
        public bool IsLogedIn { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
