namespace iot_monitoring.Services
{
    public interface ILineMessagingService
    {
        Task SendMessageAsync(
            string message,
            CancellationToken cancellationToken = default);
    }
}
