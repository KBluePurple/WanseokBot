using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;

namespace WanseokBot.Services;

public class DailyRecordNotificationService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly CalenderService _calender;

    public DailyRecordNotificationService(CalenderService calender, DiscordSocketClient client)
    {
        _client = client;
        _calender = calender;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Ready += OnClientReady;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.Ready -= OnClientReady;
        return Task.CompletedTask;
    }

    private async Task OnClientReady()
    {
        const ulong targetGuildId = 1162357983091621930UL;
        const ulong targetChannelId = 1162373221690126457UL;
        const ulong targetRoleId = 1164872355076644894UL;

        var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();

        var data = new JobDataMap
        {
            { "calenderService", _calender },
            { "client", _client },
            { "guildId", targetGuildId },
            { "channelId", targetChannelId },
            { "roleId", targetRoleId }
        };

        var job = JobBuilder.Create<DailyRecordNotificationJob>()
            .WithIdentity("daily-record-notification-job")
            .SetJobData(data)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity("daily-record-notification-trigger")
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(20, 30))
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
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