using EmojiToDiscord.Converters;
using System.Drawing;
using TL;
using static TelegramController;

static class DocumentExtension
{
    public static async Task<Bitmap> DownloadAsync(this Document document)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            var result = await TelegramInstance.DownloadDocumentAsync(document);

            return WebP.Load(result.Data);
        }
    }
}