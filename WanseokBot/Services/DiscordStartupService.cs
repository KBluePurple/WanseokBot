using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WanseokBot.Services;

public class DiscordStartupService : IHostedService
{
    private readonly DiscordSocketClient _discord;
    private readonly Settings _settings;

    public DiscordStartupService(DiscordSocketClient discord, Settings settings, ILogger<DiscordSocketClient> logger)
    {
        _discord = discord;
        _settings = settings;
        _discord.Log += msg => Task.Run(() => Console.WriteLine(msg.ToString()));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _discord.LoginAsync(TokenType.Bot, _settings.DiscordToken);
        await _discord.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _discord.LogoutAsync();
        await _discord.StopAsync();
    }
}