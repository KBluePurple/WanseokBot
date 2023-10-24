using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WanseokBot.Services;

var config = new DiscordSocketConfig
{
    UseInteractionSnowflakeDate = false,
    AlwaysDownloadUsers = true,
    MessageCacheSize = 100
};

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(new DiscordSocketClient(config));
        services.AddSingleton<InteractionService>();
        services.AddSingleton<CalenderService>();
        services.AddHostedService<DiscordStartupService>();
        services.AddHostedService<InteractionHandlingService>();
        services.AddHostedService<DailyRecordNotificationService>();
    })
    .Build();

await host.RunAsync();
