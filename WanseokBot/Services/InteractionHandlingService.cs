using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace WanseokBot.Services;

public class InteractionHandlingService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;

    public InteractionHandlingService(DiscordSocketClient client, InteractionService interactions,
        IServiceProvider services)
    {
        _client = client;
        _interactions = interactions;
        _services = services;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Ready += async () => { await _interactions.RegisterCommandsGloballyAsync(); };

        _client.InteractionCreated += HandleInteractionAsync;
        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.InteractionCreated -= HandleInteractionAsync;
        _interactions.Dispose();
        return Task.CompletedTask;
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            var result = await _interactions.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
                await interaction.RespondAsync(result.ToString(), ephemeral: true);
        }
        catch
        {
            if (interaction.Type != InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(msg => msg.Result.DeleteAsync());
        }
    }
}