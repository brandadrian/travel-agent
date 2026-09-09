using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace TravelAgent.Plugins;

public sealed class TransportPlugin(HttpClient httpClient)
{
    [KernelFunction("get_connections")]
    [Description("Gets real Swiss public transport train/bus connections between two locations. Call this whenever travel or a route from A to B is mentioned.")]
    public async Task<string> GetConnectionsAsync(
        [Description("Departure station, e.g. Zurich HB")] string from,
        [Description("Destination station, e.g. Bern")] string to,
        [Description("Date in yyyy-MM-dd format")] string? date = null,
        [Description("Departure time in HH:mm format")] string? time = null)
    {
        Console.WriteLine($"\r[Log {DateTime.Now:HH:mm:ss}] TransportPlugin: Searching public transport connections from '{from}' to '{to}' (Date: {date ?? "today"}, Time: {time ?? "now"})...");

        if (!string.IsNullOrWhiteSpace(date) &&
            !DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return $"Invalid date '{date}'. Expected yyyy-MM-dd.";
        }

        if (!string.IsNullOrWhiteSpace(time) &&
            !TimeOnly.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return $"Invalid time '{time}'. Expected HH:mm.";
        }

        var url = new StringBuilder("https://transport.opendata.ch/v1/connections");
        url.Append($"?from={Uri.EscapeDataString(from)}");
        url.Append($"&to={Uri.EscapeDataString(to)}");
        url.Append("&limit=4");

        if (!string.IsNullOrWhiteSpace(date))
            url.Append($"&date={Uri.EscapeDataString(date)}");
        if (!string.IsNullOrWhiteSpace(time))
            url.Append($"&time={Uri.EscapeDataString(time)}");

        using var response = await httpClient.GetAsync(url.ToString());
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(stream);

        if (!json.RootElement.TryGetProperty("connections", out var connections) || connections.GetArrayLength() == 0)
        {
            Console.WriteLine($"\r[Log {DateTime.Now:HH:mm:ss}] TransportPlugin: No connections found.");
            return $"No connections found from {from} to {to}.";
        }

        Console.WriteLine($"\r[Log {DateTime.Now:HH:mm:ss}] TransportPlugin: Public transport connections for {from} → {to} loaded.");

        var sb = new StringBuilder();
        sb.AppendLine($"Public transport connections {from} → {to}:");

        var index = 1;
        foreach (var connection in connections.EnumerateArray())
        {
            var fromStop = connection.GetProperty("from");
            var toStop = connection.GetProperty("to");

            var departure = GetString(fromStop, "departure");
            var arrival = GetString(toStop, "arrival");
            var platform = GetString(fromStop, "platform");
            var duration = GetString(connection, "duration");
            var transfers = connection.TryGetProperty("transfers", out var transfersEl) && transfersEl.ValueKind == JsonValueKind.Number
                ? transfersEl.GetInt32().ToString(CultureInfo.InvariantCulture)
                : "?";

            var products = connection.TryGetProperty("products", out var productsEl) && productsEl.ValueKind == JsonValueKind.Array
                ? string.Join(", ", productsEl.EnumerateArray().Select(p => p.GetString()).Where(x => !string.IsNullOrWhiteSpace(x)))
                : "";

            sb.Append($"{index}. {FormatDateTime(departure)} → {FormatDateTime(arrival)}");
            if (!string.IsNullOrWhiteSpace(duration))
                sb.Append($", Duration {FormatDuration(duration)}");
            sb.Append($", Transfers {transfers}");
            if (!string.IsNullOrWhiteSpace(platform))
                sb.Append($", Platform {platform}");
            if (!string.IsNullOrWhiteSpace(products))
                sb.Append($", {products}");
            sb.AppendLine();

            index++;
        }

        return sb.ToString().TrimEnd();
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string FormatDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "?";

        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed.ToString("dd.MM. HH:mm", CultureInfo.GetCultureInfo("de-CH"))
            : value;
    }

    private static string FormatDuration(string value)
    {
        if (value.StartsWith("00d", StringComparison.OrdinalIgnoreCase))
            value = value.Substring(3);

        if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var ts))
            return $"{ts.Hours:D2}:{ts.Minutes:D2}";

        return value;
    }
}
