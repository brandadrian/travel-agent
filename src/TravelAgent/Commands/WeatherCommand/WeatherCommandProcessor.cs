using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TravelAgent.Plugins;

namespace TravelAgent.Commands.WeatherCommand;

internal class WeatherCommandProcessor
{
    private readonly string _city;
    private readonly int _days;

    public WeatherCommandProcessor(string city, int days)
    {
        _city = city;
        _days = days;
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TravelAgent/1.0");

            var plugin = new WeatherPlugin(httpClient);
            var forecast = await plugin.GetForecastAsync(_city, _days);

            Console.WriteLine(forecast);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching weather data: {ex.Message}");
            return false;
        }
    }
}
