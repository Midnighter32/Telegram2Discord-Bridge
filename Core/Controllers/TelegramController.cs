using TL;
using WTelegram;

public class TelegramController
{
    private string _loginInfo { get; set; }
    private Client _client { get; }

    public TelegramController(int app_id, string api_hash, string loginInfo)
    {
        _loginInfo = loginInfo;
        Helpers.Log = (l, s) => System.Diagnostics.Debug.WriteLine(s);
        _client = new Client(app_id, api_hash, "WTelegram.session");
        _client.OnUpdate += OnUpdate;
    }

    public async Task StartAsync()
    {
        while (_client.User is null)
        {
            switch (await _client.Login(_loginInfo))
            {
                case "verification_code": Console.Write("Code: "); _loginInfo = Console.ReadLine() ?? "000000"; break;
            }
        }
        _ = Ready();
    }

    public static IEnumerable<long> GetEmojisFromMessage(Message msg)
    {
        return msg.entities.OfType<MessageEntityCustomEmoji>().Select(x => x.document_id);
    }

    public async Task<IEnumerable<Document>> GetEmojiDocumentsAsync(IEnumerable<long> ids)
    {
        var documents = await _client.Messages_GetCustomEmojiDocuments(ids.ToArray());

        return documents.OfType<Document>();
    }

    public async Task<(string Type, byte[] Data)> DownloadPhotoAsync(Photo photo)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            var type = await _client.DownloadFileAsync(photo, ms);

            return (type.ToString(), ms.ToArray());
        }
    }

    public async Task<(string Type, byte[] Data)> DownloadDocumentAsync(Document document)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            var type = await _client.DownloadFileAsync(document, ms);

            return (type, ms.ToArray());
        }
    }

    private Task Ready()
    {
        Console.WriteLine($"Telegram Bot: Ready ({_client.User.first_name} - {_client.User.ID})");

        IsReady = true;

        return Task.CompletedTask;
    }

    private Task OnUpdate(IObject obj)
    {
        if (obj is not UpdatesBase updates)
            return Task.CompletedTask;
        foreach (var update in updates.UpdateList)
        {
            if (update is not UpdateNewMessage unm) continue;
            if (unm.message is not Message m) continue;

            MessageRecieved?.Invoke(m);
        }

        return Task.CompletedTask;
    }

    public event Action<Message>? MessageRecieved;

    public bool IsReady { get; private set; } = false;

    public static TelegramController TelegramInstance { get; } = 
        new TelegramController(AuthConfiguration.Default.TelegramAppId, AuthConfiguration.Default.TelegramApiHash, AuthConfiguration.Default.TelegramPhoneNumber);
}
