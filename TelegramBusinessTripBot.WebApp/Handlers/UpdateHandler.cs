using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using TelegramBusinessTripBot.WebApp.Services.BackgroundServices;
using TL;
using Message = Telegram.Bot.Types.Message;
using Update = Telegram.Bot.Types.Update;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Channels;
using User = TL.User;
using System;
using System.Threading;
using Telegram.Bot.Requests.Abstractions;
using TelegramBusinessTripBot.WebApp.Entities;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace TelegramBusinessTripBot.WebApp.Handlers;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly WTelegramService _telegramService;
    private readonly TgContext _tgContext;
    private IMemoryCache _memoryCache;
    private long? hash;
    private Dictionary<long, long> _stopBot = [];

    public UpdateHandler(ITelegramBotClient botClient, WTelegramService telegramService, ILogger<UpdateHandler> logger, IMemoryCache memoryCache, TgContext tgContext)
    {
        _botClient = botClient;
        _logger = logger;
        _telegramService = telegramService;
        _memoryCache = memoryCache;
        _tgContext = tgContext;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        hash = _telegramService?.User?.access_hash;
        var handler = update switch
        {
            { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        var user = message?.From;
        var chat = message?.Chat;
        try {
            if (message.Type == Telegram.Bot.Types.Enums.MessageType.ChatMembersAdded)
            {
                var prefixSupergroup = -100;
                var chatId = chat.Id;
                int prefixLength = GetDigitsCount(Math.Abs(prefixSupergroup));
                long divisor = (long)Math.Pow(10, GetDigitsCount(chatId) - prefixLength);
                if (chatId / divisor == prefixSupergroup)
                {
                    long result = chatId % divisor;
                    chatId = Math.Abs(result);
                }
                var getChats = await _tgContext.ChatOrChannels.Where(x => x.ChatOrChannelId == chatId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
                TL.Channel? channel;
                if (getChats == null)
                {
                    var allChats = await _telegramService.Client.Messages_GetAllChats();
                    channel = allChats.chats.FirstOrDefault(x => x.Key == chatId).Value as TL.Channel;
                    if (channel != null)
                    {
                        getChats = new ChatOrChannel { ChatOrChannelId = channel.ID, Hash = channel.access_hash, Title = channel.title };
                        _tgContext.ChatOrChannels.AddOrUpdate(getChats);
                        await _tgContext.SaveChangesAsync(cancellationToken);
                    }
                }
                else
                {
                    var inputChannel = new InputChannel(getChats.ChatOrChannelId, getChats.Hash.Value);
                    var chatFull = await _telegramService.Client.Channels_GetFullChannel(inputChannel);
                    channel = chatFull.chats.FirstOrDefault(x => x.Key == chatId).Value as TL.Channel;
                }

                var allParticipants = await _telegramService.Client.Channels_GetAllParticipants(channel);
                var applicantUser = allParticipants.users.Values.Select(x => x);
                    
                foreach (var appUser in applicantUser)
                {
                    _tgContext.Users.AddOrUpdate(
                    new Users
                    {
                        UserHash = appUser.access_hash,
                        UserId = appUser.ID,
                        UserName = appUser.MainUsername,
                        Phone = appUser.phone,
                        FIO = appUser.first_name + " " + appUser.last_name,
                        ChatOrChannel = new List<ChatOrChannel> { getChats }
                    });
                }
                var save = await _tgContext.SaveChangesAsync();
                return;
            }
            if (message.Type == Telegram.Bot.Types.Enums.MessageType.GroupCreated)
            {
                await _botClient.SendTextMessageAsync(chat.Id, "Этот чат был создан для организации вашей командировки. " +
                    "Сейчас бот задаст Вам несколько уточняющих вопросов, после чего в чат чат будут добавлены travel-менеджеры нашей компании, которые возьмут Вашу Заявку в работу." +
                    "\r\nДля удобства поиска и работы рекомендуем Вам завести у себя в Телеграме папку \"Командировки\", в которую включить данный чат." +
                    "\r\n\r\nЕсли заявка Вам не нужна, то просто напишите стоп - бот удалит этот чат", cancellationToken: cancellationToken);
                var members = await _telegramService.Client.Messages_GetFullChat(chat.Id * -1);
                var haveFioMembers = members.users.Values.Where(x => _memoryCache.TryGetValue("HaveFio " + x.ID, out bool haveFio));
                if (haveFioMembers.Any())
                {
                    await _botClient.SendTextMessageAsync(chat.Id, "Укажите Ваше ФИО");
                    _memoryCache.Set(haveFioMembers.FirstOrDefault()?.ID + " AskFio", true);
                }
            }
            if (message!.Text is not { } messageText)
                return;
            if (messageText == @"/start")
            {
                await _botClient.SendTextMessageAsync(user.Id, "Здравствуйте! Бот запущен");
            }
            if (messageText.ToLower() == "стоп")
            {
                _memoryCache.Remove(user.Id + " Request");
                var members = await _telegramService.Client.Messages_GetFullChat(chat.Id * -1);
                var updatesBase = await _telegramService.Client.DeleteChat(members.chats.Values.FirstOrDefault());
                return;
            }
            var listenExists = _memoryCache.TryGetValue(user.Id + " Listen", out bool listen);
            if (!listenExists && !listen)
            {
                var askFioExists = _memoryCache.TryGetValue(user.Id + " AskFio", out bool askFio);
                _memoryCache.Remove(user.Id + " AskFio");
                if (!askFioExists && !askFio)
                {
                    var userRequest = new UserRequest(messageText, null, null, null, null, null);
                    _memoryCache.Set(user.Id + " Request", userRequest);
                    var members = await _telegramService.Client.Messages_GetFullChat(chat.Id * -1);
                    await _telegramService.Client.EditChatTitle(members.chats.Values.FirstOrDefault(), $"Командировка {messageText}");
                }

                var userRequestExists = _memoryCache.TryGetValue(user.Id + " Request", out UserRequest? request);
                if (!string.IsNullOrWhiteSpace(request?.Fio) && string.IsNullOrWhiteSpace(request?.From))
                {
                    if (_memoryCache.TryGetValue(user.Id + " askFrom", out _))
                    {
                        _memoryCache.Remove(user.Id + " askFrom");
                        _memoryCache.Remove(user.Id + " Request");
                        if (userRequestExists && request != null)
                        {
                            var newRequest = request with { From = messageText };
                            _memoryCache.Set(user.Id + " Request", newRequest);
                            if (!string.IsNullOrWhiteSpace(newRequest?.From) && string.IsNullOrWhiteSpace(newRequest?.To))
                            {
                                var sendMessage = await _botClient.SendTextMessageAsync(chat.Id, "Укажите пункт прибытия. " +
                            "\r\n\r\nЕсли предложенные варианты не подходят, то напишите самостоятельно", replyMarkup: GetKeyboardMarkupWithCities(), cancellationToken: cancellationToken);
                                _memoryCache.Set(user.Id + " askTo", sendMessage.MessageId);
                            }
                            else SendFinalMessage(user, chat, newRequest, cancellationToken);
                        }
                    }
                    else
                    {
                        var sendMessage = await _botClient.SendTextMessageAsync(chat.Id, "Укажите пункт отправления. " +
                            "\r\n\r\nЕсли предложенные варианты не подходят, то напишите самостоятельно", replyMarkup: GetKeyboardMarkupWithCities(), cancellationToken: cancellationToken);
                        _memoryCache.Set(user.Id + " askFrom", sendMessage.MessageId);
                    }
                    return;
                }
                else if (!string.IsNullOrWhiteSpace(request?.From) && string.IsNullOrWhiteSpace(request?.To))
                {
                    if (_memoryCache.TryGetValue(user.Id + " askTo", out _))
                    {
                        _memoryCache.Remove(user.Id + " askTo");
                        _memoryCache.Remove(user.Id + " Request");
                        if (userRequestExists && request != null)
                        {
                            var newRequest = request with { To = messageText };
                            _memoryCache.Set(user.Id + " Request", newRequest);
                            if (!string.IsNullOrWhiteSpace(newRequest?.To) && string.IsNullOrWhiteSpace(newRequest?.DateFrom))
                            {
                                var sendMessage = await _botClient.SendTextMessageAsync(chat.Id, "Укажите дату отправления.", cancellationToken: cancellationToken);
                                _memoryCache.Set(user.Id + " askDateFrom", sendMessage.MessageId);
                            }
                            else SendFinalMessage(user, chat, newRequest, cancellationToken);
                        }
                    }
                    else
                    {
                        _memoryCache.Remove(user.Id + " Request");
                        if (userRequestExists && request != null)
                        {
                            var newRequest = request with { From = messageText };
                            _memoryCache.Set(user.Id + " Request", newRequest);
                            if (!string.IsNullOrWhiteSpace(newRequest?.From) && string.IsNullOrWhiteSpace(newRequest?.To))
                            {
                                var sendMessage = await _botClient.SendTextMessageAsync(chat.Id, "Укажите пункт прибытия. " +
                                "\r\n\r\nЕсли предложенные варианты не подходят, то напишите самостоятельно", replyMarkup: GetKeyboardMarkupWithCities(), cancellationToken: cancellationToken);
                                _memoryCache.Set(user.Id + " askTo", sendMessage.MessageId);
                            }
                            else SendFinalMessage(user, chat, newRequest, cancellationToken);
                        }
                    }
                    return;
                }
                else if (!string.IsNullOrWhiteSpace(request?.To) && string.IsNullOrWhiteSpace(request?.DateFrom))
                {
                    _memoryCache.Remove(user.Id + " Request");
                    if (userRequestExists && request != null)
                    {
                        var newRequest = request with { DateFrom = messageText };
                        _memoryCache.Set(user.Id + " Request", newRequest);
                        _memoryCache.TryGetValue(user.Id + " Request", out request);
                        if (!string.IsNullOrWhiteSpace(newRequest?.DateFrom) && string.IsNullOrWhiteSpace(newRequest?.DateTo))
                        {
                            var sendMessage = await _botClient.SendTextMessageAsync(chat.Id, "Укажите дату возврата.", cancellationToken: cancellationToken);
                            _memoryCache.Set(user.Id + " askDateTo", sendMessage.MessageId);
                        }
                        else SendFinalMessage(user, chat, request, cancellationToken: cancellationToken);
                    }
                    return;
                }
                if ((!string.IsNullOrWhiteSpace(request?.DateFrom) && string.IsNullOrWhiteSpace(request.DateTo))
                    || (!string.IsNullOrWhiteSpace(request?.DateTo) && request!.Request is not bool valueRequest))
                {
                    if (userRequestExists && request != null && string.IsNullOrWhiteSpace(request.DateTo))
                    {
                        _memoryCache.Remove(user.Id + " Request");
                        var newRequest = request with { DateTo = messageText };
                        _memoryCache.Set(user.Id + " Request", newRequest);
                    }
                    SendFinalMessage(user, chat, request, cancellationToken: cancellationToken);
                    return;
                }
            }
            if (message.MessageThreadId == 3)
            {
                var inlineKeyboard = new InlineKeyboardMarkup(
                                        new List<InlineKeyboardButton[]>()
                                        {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Подать заявку на командировку"),
                                        }
                                        });

                await _botClient.SendTextMessageAsync(
                    chat.Id,
                    "Если вы хотите оформить заявку на командировку нажмите на кнопку:", messageThreadId: message.MessageThreadId,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken); // Все клавиатуры передаются в параметр replyMarkup

                return;
            }
        }
        catch (Exception ex)
        {
            await _botClient.SendTextMessageAsync(chat.Id, "Произошла ошибка, обратитесь к администраторам");
        }
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var prefixSupergroup = -100;
        var user = callbackQuery?.From;
        var chat = callbackQuery?.Message?.Chat;
        var chatId = chat.Id;
        var chatIdWOPrefix = chatId;
        int prefixLength = GetDigitsCount(Math.Abs(prefixSupergroup));
        long divisor = (long)Math.Pow(10, GetDigitsCount(chatId) - prefixLength);
        if (chatId / divisor == prefixSupergroup)
        {
            long result = chatId % divisor;
            chatIdWOPrefix = Math.Abs(result);
        }
        if (callbackQuery.Data == "Подать заявку на командировку")
        {
            var title = $"Командировка ";
            var botUser = await _tgContext.Users.FirstOrDefaultAsync(x => x.UserId == 7067834045);
            var inputUsers = await _tgContext.Users.Where(x => x.AdminAccount.Value || x.UserId == user.Id).Select(x => new InputUser(x.UserId.Value, x.UserHash.Value)).ToArrayAsync();
            var haveFio = !string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName);
            title += haveFio ? $"{user.FirstName}_{user.LastName}" : $"{user.Username}";
            var newChat = await _telegramService.Client.Messages_CreateChat(inputUsers, $"Командировка {user.Username}_{user.FirstName}_{user.LastName}");
            var editChannel = await _telegramService.Client.EditChatAdmin(newChat.updates.Chats.Values.FirstOrDefault(), inputUsers.FirstOrDefault(x => x.UserId == 7067834045), true);
            _memoryCache.Set(user.Id + " Listen", true);
            _memoryCache.Set("HaveFio " + user.Id, haveFio);
            return;
        }
        var messageExistsFrom = _memoryCache.TryGetValue(user.Id + " askFrom", out int sendMessageFromId);
        if (messageExistsFrom && sendMessageFromId == callbackQuery?.Message?.MessageId)
        {
            _memoryCache.Remove(user.Id + " askFrom");
            var userRequestExists = _memoryCache.TryGetValue(user.Id + " Request", out UserRequest? request);
            _memoryCache.Remove(user.Id + " Request");
            if(userRequestExists && request != null)
            {
                var newRequest = request with { From = callbackQuery?.Data };
                _memoryCache.Set(user.Id + " Request", newRequest);
                if (!string.IsNullOrWhiteSpace(newRequest?.From) && string.IsNullOrWhiteSpace(newRequest?.To))
                {
                    var sendMessage = await _botClient.SendTextMessageAsync(chatIdWOPrefix, "Укажите пункт прибытия. " +
                    "\r\n\r\nЕсли предложенные варианты не подходят, то напишите самостоятельно", replyMarkup: GetKeyboardMarkupWithCities(), cancellationToken: cancellationToken);
                    _memoryCache.Set(user.Id + " askTo", sendMessage.MessageId);
                }
                else SendFinalMessage(user, callbackQuery?.Message?.Chat, newRequest, cancellationToken);
            }
            return;
        }
        var messageExistsTo = _memoryCache.TryGetValue(user.Id + " askTo", out int sendMessageToId);
        if (messageExistsTo && sendMessageToId == callbackQuery?.Message?.MessageId)
        {
            _memoryCache.Remove(user.Id + " askTo");
            var userRequestExists = _memoryCache.TryGetValue(user.Id + " Request", out UserRequest? request);
            _memoryCache.Remove(user.Id + " Request");
            if (userRequestExists && request != null)
            {
                var newRequest = request with { To = callbackQuery?.Data };
                _memoryCache.Set(user.Id + " Request", newRequest);
                if (!string.IsNullOrWhiteSpace(newRequest?.To) && string.IsNullOrWhiteSpace(newRequest?.DateFrom))
                {
                    var sendMessage = await _botClient.SendTextMessageAsync(chatIdWOPrefix, "Укажите дату отправления.", cancellationToken: cancellationToken);
                    _memoryCache.Set(user.Id + " askDateFrom", sendMessage.MessageId);
                }
                else SendFinalMessage(user, callbackQuery?.Message?.Chat, newRequest, cancellationToken);
            }
            return;
        }
        var messageExistsRequest = _memoryCache.TryGetValue(user.Id + " request", out int sendMessageRequest);
        if (messageExistsRequest && sendMessageRequest == callbackQuery?.Message?.MessageId)
        {
            _memoryCache.Remove(user.Id + " request");

            var userRequestExists = _memoryCache.TryGetValue(user.Id + " Request", out UserRequest? request);
            if (userRequestExists && request != null)
            {
                switch (callbackQuery?.Data)
                {
                    case "Всё верно. Создать заявку":
                        var inputUsers = await _tgContext.Users.Where(x => x.TrevelAccount.Value).Select(x => new InputUser(x.UserId.Value, x.UserHash.Value)).ToArrayAsync();
                        var getChats = await _tgContext.ChatOrChannels.Where(x => x.ChatOrChannelId == chatIdWOPrefix).FirstOrDefaultAsync(cancellationToken: cancellationToken);
                        var inputChannel = new InputChannel(getChats.ChatOrChannelId, getChats.Hash.Value);
                        foreach (var inputUser in inputUsers)
                        {
                            var addUser = await _telegramService.Client.AddChatUser(inputChannel, inputUser);
                        }
                        var sendMessage = await _botClient.SendTextMessageAsync(chatIdWOPrefix, "Ваша заявка зарегистрирована." +
                            "\r\nДальнейшие коммуникации по заявке будут вести travel-менеджеры", cancellationToken: cancellationToken);
                        _memoryCache.Remove(user.Id + " Listen");
                        break;
                    case "Исправить пункт отправления":
                        _memoryCache.Remove(user.Id + " Request");
                        var newRequest = request with { From = null };
                        _memoryCache.Set(user.Id + " Request", newRequest);
                        sendMessage = await _botClient.SendTextMessageAsync(chatIdWOPrefix, "Укажите пункт отправления. " +
                        "\r\n\r\nЕсли предложенные варианты не подходят, то напишите самостоятельно", replyMarkup: GetKeyboardMarkupWithCities(), cancellationToken: cancellationToken);
                        _memoryCache.Set(user.Id + " askFrom", sendMessage.MessageId);
                        break;
                    case "Исправить пункт назначения":
                        _memoryCache.Remove(user.Id + " Request");
                        newRequest = request with { To = null };
                        _memoryCache.Set(user.Id + " Request", newRequest);
                        sendMessage = await _botClient.SendTextMessageAsync(chatIdWOPrefix, "Укажите пункт прибытия. " +
                    "\r\n\r\nЕсли предложенные варианты не подходят, то напишите самостоятельно", replyMarkup: GetKeyboardMarkupWithCities(), cancellationToken: cancellationToken);
                        _memoryCache.Set(user.Id + " askTo", sendMessage.MessageId);
                        break;
                    case "Исправить дату отправления":
                        _memoryCache.Remove(user.Id + " Request");
                        newRequest = request with { DateFrom = null };
                        _memoryCache.Set(user.Id + " Request", newRequest);
                        sendMessage = await _botClient.SendTextMessageAsync(chatIdWOPrefix, "Укажите дату отправления.", cancellationToken: cancellationToken);
                        _memoryCache.Set(user.Id + " askDateFrom", sendMessage.MessageId);
                        break;
                    case "Исправить дату возврата":
                        _memoryCache.Remove(user.Id + " Request");
                        newRequest = request with { DateTo = null };
                        _memoryCache.Set(user.Id + " Request", newRequest);
                        sendMessage = await _botClient.SendTextMessageAsync(chatIdWOPrefix, "Укажите дату возврата.", cancellationToken: cancellationToken);
                        _memoryCache.Set(user.Id + " askDateTo", sendMessage.MessageId);
                        break;
                    case "Отменить заявку":
                        _memoryCache.Remove(user.Id + " Request");
                        getChats = await _tgContext.ChatOrChannels.Where(x => x.ChatOrChannelId == chatIdWOPrefix).FirstOrDefaultAsync(cancellationToken: cancellationToken);
                        inputChannel = new InputChannel(getChats.ChatOrChannelId, getChats.Hash.Value);
                        var updatesBase = await _telegramService.Client.DeleteChat(inputChannel);
                        break;
                }
            }
            return;
        }

    }
    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    private async Task<User?> GetUserByUserNameAndCompareById(string userName, long id)
    {
        var applicantUserSearch = await _telegramService.Client.Contacts_Search(userName);
        return applicantUserSearch.users.Values.FirstOrDefault(x => x.ID == id);
    }

    private InlineKeyboardMarkup GetKeyboardMarkupWithCities()
    {
        return new InlineKeyboardMarkup(
                                    new List<InlineKeyboardButton[]>()
                                    {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Санкт-Петербург")
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Москва"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Саранск"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Гродно"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Ростов-на-Дону"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Екатеринбург"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Новосибирск"),
                                        }
                                    });
    }
    private InlineKeyboardMarkup GetKeyboardMarkupWithRequest()
    {
        return new InlineKeyboardMarkup(
                                        new List<InlineKeyboardButton[]>()
                                        {
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Всё верно. Создать заявку"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Исправить пункт отправления"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Исправить пункт назначения"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Исправить дату отправления"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Исправить дату возврата"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Отменить заявку")
                                        }
                                        });
    }
    private async void SendFinalMessage(Telegram.Bot.Types.User user, Telegram.Bot.Types.Chat chat, UserRequest request, CancellationToken cancellationToken)
    {
        _memoryCache.TryGetValue(user.Id + " Request", out request);
        var sendMessage = await _botClient.SendTextMessageAsync(chat.Id, "Проверьте информацию для подачи заявки." +
            $"\r\n\r\n{request.Fio}" +
            $"\r\nПункт отправления: {request.From}" +
            $"\r\nПункт назначения: {request.To}" +
        $"\r\nДата отправления: {request.DateFrom}" +
            $"\r\nДата возврата: {request.DateTo}", replyMarkup: GetKeyboardMarkupWithRequest(), cancellationToken: cancellationToken);
        _memoryCache.Set(user.Id + " request", sendMessage.MessageId);
    }

    private int GetDigitsCount(long number)
    {
        // Функция для получения количества цифр в числе
        number = Math.Abs(number);
        return (int)Math.Floor(Math.Log10(number) + 1);
    }
}

public record UserRequest(string? Fio, string? From, string? To, string? DateFrom, string? DateTo, bool? Request);
