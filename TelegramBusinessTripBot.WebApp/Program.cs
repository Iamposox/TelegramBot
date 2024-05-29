using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TelegramBusinessTripBot.WebApp.Configuration;
using TelegramBusinessTripBot.WebApp.Entities;
using TelegramBusinessTripBot.WebApp.Handlers;
using TelegramBusinessTripBot.WebApp.Services;
using TelegramBusinessTripBot.WebApp.Services.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

DatabaseConfiguration databaseConfiguration = new();
builder.Configuration.Bind(databaseConfiguration);

builder.Services.AddDbContext<TgContext>(options =>
{
    options
        .UseSqlServer(databaseConfiguration.ConnectionString, sqlOptions =>
        {
            sqlOptions.CommandTimeout(3600);
        });
    options.UseLazyLoadingProxies();
});
builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp)=>
    {
        BotConfiguration botConfiguration = new ();
        builder.Configuration.Bind(botConfiguration);
        TelegramBotClientOptions options = new(botConfiguration.Token_Bot);
        return new TelegramBotClient(options, httpClient);
    });

builder.Services.AddMemoryCache();
builder.Services.AddHostedService(provider => provider.GetService<WTelegramService>());
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<ReceiverService>();
builder.Services.AddSingleton<WTelegramService>();
builder.Services.AddHostedService<PollingService>();
builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints => endpoints.MapControllers());
await app.RunAsync();