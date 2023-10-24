using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using WanseokBot.Data;

namespace WanseokBot.Services;

public class CalenderService
{
    private readonly HashSet<DateTime> _holidays = new();
    private DateTime _lastUpdate = DateTime.MinValue;

    public CalenderService()
    {
        UpdateHolidays().Wait();
    }

    public bool IsHoliday(DateTime date)
    {
        if (date.Month != _lastUpdate.Month)
            UpdateHolidays().Wait();

        return date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday || _holidays.Contains(date);
    }

    private async Task UpdateHolidays()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(
            "https://apis.data.go.kr/B090041/openapi/service/SpcdeInfoService/getRestDeInfo?serviceKey=3waHcJo7VaTeRYq6U2eFaqL8ikgfUCmGVNAPFtbwUyVUgoZlcq%2BoKGrFXRicPEr5DHU8R3gkLncQqyECSF%2FrCg%3D%3D&solYear=2023&solMonth=10&_type=json");

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<CalenderResponse>(json);

        if (data?.Response.Body.Items.Item is null) return;

        foreach (var item in data.Response.Body.Items.Item)
        {
            if (item.IsHoliday != "Y") continue;

            var date = DateTimeOffset.ParseExact(item.Locdate.ToString(), "yyyyMMdd", null).DateTime;
            _holidays.Add(date);
        }

        _lastUpdate = DateTime.Now;
    }
}