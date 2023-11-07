using Newtonsoft.Json;

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

    public static Settings Instance { get; set; } = new();

    public IServiceProvider ServiceProvider { get; set; } = null!;

    public string DiscordToken { get; set; } = "";
    public string OpenDataApiKey { get; set; } = "";
    public string GoogleApiKey { get; set; } = "";

    public Dictionary<string, NotificationSetting> Notifications { get; set; } = new()
    {
        { "dailyRecord", new NotificationSetting() },
        { "dailyWeather", new NotificationSetting() }
    };

    public void Watch()
    {
        var watcher = new FileSystemWatcher
        {
            Path = ".",
            Filter = "settings.json",
            NotifyFilter = NotifyFilters.LastWrite
        };

        watcher.Changed += (_, _) =>
        {
            Console.WriteLine("settings.json changed.");
            ReloadSettings();
            Console.WriteLine("settings.json reloaded.");
        };
        watcher.EnableRaisingEvents = true;
    }

    private void ReloadSettings()
    {
        CheckFileAndInitialize();

        var json = File.ReadAllText("settings.json");
        JsonConvert.PopulateObject(json, this);

        File.WriteAllText("settings.json", JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public static Settings Load()
    {
        CheckFileAndInitialize();

        var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"))!;
        File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));
        settings.Watch();
        Instance = settings;
        return settings;
    }

    private static void CheckFileAndInitialize()
    {
        if (!File.Exists("settings.json"))
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(new Settings(), Formatting.Indented));
    }
}