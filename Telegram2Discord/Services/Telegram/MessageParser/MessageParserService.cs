using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Test.DataTypes;
using TL;

namespace Test.Services
{
    public class MessageParserService : IMessageParserService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageParserService> _logger;
        private readonly IConfigurationService _configuration;
        private readonly ITelegramService _telegram;
        private readonly IEmojiService _emoji;

        private Task NewsUpdater { get; }

        public MessageParserService(IServiceProvider provider)
        {
            _serviceProvider = provider;
            _logger = _serviceProvider.GetService<ILogger<MessageParserService>>();
            _configuration = _serviceProvider.GetService<IConfigurationService>();
            _telegram = _serviceProvider.GetService<ITelegramService>();
            _emoji = _serviceProvider.GetService<IEmojiService>();

            _logger.LogInformation($"[ {nameof(MessageParserService)} ] Initialized");

            NewsUpdater = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    foreach (var key in NewsPosts.Keys)
                    {
                        NewsPosts.TryGetValue(key, out var news);

                        if ((DateTime.Now - news.LastUpdate).TotalSeconds > 3)
                        {
                            PostReady?.Invoke(news);

                            NewsPosts.TryRemove(key, out _);
                        }
                    }

                    await Task.Delay(1000);
                }
            });

            _telegram.MessageRecieved += Telegram_MessageRecieved;
        }

        public event Action<News> PostReady;

        private async void Telegram_MessageRecieved(Message message)
        {
            if ((!IsFromNewsChannel(message) || !IsSupportedTag(message)) && !NewsPosts.ContainsKey(message.grouped_id)) return;

            long id = NewsPosts.Count;

            if (message.flags.HasFlag(Message.Flags.has_grouped_id))
                id = message.grouped_id;

            if (!NewsPosts.ContainsKey(id))
                NewsPosts.TryAdd(id, await CreateNewsFromMessageAsync(message));
            else NewsPosts[id].Update();

            if (message.media is not MessageMediaDocument 
                && message.media is not MessageMediaPhoto)
                return;

            var doc = await _telegram.DownloadPhotoAsync(message.media);
            NewsPosts[id].Embeds.Add((doc.Name, doc.Data));
        }

        private bool IsFromNewsChannel(Message msg)
        {
            return _configuration.GetValue<long[]>("Telegram.NewsChannels").Contains(msg.Peer.ID);
        }

        private bool IsSupportedTag(Message msg)
        {
            int idx = msg.message.LastIndexOf('#');
            if (idx == -1) return false;

            var tag = msg.message.Substring(idx + 1);

            return _configuration.GetValue<string[]>("Telegram.NewsTags").Contains(tag);
        }

        private async Task<News> CreateNewsFromMessageAsync(Message msg)
        {
            News news = new News(msg);

            if (msg.entities is null) return news;

            var unkownEmojis = msg.entities
                .OfType<MessageEntityCustomEmoji>()
                .Where(x => !_emoji.ContainsEmoji(x.document_id))
                .Select(x => x.document_id);

            foreach (var item in await _telegram.DownloadEmojisAsync(unkownEmojis))
            {
                await _emoji.AddEmojiAsync(item.Key, item.Value);
            }
            _emoji.Save();

            news.Formatted = news.Original.ApplyRules(msg.entities, _emoji);

            return news;
        }

        public void Dispose()
        {
            NewsUpdater.Dispose();
        }

        private ConcurrentDictionary<long, News> NewsPosts { get; } = new ConcurrentDictionary<long, News>();
    }
}
