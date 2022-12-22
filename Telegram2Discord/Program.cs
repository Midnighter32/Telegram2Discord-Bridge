using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Test.DataTypes;
using Test.Services;

public class Program
{
    public static void Main(string[] args) => new Program().Start(args);

    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageParserService _messageParser;
    private readonly IDiscordService _discordService;

    public Program()
    {
        _serviceProvider = ConfigureServices()
            .BuildServiceProvider();

        _messageParser = _serviceProvider.GetService<IMessageParserService>();
        _discordService = _serviceProvider.GetService<IDiscordService>();
    }

    public IServiceCollection ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
            loggingBuilder.SetMinimumLevel(LogLevel.Debug);
        });

        services.AddSingleton<IMessageParserService, MessageParserService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<ITelegramService, TelegramService>();
        services.AddSingleton<IEmojiService, EmojiService>();
        services.AddSingleton<IDiscordService, DiscordService>();

        return services;
    }

    private void Start(string[] args)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<object>();

        _messageParser.PostReady += MessageParserService_PostReady;

        tcs.Task.Wait(cts.Token);

        _serviceProvider.GetService<IConfigurationService>().Save();
    }

    private void MessageParserService_PostReady(News news)
    {
        _discordService.SendMessage(news);
    }
}