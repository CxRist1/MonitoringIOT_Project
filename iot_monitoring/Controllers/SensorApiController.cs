using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace iot_monitoring.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetSensorData()
        {
            var random = new Random();
            var data = new
            {
                Temperature = Math.Round(25 + random.NextDouble() * 10, 1),
                Humidity = random.Next(50, 80),
                soilMoisture = random.Next(30, 70),
                status = "Onine",
                time = DateTime.Now.ToString("HH:mm:ss")
            };
            return Ok(data);
        }
    }

}
