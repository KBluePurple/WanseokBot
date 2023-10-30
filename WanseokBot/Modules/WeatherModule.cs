using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace WanseokBot.Services.MealService;

public class WeatherModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("meal", "주변 맛집을 알려줘요!")]
    public async Task MealAsync()
    {
        await RespondAsync("아직 준비중이에요!", ephemeral: true);
    }
}