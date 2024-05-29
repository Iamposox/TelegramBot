namespace TelegramBusinessTripBot.WebApp.Abstract;

public interface IReceiverService
{
    Task ReceiveAsync(CancellationToken stoppingToken);
}
