using TelegramBusinessTripBot.WebApp.Abstract;

namespace TelegramBusinessTripBot.WebApp.Services;

public class PollingService : PollingServiceBase<ReceiverService>
{
    public PollingService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<PollingService> logger)
        : base(serviceProvider, configuration, logger)
    {
    }
}