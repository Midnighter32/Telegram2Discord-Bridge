using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace Test.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private const string _configurationFile = "config";

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DiscordService> _logger;

        private JToken _configuration { get; }

        public ConfigurationService(IServiceProvider provider)
        {
            _serviceProvider = provider;
            _logger = _serviceProvider.GetService<ILogger<DiscordService>>();

            _logger.LogInformation($"[ {nameof(ConfigurationService)} ] Initialized");

            if (File.Exists(_configurationFile))
            {
                using (FileStream fs = File.OpenRead(_configurationFile))
                {
                    byte[] data = new byte[fs.Length];

                    fs.Read(data, 0, data.Length);

                    var str = Encoding.Default.GetString(data);
                    _configuration = JToken.Parse(str);
                }
            }
            else
            {
                /* Default Settings */
                _configuration = JToken.FromObject(new
                {
                    Discord = new {
                        Token = "0", // token of your bot
                        EmojiStorages = new ulong[] {
                            0, // guild-id
                            0, // guild-id
                            0, // guild-id
                            0, // guild-id
                            0, // guild-id
                        },
                        NewsChannels = new long[]
                        {
                            0 // channel-id
                        }
                    },
                    Telegram = new {
                        AppId = 0, // telegram app id
                        ApiHash = "0", // telegram api hash
                        PhoneNumber = "0", // phone number of your accaunt
                        NewsChannels = new long[] {
                            0, // telegram channel id
                            0, // telegram channel id
                        },
                        NewsTags = new string[] {
                            "Новости",
                            "Арты",
                            "События",
                            "Сливы",
                            "Интересное",
                            "Будущее",
                            "Гайды",
                            "Промокоды",
                            "Тест"
                        }
                    },

                });
                Save();
            }
        }

        public T GetValue<T>(string key)
        {
            var value = _configuration.SelectToken(key);
            return value.ToObject<T>();
        }

        public void SetValue<T>(string key, T value)
        {
            _configuration[key] = JToken.FromObject(value);

            Save();
        }

        public void Save()
        {
            byte[] data = Encoding.Default.GetBytes(ToString());
            using (FileStream fs = File.Exists(_configurationFile)
                ? File.OpenWrite(_configurationFile)
                : File.Create(_configurationFile))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        public override string ToString()
        {
            return _configuration.ToString();
        }
    }
}
