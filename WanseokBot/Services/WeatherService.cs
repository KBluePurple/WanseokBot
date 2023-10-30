using Newtonsoft.Json.Linq;

namespace WanseokBot.Services;

public class WeatherService
{
    private readonly Settings _settings;

    public WeatherService(Settings settings)
    {
        _settings = settings;
    }

    private readonly string[] _times = { "0220", "0520", "0820", "1120", "1420", "1720", "2020", "2320" };

    public async Task<WeatherInfo[]> GetToday(int x, int y)
    {
        do
        {
            using var client = new HttpClient();

            var now = DateTime.Now;

            var times = _times.Select(s =>
            {
                var t = DateTime.Now;
                var h = int.Parse(s[..2]);
                var m = int.Parse(s[2..]);
                return new DateTime(t.Year, t.Month, t.Day, h, m, 0);
            }).ToArray();

            var baseDateTime = times.First(t => t <= now);

            var url = "http://apis.data.go.kr/1360000/VilageFcstInfoService_2.0/getVilageFcst";
            url += $"?ServiceKey={_settings.OpenDataApiKey}";
            url += "&pageNo=1";
            url += "&numOfRows=1000";
            url += "&dataType=JSON";
            url += $"&base_date={baseDateTime:yyyyMMdd}";
            url += $"&base_time={baseDateTime:HHmm}";
            url += $"&nx={x}";
            url += $"&ny={y}";

            try
            {
                var response = await client.GetAsync(url);

                var content = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(content);
                var items = json["response"]?["body"]?["items"]?["item"]?.ToArray();

                if (items == null) return Array.Empty<WeatherInfo>();

                var weatherInfos = new Dictionary<DateTime, WeatherInfo>();
                foreach (var item in items)
                {
                    var category = item["category"]?.ToString();
                    var value = item["fcstValue"]?.ToString();

                    var fcstDate = item["fcstDate"]?.ToString();
                    var fcstTime = item["fcstTime"]?.ToString();

                    var fcstDateTime = DateTime.ParseExact($"{fcstDate}{fcstTime}", "yyyyMMddHHmm", null);

                    if (category == null || value == null) continue;

                    var weatherInfo = weatherInfos.TryGetValue(fcstDateTime, out var info) ? info : new WeatherInfo();

                    weatherInfo.Time = fcstDateTime;

                    switch (category)
                    {
                        case "TMP":
                            weatherInfo.Temperature = float.Parse(value);
                            break;
                        case "RN1":
                            if (value == "강수없음") value = "-1";
                            weatherInfo.Rainfall = float.Parse(value);
                            break;
                        case "SKY":
                            weatherInfo.Sky = (Sky)Enum.Parse(typeof(Sky), value);
                            break;
                        case "PTY":
                            weatherInfo.State = (State)Enum.Parse(typeof(State), value);
                            break;
                    }

                    weatherInfos[weatherInfo.Time] = weatherInfo;
                }

                return weatherInfos.Values.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } while (true);
    }
}

public enum Sky
{
    Sunny = 1,
    MostlyCloudy = 3,
    Cloudy = 4,
}

public static class SkyExtensions
{
    public static string ToKorean(this Sky sky)
    {
        return sky switch
        {
            Sky.Sunny => "맑음",
            Sky.MostlyCloudy => "구름 많음",
            Sky.Cloudy => "흐림",
            _ => throw new ArgumentOutOfRangeException(nameof(sky), sky, null)
        };
    }
}

public enum State
{
    None = 0,
    Rain = 1,
    SnowAndRain = 2,
    Snow = 3,
    SmallRain = 5,
    SmallRainSmallSnow = 6,
    SmallSnow = 7,
}

public static class StateExtensions
{
    public static string ToKorean(this State state)
    {
        return state switch
        {
            State.None => "",
            State.Rain => "☔비",
            State.SnowAndRain => "☔비/눈",
            State.Snow => "☔눈",
            State.SmallRain => "🌂빗방울",
            State.SmallRainSmallSnow => "🌂빗방울/눈날림",
            State.SmallSnow => "🌂눈날림",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}

public class WeatherInfo
{
    public DateTime Time { get; set; }
    public float Temperature { get; set; }
    public float Rainfall { get; set; }
    public Sky Sky { get; set; }
    public State State { get; set; }
}

public static class WeatherInfoExtensions
{
    public static string ToKorean(this WeatherInfo weatherInfo)
    {
        var time = weatherInfo.Time.ToString("HH시");
        var temperature = $"{weatherInfo.Temperature}℃";
        // var rainfall = $"{weatherInfo.Rainfall}mm";
        var sky = weatherInfo.Sky.ToKorean();
        var state = weatherInfo.State.ToKorean();

        return $"{time} - {temperature} / {sky} / {state}";
    }
}