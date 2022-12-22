using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using static DiscordController;
using static TelegramController;

using Storage session = Storage.Load();

await TelegramInstance.StartAsync();
await DiscordInstance.StartAsync();

ConcurrentDictionary<long, byte[]> UnknownEmojis = new ConcurrentDictionary<long, byte[]>();

TelegramInstance.MessageRecieved += async message =>
{
    if (message.entities is null) return;

    foreach (var document in await TelegramInstance.GetEmojiDocumentsAsync(GetEmojisFromMessage(message)))
    {
        if (session.Emojis.ContainsKey(document.ID)) continue;

        using (MemoryStream ms = new MemoryStream())
        {
            Bitmap bmp = await document.DownloadAsync();
            bmp.Save(ms, ImageFormat.Png);

            if (!UnknownEmojis.TryAdd(document.ID, ms.ToArray()))
                Console.WriteLine($"[ ADD ] {document.ID}: Failed");
        }
    }
};

_ = Task.Factory.StartNew(async () =>
{
    EmojiStorage? emojiStorage = null;

    while (true)
    {
        if (DiscordInstance.IsReady)
        {
            if (emojiStorage is null) emojiStorage = new EmojiStorage(DiscordInstance, Configuration.Default.EmojiStorages);

            foreach (var key in UnknownEmojis.Keys)
            {
                if (!UnknownEmojis.TryGetValue(key, out var bytes))
                    Console.WriteLine($"[ GET ] {key}: Failed");

                var id = await emojiStorage.AddEmojiAsync(key, bytes!);

                if (id is 0)
                    Console.WriteLine($"[ CREATE EMOJI ] {key}: Failed");
                else
                {
                    session.Emojis.Add(key, new Emoji
                    {
                        TelegramId = key,
                        DiscordId = id
                    });

                    if (!UnknownEmojis.TryRemove(key, out _))
                        Console.WriteLine($"[ REMOVE ] {key}: Failed");

                    Console.WriteLine($"[ CREATE EMOJI ] {key}: Success");
                }

                session.Update();
                await Task.Delay(5000);
            }
        }

        await Task.Delay(5000);
    }
});

Console.ReadKey();

Configuration.Default.Save();