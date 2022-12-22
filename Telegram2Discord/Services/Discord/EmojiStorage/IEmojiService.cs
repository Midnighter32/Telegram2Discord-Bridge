using System.Threading.Tasks;

namespace Test.Services
{
    public interface IEmojiService
    {
        public Task AddEmojiAsync(long id, byte[] data);

        public string GetEmoji(long id);

        public bool ContainsEmoji(long id);

        public void Save();
    }
}
