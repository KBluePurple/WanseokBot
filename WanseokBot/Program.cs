using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using WanseokBot;
using WanseokBot.Services;

if (!File.Exists("settings.json"))
    File.WriteAllText("settings.json", JsonConvert.SerializeObject(new Settings(), Formatting.Indented));

var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"))!;
File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));

var config = new DiscordSocketConfig
{
    UseInteractionSnowflakeDate = false,
    AlwaysDownloadUsers = true,
    MessageCacheSize = 100
};

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(settings);
        services.AddSingleton(new DiscordSocketClient(config));
        services.AddSingleton<InteractionService>();
        services.AddSingleton<CalenderService>();
        services.AddSingleton<WeatherService>();
        services.AddHostedService<DiscordStartupService>();
        services.AddHostedService<InteractionHandlingService>();
        services.AddHostedService<ScheduledNotificationService>();
    })
    .Build();

await host.RunAsync();