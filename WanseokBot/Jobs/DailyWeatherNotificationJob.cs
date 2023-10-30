using Discord;
using Discord.WebSocket;
using Quartz;

namespace WanseokBot.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public class DailyWeatherNotificationJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("DailyWeatherNotificationJob executed.");

        var dataMap = context.JobDetail.JobDataMap;

        var calendarService = (CalenderService)dataMap["calenderService"];
        var weatherService = (WeatherService)dataMap["weatherService"];

        var pangyoWeatherInfos = await weatherService.GetToday(62, 123);
        var pangyoString = GetWeatherString(pangyoWeatherInfos);

        var gangnamWeatherInfos = await weatherService.GetToday(61, 125);
        var gangnamString = GetWeatherString(gangnamWeatherInfos);

        Console.WriteLine(pangyoString);

        if (calendarService.IsHoliday(DateTime.Now)) return;

        var dailyRecordNotification = (Settings.NotificationSetting)dataMap["settings"];

        var discordClient = (DiscordSocketClient)dataMap["client"];
        var guildId = dailyRecordNotification.GuildId;
        var channelId = dailyRecordNotification.ChannelId;
        var roleId = dailyRecordNotification.RoleId;

        var guild = discordClient.GetGuild(guildId);
        var channel = guild.GetTextChannel(channelId);
        var role = guild.GetRole(roleId);

        var embed = new EmbedBuilder()
            .WithColor(role.Color)
            .WithAuthor("굿모닝 날씨 알림!", guild.IconUrl)
            .WithDescription(
                $"<@&{roleId}>이신 분들은 모두 오늘의 날씨를 확인해주세요!"
            )
            .WithFields(
                new EmbedFieldBuilder()
                    .WithName("강남")
                    .WithValue(pangyoString)
                    .WithIsInline(true),
                new EmbedFieldBuilder()
                    .WithName("판교")
                    .WithValue(gangnamString)
                    .WithIsInline(true)
            )
            .WithTimestamp(DateTimeOffset.Now)
            .WithFooter("정보 제공: 기상청")
            .Build();

        var text = $"||<@&{roleId}>||";

        var message = await channel.SendMessageAsync(text, embed: embed);
        await message.AddReactionAsync(new Emoji("✅"));
    }

    private static string GetWeatherString(IReadOnlyList<WeatherInfo> weatherInfos)
    {
        const string temperatureStringBase = "최저 기온은 {0}시에 {1}도이며, 최고 기온은 {2}시에 {3}도입니다.";
        const string noRainStringBase = "비/눈 소식은 없습니다.";
        const string statusStringBase = "6시부터 10시까지 {0}이(가) 내릴 확률이 있습니다!\n🌂우산을 챙기세요!";

        var (lowestTemperatureInfo, highestTemperatureInfo) = weatherInfos
            .OrderBy(w => w.Temperature)
            .Aggregate(
                (lowest: weatherInfos[0], highest: weatherInfos[0]),
                (acc, w) => (
                    w.Temperature < acc.lowest.Temperature ? w : acc.lowest,
                    w.Temperature > acc.highest.Temperature ? w : acc.highest
                )
            );

        var temperatureString = string.Format(
            temperatureStringBase,
            lowestTemperatureInfo.Time.ToString("HH"),
            lowestTemperatureInfo.Temperature,
            highestTemperatureInfo.Time.ToString("HH"),
            highestTemperatureInfo.Temperature
        );

        var rainInfo = weatherInfos.Where(w => w.Time.Hour is >= 6 and <= 22).MaxBy(w => w.Rainfall);

        var rainString = rainInfo is { Rainfall: 0 }
            ? noRainStringBase
            : string.Format(statusStringBase, rainInfo?.State.ToKorean());

        return $"{temperatureString}\n{rainString}";
    }
}