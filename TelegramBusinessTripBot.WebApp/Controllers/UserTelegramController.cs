using Microsoft.AspNetCore.Mvc;
using TelegramBusinessTripBot.WebApp.Abstract;
using TelegramBusinessTripBot.WebApp.Handlers;
using TelegramBusinessTripBot.WebApp.Services;
using TelegramBusinessTripBot.WebApp.Services.BackgroundServices;

namespace TelegramBusinessTripBot.WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class UserTelegramController : ControllerBase
{
    private readonly WTelegramService WT;
    public UserTelegramController(WTelegramService wt) => WT = wt;

    [HttpGet("status")]
    public ContentResult Status()
    {
        switch (WT.ConfigNeeded)
        {
            case "connecting": return Content("<meta http-equiv=\"refresh\" content=\"1\">WTelegram is connecting...", "text/html");
            case null: return Content($@"Connected as {WT.User}<br/><a href=""chats"">Get all chats</a>", "text/html");
            default: return Content($@"Enter {WT.ConfigNeeded}: <form action=""config""><input name=""value"" autofocus/></form>", "text/html");
        }
    }

    [HttpGet("config")]
    public async Task<ActionResult> Config(string value)
    {
        await WT.DoLogin(value);
        return Redirect("status");
    }
}
