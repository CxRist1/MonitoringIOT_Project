using System.ComponentModel.DataAnnotations;

namespace iot_monitoring.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
