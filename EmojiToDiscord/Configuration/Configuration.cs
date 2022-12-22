class Configuration : IConfiguration<Configuration>
{
    public IEnumerable<ulong> EmojiStorages { get; } = new List<ulong>();

    public static Configuration Default { get; } = Load();
}