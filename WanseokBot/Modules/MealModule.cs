using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace WanseokBot.Services;

public class MealModule : InteractionModuleBase<SocketInteractionContext>
{
    private const string CustomId = "meal";

    private readonly MealService _mealService = Settings.Instance.ServiceProvider.GetRequiredService<MealService>();

    [SlashCommand("meal", "주변 맛집을 알려줘요!")]
    public async Task MealAsync()
    {
        var address = await Database.GetMealAddress(Context.User.Id);

        if (string.IsNullOrEmpty(address))
        {
            await RespondWithModalAsync<MealModal>(CustomId);
            return;
        }

        var result = await _mealService.Search(address);

        if (result.Length == 0)
        {
            await RespondAsync("주변에 맛집이 없어요!");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("주변 맛집 추천")
            .WithDescription($"**{address}** 주변 맛집을 추천해드릴게요!")
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();

        foreach (var place in result)
        {
            if (embed.Fields.Count >= 5) break;
            if (place.Rating < 4.0f) continue;

            embed.AddField(place.Name, $"{place.Address}\n⭐ {place.Rating}점");
        }

        await RespondAsync(embed: embed.Build());
    }

    [ModalInteraction(CustomId)]
    public async Task MealModalAsync(MealModal modal)
    {
        var address = modal.Address;
        await DeferAsync();

        var result = await _mealService.Search(address);

        if (result.Length == 0)
        {
            await FollowupAsync("주변에 맛집이 없어요!");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("주변 맛집 추천")
            .WithDescription($"**{address}** 주변 맛집을 추천해드릴게요!")
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();

        foreach (var place in result)
        {
            if (embed.Fields.Count >= 5) break;
            if (place.Rating < 4.0f) continue;

            embed.AddField(place.Name, $"{place.Address}\n⭐ {place.Rating}점");
        }

        await FollowupAsync(embed: embed.Build());
    }

    public class MealModal : IModal
    {
        public string Title => "주변 맛집 추천받기";

        [InputLabel("주소")]
        [RequiredInput(false)]
        [ModalTextInput("address", TextInputStyle.Short, "빈칸으로 두면 마지막으로 설정한 주소를 사용해요.")]
        public string Address { get; set; } = string.Empty;
    }
}