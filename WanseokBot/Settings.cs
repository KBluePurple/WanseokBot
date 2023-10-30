namespace WanseokBot;

[Serializable]
public class Settings
{
    [Serializable]
    public class NotificationSetting
    {
        public ulong GuildId { get; set; } = 0;
        public ulong ChannelId { get; set; } = 0;
        public ulong RoleId { get; set; } = 0;
        public string Cron { get; set; } = "0 30 20 * * ?";
    }

    public string Token { get; set; } = "";
    public string OpenDataApiKey { get; set; } = "";

    public Dictionary<string, NotificationSetting> Notifications { get; set; } = new()
    {
        { "dailyRecord", new NotificationSetting() },
        { "dailyWeather", new NotificationSetting() }
    };
}