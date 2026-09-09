using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TravelAgent.Plugins;

namespace TravelAgent.Commands.TransportCommand;

internal class TransportCommandProcessor
{
    private readonly string _from;
    private readonly string _to;
    private readonly string? _date;
    private readonly string? _time;

    public TransportCommandProcessor(string from, string to, string? date, string? time)
    {
        _from = from;
        _to = to;
        _date = date;
        _time = time;
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TravelAgent/1.0");

            var plugin = new TransportPlugin(httpClient);
            var connections = await plugin.GetConnectionsAsync(_from, _to, _date, _time);

            Console.WriteLine(connections);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching connection information: {ex.Message}");
            return false;
        }
    }
}
