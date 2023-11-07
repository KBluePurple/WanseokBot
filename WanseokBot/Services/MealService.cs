using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WanseokBot.Services;

public class MealService
{
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class Place
    {
        public string? Name { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public float Rating { get; set; }
        public string? PhotoUrl { get; set; } = string.Empty;
    }

    public async Task<Place[]> Search(string address)
    {
        var location = await GetLocationFromAddress(address);
        var places = await GetPlaces(location);
        return places;
    }

    private async Task<Location> GetLocationFromAddress(string address)
    {
        var url =
            $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={Settings.Instance.GoogleApiKey}";
        var response = await new HttpClient().GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var jObject = JObject.Parse(json);
        var location = jObject["results"]?[0]?["geometry"]?["location"];

        if (location != null)
            return new Location
            {
                Latitude = (location["lat"] ?? throw new InvalidOperationException()).Value<double>(),
                Longitude = (location["lng"] ?? throw new InvalidOperationException()).Value<double>()
            };

        throw new InvalidOperationException();
    }

    private async Task<Place[]> GetPlaces(Location location)
    {
        var url =
            $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={location.Latitude},{location.Longitude}&type=restaurant&radius=500&language=ko&key={Settings.Instance.GoogleApiKey}";
        var response = await new HttpClient().GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var jObject = JObject.Parse(json);
        var results = jObject["results"]?.ToArray();

        if (results == null)
            throw new InvalidOperationException();

        return results.Select(result => new Place
        {
            Name = result["name"]?.Value<string>(),
            Address = result["vicinity"]?.Value<string>(),
            Rating = result["rating"]?.Value<float>() ?? 0f,
            PhotoUrl = result["photos"]?[0]?["photo_reference"]?.Value<string>()
        }).ToArray();
    }
}