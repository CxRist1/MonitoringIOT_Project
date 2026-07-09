using System.ComponentModel.DataAnnotations;

namespace iot_monitoring.Models
{
    public class Device
    {
        public int Id { get; set; }

        [Required]
        public string DeviceName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        public bool IsConnected { get; set; } = true;
    }
}
