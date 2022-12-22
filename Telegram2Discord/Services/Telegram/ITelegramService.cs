using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TL;

namespace Test.Services
{
    public interface ITelegramService
    {
        public Task<(string Name, byte[] Data)> DownloadPhotoAsync(MessageMedia media);

        public Task<IDictionary<long, byte[]>> DownloadEmojisAsync(IEnumerable<long> ids);

        public event Action<Message> MessageRecieved;
    }
}
