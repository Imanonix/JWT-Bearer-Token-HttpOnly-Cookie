
namespace Application.ModelServices
{
    public class JWTSetting
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } =string.Empty;
        public string Key { get; set; } = string.Empty;
    }
}
