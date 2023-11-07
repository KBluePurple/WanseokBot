using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WanseokBot;
using WanseokBot.Services;

var config = new DiscordSocketConfig
{
    UseInteractionSnowflakeDate = false,
    AlwaysDownloadUsers = true,
    MessageCacheSize = 100
};

Database.Initialize();

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(Settings.Load());
        services.AddSingleton(new DiscordSocketClient(config));
        services.AddSingleton<InteractionService>();
        services.AddSingleton<CalenderService>();
        services.AddSingleton<WeatherService>();
        services.AddSingleton<MealService>();
        services.AddHostedService<DiscordStartupService>();
        services.AddHostedService<InteractionHandlingService>();
        services.AddHostedService<ScheduledNotificationService>();
    })
    .Build();

Settings.Instance.ServiceProvider = host.Services;

await host.RunAsync();