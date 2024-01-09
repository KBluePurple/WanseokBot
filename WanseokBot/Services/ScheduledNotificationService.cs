using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;

namespace WanseokBot.Services;

public class ScheduledNotificationService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly CalenderService _calender;
    private readonly WeatherService _weather;
    private readonly Settings _settings;

    public ScheduledNotificationService(CalenderService calender, WeatherService weather, Settings settings, DiscordSocketClient client)
    {
        _calender = calender;
        _weather = weather;
        _client = client;
        _settings = settings;
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
        var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();

        // var notificationSettings = _settings.Notifications["dailyRecord"];

        // var data = new JobDataMap
        // {
        //     { "calenderService", _calender },
        //     { "client", _client },
        //     { "settings", notificationSettings }
        // };

        // var job = JobBuilder.Create<DailyRecordNotificationJob>()
        //     .WithIdentity("daily-record-notification-job")
        //     .SetJobData(data)
        //     .Build();

        // var trigger = TriggerBuilder.Create()
        //     .WithIdentity("daily-record-notification-trigger")
        //     .StartNow()
        //     .WithSchedule(CronScheduleBuilder.CronSchedule(notificationSettings!.Cron))
        //     .Build();

        // await scheduler.ScheduleJob(job, trigger);

        var weatherSettings = _settings.Notifications["dailyWeather"];

        var weatherData = new JobDataMap
        {
            { "calenderService", _calender! },
            { "weatherService", _weather! },
            { "client", _client! },
            { "settings", weatherSettings }
        };

        var weatherJob = JobBuilder.Create<DailyWeatherNotificationJob>()
            .WithIdentity("daily-weather-notification-job")
            .SetJobData(weatherData)
            .Build();

        var weatherTrigger = TriggerBuilder.Create()
            .WithIdentity("daily-weather-notification-trigger")
            .StartNow()
            .WithSchedule(CronScheduleBuilder.CronSchedule(weatherSettings!.Cron))
            .Build();

        await scheduler.ScheduleJob(weatherJob, weatherTrigger);
    }
}
