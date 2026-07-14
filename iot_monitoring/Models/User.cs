using System.Globalization;

namespace iot_monitoring.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
        public string? ResetPasswordToken { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? ResetPasswordTokenExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Cart? Cart { get; set; }
    }
}