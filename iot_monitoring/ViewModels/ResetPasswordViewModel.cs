using System.ComponentModel.DataAnnotations;
namespace iot_monitoring.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Password do not match.")]
        public string ConfirmPassword {  get; set; } = string.Empty;
    }
}
