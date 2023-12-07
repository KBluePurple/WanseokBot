using Discord;
using Discord.WebSocket;
using Quartz;

namespace WanseokBot.Services;

public class DailyWeatherNotificationJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("DailyWeatherNotificationJob executed.");

        var dataMap = context.JobDetail.JobDataMap;

        var calendarService = (CalenderService)dataMap["calenderService"];
        var weatherService = (WeatherService)dataMap["weatherService"];

        var pangyoWeatherInfos = await weatherService.Get(62, 123);
        var pangyoString = GetWeatherString(pangyoWeatherInfos);

        var gangnamWeatherInfos = await weatherService.Get(61, 125);
        var gangnamString = GetWeatherString(gangnamWeatherInfos);

        var najuWeatherInfos = await weatherService.Get(56, 72);
        var najuString = GetWeatherString(najuWeatherInfos);

        if (calendarService.IsHoliday(DateTime.Now)) return;

        var dailyRecordNotification = (Settings.NotificationSetting)dataMap["settings"];

        var discordClient = (DiscordSocketClient)dataMap["client"];
        var guildId = dailyRecordNotification.GuildId;
        var channelId = dailyRecordNotification.ChannelId;
        var roleId = dailyRecordNotification.RoleId;

        var guild = discordClient.GetGuild(guildId);
        var channel = guild.GetTextChannel(channelId);
        var role = guild.GetRole(roleId);

        var embedBuilder = new EmbedBuilder()
            .WithColor(role.Color)
            .WithAuthor("굿모닝 날씨 알림!", guild.IconUrl)
            .WithDescription(
                $"<@&{roleId}>이신 분들은 모두 내일의 날씨를 확인해주세요!"
            )
            .WithFields(
                new EmbedFieldBuilder()
                    .WithName("강남")
                    .WithValue(pangyoString)
                    .WithIsInline(true),
                new EmbedFieldBuilder()
                    .WithName("판교")
                    .WithValue(gangnamString)
                    .WithIsInline(true),
                new EmbedFieldBuilder()
                    .WithName("나주")
                    .WithValue(najuString)
                    .WithIsInline(true)
            )
            .WithTimestamp(DateTimeOffset.Now)
            .WithFooter("정보 제공: 기상청");

        if (DateTime.Now.DayOfWeek is DayOfWeek.Friday)
        {
            embedBuilder.Fields.Add(
                new EmbedFieldBuilder()
                    .WithName("내일은...")
                    .WithValue("🎉금요일🎉\n이번 주 마지막 날!\n이번 주도 수고 많으셨습니다! 힘내세요!")
                    .WithIsInline(true)
            );
        }

        var text = $"||<@&{roleId}>||";

        var message = await channel.SendMessageAsync(text, embed: embedBuilder.Build());
        await message.AddReactionAsync(new Emoji("✅"));
    }

    private static string GetWeatherString(IEnumerable<WeatherInfo> weatherInfos)
    {
        const string temperatureStringBase = "최저 기온: {0}시 {1}도\n최고 기온: {2}시 {3}도";
        const string noRainStringBase = "비/눈 소식은 없습니다.";
        const string statusStringBase = "6시부터 22시까지 사이에 {0}이(가) 내릴 확률이 있습니다!\n🌂우산을 챙기세요!";

        var tomorrowWeatherInfo = weatherInfos.Where(w => w.Time.Date == DateTime.Now.AddDays(1).Date).ToList();

        var (lowestTemperatureInfo, highestTemperatureInfo) = tomorrowWeatherInfo
            .OrderBy(w => w.Temperature)
            .Aggregate(
                (lowest: tomorrowWeatherInfo[0], highest: tomorrowWeatherInfo[0]),
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

        var rainInfo = tomorrowWeatherInfo.Where(w => w.Time.Hour is >= 6 and <= 22).MaxBy(w => w.Rainfall);

        var rainString = rainInfo is { Rainfall: 0 }
            ? noRainStringBase
            : string.Format(statusStringBase, rainInfo?.State.ToKorean());

        return $"{temperatureString}\n{rainString}";
    }
}