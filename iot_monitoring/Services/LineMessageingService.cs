using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace iot_monitoring.Services
{
    public class LineMessagingService : ILineMessagingService
    {
        private const string PushMessageEndpoint =
            "https://api.line.me/v2/bot/message/push";

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LineMessagingService> _logger;

        public LineMessagingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<LineMessagingService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendMessageAsync(
            string message,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "LINE message cannot be empty.",
                    nameof(message));
            }

            var channelAccessToken =
                _configuration["LineMessaging:ChannelAccessToken"];

            var groupId =
                _configuration["LineMessaging:GroupId"];

            if (string.IsNullOrWhiteSpace(channelAccessToken))
            {
                throw new InvalidOperationException(
                    "LINE Channel Access Token is not configured.");
            }

            if (string.IsNullOrWhiteSpace(groupId))
            {
                throw new InvalidOperationException(
                    "LINE Group ID is not configured.");
            }

            var requestBody = new
            {
                to = groupId,
                messages = new[]
                {
                    new
                    {
                        type = "text",
                        text = message
                    }
                }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                PushMessageEndpoint);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    channelAccessToken);

            request.Content = JsonContent.Create(requestBody);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "LINE message sent successfully.");

                return;
            }

            var responseBody = await response.Content
                .ReadAsStringAsync(cancellationToken);

            _logger.LogError(
                "LINE API returned {StatusCode}. Response: {ResponseBody}",
                (int)response.StatusCode,
                responseBody);

            throw new HttpRequestException(
                $"LINE API request failed with status {(int)response.StatusCode}.");
        }
    }
}