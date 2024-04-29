using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Username { get; set; } 
        [Required]
        public byte[] PasswordHash { get; set; } = new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];
        public string VerificationToken { get; set; } = string.Empty;
        public DateTime? VerifiedDate { get; set; }

        public string ResetPasswordToken { get; set; } = string.Empty;
        public DateTime? RestPasswordTokenExpired { get; set; }

        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpired { get; set; }
        public string Roles { get; set; } = UserRoles.Member.ToString();

    }
    public enum UserRoles
    {
        Admin,
        SpecificMember,
        Member
    }
}
