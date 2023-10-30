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

        var pangyo = await weatherService.GetToday(62, 123);
        var pangyoString =
            string.Join("\n", pangyo
                .Where(x => x.Time.Day == DateTime.Now.Day)
                .Select(x => x.ToKorean())
            );

        var gangnam = await weatherService.GetToday(61, 125);
        var gangnamString =
            string.Join("\n", gangnam
                .Where(x => x.Time.Day == DateTime.Now.Day)
                .Select(x => x.ToKorean())
            );

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
}