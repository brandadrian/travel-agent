using System;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace TravelAgent.Plugins;

public sealed class WeatherPlugin(HttpClient httpClient)
{
    [KernelFunction("get_forecast")]
    [Description("Gets weather forecast for a city.")]
    public async Task<string> GetForecastAsync(
        [Description("City name, e.g. Zurich")] string city,
        [Description("Forecast days (1-7)")] int days = 4)
    {
        days = Math.Clamp(days, 1, 7);
        Console.WriteLine($"\r[Log {DateTime.Now:HH:mm:ss}] WeatherPlugin: Searching geodata for '{city}'...");

        var geoUrl =
            "https://geocoding-api.open-meteo.com/v1/search" +
            $"?name={Uri.EscapeDataString(city)}&count=1&language=en&format=json";

        using var geoResponse = await httpClient.GetAsync(geoUrl);
        geoResponse.EnsureSuccessStatusCode();

        await using var geoStream = await geoResponse.Content.ReadAsStreamAsync();
        using var geoJson = await JsonDocument.ParseAsync(geoStream);

        if (!geoJson.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
        {
            Console.WriteLine($"\r[Log {DateTime.Now:HH:mm:ss}] WeatherPlugin: No location found for '{city}'.");
            return $"No location found for '{city}'.";
        }

        var place = results[0];
        var name = place.GetProperty("name").GetString() ?? city;
        var country = place.TryGetProperty("country", out var countryEl) ? countryEl.GetString() : null;
        var latitude = place.GetProperty("latitude").GetDouble();
        var longitude = place.GetProperty("longitude").GetDouble();

        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);

        Console.WriteLine($"\r[Log {DateTime.Now:HH:mm:ss}] WeatherPlugin: Loading weather forecast for {name} ({lat}, {lon})...");

        var forecastUrl =
            "https://api.open-meteo.com/v1/forecast" +
            $"?latitude={lat}&longitude={lon}" +
            "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max,wind_speed_10m_max" +
            $"&forecast_days={days}&timezone=auto";

        using var forecastResponse = await httpClient.GetAsync(forecastUrl);
        forecastResponse.EnsureSuccessStatusCode();

        await using var forecastStream = await forecastResponse.Content.ReadAsStreamAsync();
        using var forecastJson = await JsonDocument.ParseAsync(forecastStream);

        if (!forecastJson.RootElement.TryGetProperty("daily", out var daily))
        {
            Console.WriteLine($"\r[Log {DateTime.Now:HH:mm:ss}] WeatherPlugin: No weather data received for {name}.");
            return $"No weather data received for {name}.";
        }

        Console.WriteLine($"\r[Log {DateTime.Now:HH:mm:ss}] WeatherPlugin: Weather data for {name} successfully loaded.");

        var dates = daily.GetProperty("time");
        var codes = daily.GetProperty("weather_code");
        var maxTemps = daily.GetProperty("temperature_2m_max");
        var minTemps = daily.GetProperty("temperature_2m_min");
        var rainProbabilities = daily.GetProperty("precipitation_probability_max");
        var maxWinds = daily.GetProperty("wind_speed_10m_max");

        var sb = new StringBuilder();
        sb.AppendLine($"Weather for {name}{(string.IsNullOrWhiteSpace(country) ? "" : $", {country}")}:");

        for (var i = 0; i < dates.GetArrayLength(); i++)
        {
            var date = dates[i].GetString();
            var code = codes[i].GetInt32();
            var max = maxTemps[i].GetDouble();
            var min = minTemps[i].GetDouble();
            var rain = rainProbabilities[i].ValueKind == JsonValueKind.Null ? (double?)null : rainProbabilities[i].GetDouble();
            var wind = maxWinds[i].GetDouble();

            sb.Append($"- {date}: {WeatherCodeToText(code)}, {min:0.#}–{max:0.#} °C");
            if (rain is not null)
                sb.Append($", max rain probability {rain:0}%");
            sb.AppendLine($", max wind {wind:0.#} km/h");
        }

        return sb.ToString().TrimEnd();
    }

    private static string WeatherCodeToText(int code) => code switch
    {
        0 => "clear",
        1 => "mainly clear",
        2 => "partly cloudy",
        3 => "overcast",
        45 or 48 => "fog",
        51 or 53 or 55 => "drizzle",
        56 or 57 => "freezing drizzle",
        61 or 63 or 65 => "rain",
        66 or 67 => "freezing rain",
        71 or 73 or 75 => "snowfall",
        77 => "snow grains",
        80 or 81 or 82 => "rain showers",
        85 or 86 => "snow showers",
        95 => "thunderstorm",
        96 or 99 => "thunderstorm with hail",
        _ => "unknown"
    };
}
