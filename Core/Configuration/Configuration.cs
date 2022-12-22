class AuthConfiguration : IConfiguration<AuthConfiguration>
{
    public string DiscordToken { get; set; }

    public int TelegramAppId { get; set; }

    public string TelegramApiHash { get; set; }

    public string TelegramPhoneNumber { get; set; }

    public static AuthConfiguration Default { get; } = Load();
}