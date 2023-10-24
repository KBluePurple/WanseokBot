using Discord;
using Discord.WebSocket;
using Quartz;

namespace WanseokBot.Services;

public class DailyRecordNotificationJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("DailyRecordNotificationJob executed.");

        var dataMap = context.JobDetail.JobDataMap;

        var calendarService = (CalenderService)dataMap["calenderService"];

        if (calendarService.IsHoliday(DateTime.Now)) return;

        var client = (DiscordSocketClient)dataMap["client"];
        var guildId = (ulong)dataMap["guildId"];
        var channelId = (ulong)dataMap["channelId"];
        var roleId = (ulong)dataMap["roleId"];

        var guild = client.GetGuild(guildId);
        var channel = guild.GetTextChannel(channelId);
        var role = guild.GetRole(roleId);

        var embed = new EmbedBuilder()
            .WithColor(role.Color)
            .WithAuthor("실습 일지 알림!", guild.IconUrl)
            .WithDescription(
                $"""
                 <@&{roleId}>이신 분들은 모두 오늘의 실습 일지를 작성해주세요!
                 [실습 일지 작성 바로가기](https://www.hifive.go.kr/mobile/mInvovedReportingMain.do)
                 """
            )
            .WithTimestamp(DateTimeOffset.Now)
            .Build();

        var text = $"||<@&{roleId}>||";

        await channel.SendMessageAsync(text, embed: embed);
    }
}