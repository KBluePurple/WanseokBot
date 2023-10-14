using Discord.Interactions;

namespace WanseokBot;

public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Ping pong!")]
    public async Task PingAsync()
    {
        await ReplyAsync("Pong!");
    }

    [SlashCommand("echo", "Echoes your input")]
    public async Task EchoAsync(string input)
    {
        await ReplyAsync(input);
    }
}