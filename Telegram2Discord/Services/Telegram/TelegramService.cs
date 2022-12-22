using Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TL;
using WTelegram;

namespace Test.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelegramService> _logger;
        private readonly IConfigurationService _config;

        private Client _client { get; }

        public TelegramService(IServiceProvider provider)
        {
            _serviceProvider = provider;
            _logger = _serviceProvider.GetService<ILogger<TelegramService>>();
            _config = _serviceProvider.GetService<IConfigurationService>();

            _logger.LogInformation($"[ {nameof(TelegramService)} ] Initialized");

            Helpers.Log = Log;

            int app_id = _config.GetValue<int>("Telegram.AppId");
            string api_hash = _config.GetValue<string>("Telegram.ApiHash");

            _client = new Client(app_id, api_hash, "WTelegram.session");
            _client.OnUpdate += OnUpdate;

            string _loginInfo = _config.GetValue<string>("Telegram.PhoneNumber");

            while (_client.User is null)
            {
                switch (_client.Login(_loginInfo).GetAwaiter().GetResult())
                {
                    case "verification_code": Console.Write("Code: "); _loginInfo = Console.ReadLine() ?? "000000"; break;
                }
            }

            _logger.LogInformation($"[ {nameof(TelegramService)} ] Ready ({_client.User.first_name} - {_client.User.ID})");
        }

        public event Action<Message> MessageRecieved;

        private Task OnUpdate(IObject arg)
        {
            if (arg is not UpdatesBase updates)
                return Task.CompletedTask;

            foreach (var update in updates.UpdateList)
            {
                if (update is not UpdateNewMessage unm) continue;
                if (unm.message is not Message m) continue;

                MessageRecieved?.Invoke(m);
            }

            return Task.CompletedTask;
        }

        private void Log(int _, string message)
        {
            _logger.LogTrace($"[ {nameof(TelegramService)} ] Log: {message}");
        }

        private async Task<IEnumerable<Document>> GetEmojiDocumentsAsync(IEnumerable<long> ids)
        {
            var documents = await _client.Messages_GetCustomEmojiDocuments(ids.ToArray());

            return documents.OfType<Document>();
        }

        private async Task<(string Type, byte[] Data)> DownloadDocumentAsync(Document document)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var type = await _client.DownloadFileAsync(document, ms);

                return (type, ms.ToArray());
            }
        }

        private async Task<(string type, Bitmap bmp)> DownloadAsync(Document document)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var result = await DownloadDocumentAsync(document);

                return (result.Type, WebP.Load(result.Data));
            }
        }

        public async Task<(string Name, byte[] Data)> DownloadPhotoAsync(MessageMedia media)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var name = string.Empty;

                switch (media)
                {
                    case MessageMediaDocument doc:
                        await _client.DownloadFileAsync(doc.document as Document, ms);
                        name = (doc.document as Document).Filename;
                        break;
                    case MessageMediaPhoto photo:
                        var type = await _client.DownloadFileAsync(photo.photo as Photo, ms);
                        name = $"{photo.photo.ID}.{type}";
                        break;
                    default: break;
                }

                return (name, ms.ToArray());
            }
        }

        public async Task<IDictionary<long, byte[]>> DownloadEmojisAsync(IEnumerable<long> ids)
        {
            Dictionary<long, byte[]> newEmojis = new Dictionary<long, byte[]>();

            foreach (var item in await GetEmojiDocumentsAsync(ids))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    var result = await DownloadAsync(item);
                    result.bmp.Save(ms, ImageFormat.Png);

                    newEmojis.Add(item.ID, ms.ToArray());
                }
            }

            return newEmojis;
        }
    }
}
