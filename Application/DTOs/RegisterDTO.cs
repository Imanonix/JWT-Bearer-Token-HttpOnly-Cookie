using System.ComponentModel.DataAnnotations;


namespace Application.DTOs
{
    public class RegisterDTO
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage ="Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email is not valid")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,20}$", ErrorMessage = "Password must be between 8 and 20 characters and include at least one digit, one uppercase letter, and one lowercase letter.")]
        public string Password { get; set; } = string.Empty;
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string Repassword { get; set; } = string.Empty;

    }
}
