using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iot_monitoring.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/line/webhook")]
    public class LineWebhookController : ControllerBase
    {
        private readonly ILogger<LineWebhookController> _logger;

        public LineWebhookController(
            ILogger<LineWebhookController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();

            using var document = JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty(
                    "events",
                    out var events))
            {
                return Ok();
            }

            foreach (var lineEvent in events.EnumerateArray())
            {
                if (!lineEvent.TryGetProperty(
                        "source",
                        out var source))
                {
                    continue;
                }

                var sourceType = source
                    .GetProperty("type")
                    .GetString();

                if (sourceType != "group")
                {
                    continue;
                }

                var groupId = source
                    .GetProperty("groupId")
                    .GetString();

                _logger.LogInformation(
                    "LINE Group ID: {GroupId}",
                    groupId
                );
            }

            return Ok();
        }
    }
}