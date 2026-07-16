using iot_monitoring.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iot_monitoring.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LineTestController : Controller
    {
        private readonly ILineMessagingService _lineMessagingService;
        private readonly ILogger<LineTestController> _logger;

        public LineTestController(
            ILineMessagingService lineMessagingService,
            ILogger<LineTestController> logger)
        {
            _lineMessagingService = lineMessagingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Send(
            CancellationToken cancellationToken)
        {
            try
            {
                await _lineMessagingService.SendMessageAsync(
                    """
                    ✅ IoT Monitoring Dashboard

                    เชื่อมต่อ LINE Group Notification สำเร็จ
                    """,
                    cancellationToken);

                return Content(
                    "LINE test message sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "LINE test message failed.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "LINE test message failed. Check Visual Studio Output.");
            }
        }
    }
}