using Discord.Interactions;

namespace WanseokBot;

public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Ping pong!")]
    public async Task PingAsync()
    {
        await RespondAsync();
    }

    [SlashCommand("echo", "Echoes your input")]
    public async Task EchoAsync(string input)
    {
        await RespondAsync(input);
    }
}