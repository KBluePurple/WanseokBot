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