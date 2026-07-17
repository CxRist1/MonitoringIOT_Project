using System.ComponentModel.DataAnnotations;

namespace iot_monitoring.ViewModels
{
    public class ConfirmPurchaseViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อผู้รับ")]
        [StringLength(
            150,
            ErrorMessage = "ชื่อผู้รับต้องไม่เกิน 150 ตัวอักษร")]
        [Display(Name = "ชื่อผู้รับ")]
        public string RecipientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
        [StringLength(
            20,
            ErrorMessage = "เบอร์โทรต้องไม่เกิน 20 ตัวอักษร")]
        [Phone(ErrorMessage = "รูปแบบเบอร์โทรไม่ถูกต้อง")]
        [Display(Name = "เบอร์โทร")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกที่อยู่จัดส่ง")]
        [StringLength(
            500,
            ErrorMessage = "ที่อยู่ต้องไม่เกิน 500 ตัวอักษร")]
        [Display(Name = "ที่อยู่จัดส่ง")]
        public string ShippingAddress { get; set; } = string.Empty;
    }
}