using Discord;

class EmojiStorage
{
    private DiscordController _controller { get; }

    public EmojiStorage(DiscordController discordController, IEnumerable<ulong> storages)
    {
        _controller = discordController;

        Console.WriteLine("Emoji storages:");

        foreach (var storage_id in storages)
        {
            var guild = _controller.GetGuild(storage_id);

            Storages.Add(guild);

            Console.WriteLine($"\t{guild.Name}: {guild.Id}");
        }
    }

    public async Task<ulong> AddEmojiAsync(long EmojiId, byte[] data)
    {
        foreach (var storage in Storages)
        {
            if (storage.Emotes.Count < 50)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    var emote = await storage.CreateEmoteAsync(EmojiId.ToString(), new Discord.Image(ms));

                    return emote.Id;
                }
            }
        }

        return 0;
    }

    private List<IGuild> Storages { get; set; } = new List<IGuild>();
}
