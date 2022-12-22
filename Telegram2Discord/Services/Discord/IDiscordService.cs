using Discord;
using Test.DataTypes;

namespace Test.Services
{
    public interface IDiscordService
    {
        public IGuild GetGuild(ulong id);

        public void SendMessage(News post);
    }
}
