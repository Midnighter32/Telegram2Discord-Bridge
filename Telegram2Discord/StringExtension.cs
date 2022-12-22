using System.Collections.Generic;
using System.Linq;
using Test.Services;
using TL;

namespace Test
{
    public static class StringExtension
    {
        public static string ApplyRules(this string str, IEnumerable<MessageEntity> entities, IEmojiService emojiStorage)
        {
            String formatted = new String(str);

            foreach (var entity in entities.OrderBy(x => x.offset))
            {
                switch (entity)
                {
                    case MessageEntityCode code:
                        formatted.Insert(code.offset, "`");
                        formatted.Insert(code.offset + code.length, "`");
                        break;
                    case MessageEntityBold bold:
                        formatted.Insert(bold.offset, "**");
                        formatted.Insert(bold.offset + bold.length, "**");
                        break;
                    case MessageEntityCustomEmoji emoji:
                        if (emojiStorage.ContainsEmoji(emoji.document_id))
                            formatted.Replace(
                            emoji.offset,
                                emoji.length,
                                emojiStorage.GetEmoji(emoji.document_id));
                        break;
                    case MessageEntityTextUrl url:
                        formatted.Insert(url.offset + url.length, $"({url.url})");
                        break;
                    default: break;
                }
            }

            return formatted.ToString();
        }
    }
}
