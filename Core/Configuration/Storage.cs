public class Emoji
{
    public long TelegramId { get; set; }
    public ulong DiscordId { get; set; }

    public override string ToString()
    {
        return $"<:{TelegramId}:{DiscordId}>";
    }
}

public class Storage : IConfiguration<Storage>
{
    public Dictionary<long, Emoji> Emojis { get; } = new Dictionary<long, Emoji>();

    public void Update() => Save();
}
