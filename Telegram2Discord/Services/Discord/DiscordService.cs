using Discord;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Test.DataTypes;

namespace Test.Services
{
    public class DiscordService : IDiscordService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DiscordService> _logger;
        private readonly IConfigurationService _configuration;

        private DiscordSocketClient _client { get; }

        public DiscordService(IServiceProvider provider)
        {
            _serviceProvider = provider;
            _logger = _serviceProvider.GetService<ILogger<DiscordService>>();
            _configuration = _serviceProvider.GetService<IConfigurationService>();

            _logger.LogInformation($"[ {nameof(DiscordService)} ] Initialized");

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                WebSocketProvider = WS4NetProvider.Instance,
                GatewayIntents = GatewayIntents.GuildEmojis | GatewayIntents.Guilds
            });
            _client.Log += Log;

            StartAsync().Wait();

            _logger.LogInformation($"[ {nameof(DiscordService)} ] Ready ({_client.CurrentUser.Username} - {_client.CurrentUser.Id})");
        }

        private Task Log(LogMessage arg)
        {
            _logger.LogTrace($"[ {nameof(DiscordService)} ] Log: {arg.Message}");

            return Task.CompletedTask;
        }

        public IGuild GetGuild(ulong id)
        {
            return _client.GetGuild(id);
        }

        public async void SendMessage(News post)
        {
            foreach (var channels in _configuration.GetValue<ulong[]>("Discord.NewsChannels")
                .Select(x => _client.GetChannel(x) as ITextChannel))
            {
                if (post.Embeds.Count == 0)
                    await channels.SendMessageAsync(post.Formatted);
                else
                {
                    List<Stream> streams = new List<Stream>();
                    IEnumerable<FileAttachment> attachments = post.Embeds.ConvertAll((img) =>
                    {
                        MemoryStream ms = new MemoryStream(img.Data);
                        streams.Add(ms);

                        return new FileAttachment(ms, img.Name);
                    });

                    await channels.SendFilesAsync(attachments, post.Formatted);
                }
            }
        }

        private async Task StartAsync()
        {
            var token = _configuration.GetValue<string>("Discord.Token");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            CancellationTokenSource cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<object>();

            Func<Task> callback = () =>
            {
                tcs.SetResult(null);

                return Task.CompletedTask;
            };

            _client.Ready += callback;

            try
            {
                await tcs.Task.WaitAsync(cts.Token);
            }
            finally
            {
                _client.Ready -= callback;
            }
        }
    }
}
