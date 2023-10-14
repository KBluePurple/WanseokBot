using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WanseokBot;

public class DiscordStartupService : IHostedService
{
    private readonly DiscordSocketClient _discord;

    public DiscordStartupService(DiscordSocketClient discord, ILogger<DiscordSocketClient> logger)
    {
        _discord = discord;
        _discord.Log += msg => Task.Run(() => logger.LogInformation(msg.ToString()));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _discord.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("TOKEN"));
        await _discord.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _discord.LogoutAsync();
        await _discord.StopAsync();
    }
}