using System.ComponentModel.DataAnnotations;

namespace iot_monitoring.ViewModels
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อผู้ใช้")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอก E-mail")]
        [EmailAddress(ErrorMessage = "รูปแบบ E-mail ไม่ถูกต้อง")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";

        public bool IsActive { get; set; } = true;
    }
}