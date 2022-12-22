using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Services
{
    public class EmojiService : IEmojiService
    {
        private const string _storageFile = "storage";

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmojiService> _logger;
        private readonly IDiscordService _discord;
        private readonly IConfigurationService _config;

        private JToken _storage { get; }

        public EmojiService(IServiceProvider provider)
        {
            _serviceProvider = provider;
            _logger = _serviceProvider.GetService<ILogger<EmojiService>>();
            _discord = _serviceProvider.GetService<IDiscordService>();
            _config = _serviceProvider.GetService<IConfigurationService>();

            _logger.LogInformation($"[ {nameof(EmojiService)} ] Initialized");

            _logger.LogInformation( "Storages:\n\t" + 
                string.Join("\n\t", _config.GetValue<ulong[]>("Discord.EmojiStorages")
                .Select(x =>
                {
                    var guild = _discord.GetGuild(x);
                    Storages.Add(guild);

                    return $"{guild.Name}: {guild.Id}";
                })));

            if (File.Exists(_storageFile))
            {
                using (FileStream fs = File.OpenRead(_storageFile))
                {
                    byte[] data = new byte[fs.Length];

                    fs.Read(data, 0, data.Length);

                    var str = Encoding.Default.GetString(data);
                    _storage = JToken.Parse(str);
                }
            }
            else
            {
                /* Default Settings */
                _storage = JToken.Parse("{\"0\":{\"TelegramId\":0,\"DiscordId\":0}}");
            }
        }

        public async Task AddEmojiAsync(long id, byte[] data)
        {
            foreach (var storage in Storages)
            {
                if (storage.Emotes.Count < 50)
                {
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        var emote = await storage.CreateEmoteAsync(id.ToString(), new Image(ms));

                        _storage[id.ToString()] = JObject.FromObject(new { TelegramId = id, DiscordId = emote.Id });
                    }

                    return;
                }
            }
        }

        public string GetEmoji(long id)
        {
            var emoji = _storage.SelectToken(id.ToString());

            if (emoji is null)
                return string.Empty;

            return $"<:{emoji["TelegramId"]}:{emoji["DiscordId"]}>";
        }

        public bool ContainsEmoji(long id)
        {
            var emoji = _storage.SelectToken(id.ToString());

            return emoji is not null;
        }

        public void Save()
        {
            byte[] data = Encoding.Default.GetBytes(ToString());
            using (FileStream fs = File.Exists(_storageFile)
                ? File.OpenWrite(_storageFile)
                : File.Create(_storageFile))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        public override string ToString()
        {
            return _storage.ToString();
        }

        private List<IGuild> Storages { get; } = new List<IGuild>();
    }
}
