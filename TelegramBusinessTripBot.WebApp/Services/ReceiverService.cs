using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;
using TelegramBusinessTripBot.WebApp.Abstract;
using TelegramBusinessTripBot.WebApp.Handlers;

namespace TelegramBusinessTripBot.WebApp.Services;

public class ReceiverService : ReceiverServiceBase<UpdateHandler>
{
    public ReceiverService(
        ITelegramBotClient botClient,
        UpdateHandler updateHandler,
        IConfiguration configuration,
        ILogger<ReceiverServiceBase<UpdateHandler>> logger,
        IMemoryCache memoryCache)
        : base(botClient,  updateHandler, configuration, logger, memoryCache)
    {
    }
}