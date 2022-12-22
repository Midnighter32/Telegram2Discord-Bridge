using Discord;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;

public class DiscordController
{
    private string _token { get; }
    private DiscordSocketClient _client { get; }

    public DiscordController(string token)
    {
        _token = token;
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info,
            WebSocketProvider = WS4NetProvider.Instance,
            GatewayIntents = GatewayIntents.GuildEmojis | GatewayIntents.Guilds
        });
        _client.Ready += Ready;
    }

    public async Task StartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();

        while (!IsReady)
        {
            await Task.Delay(1000);
        }
    }

    public IGuild GetGuild(ulong id)
    {
        return _client.GetGuild(id);
    }

    private Task Ready()
    {
        Console.WriteLine($"Discord Bot: Ready ({_client.CurrentUser.Username} - {_client.CurrentUser.Id})");

        IsReady = true;

        return Task.CompletedTask;
    }

    public bool IsReady { get; private set; } = false;

    public static DiscordController DiscordInstance { get; private set; } = new DiscordController(AuthConfiguration.Default.DiscordToken);
}
