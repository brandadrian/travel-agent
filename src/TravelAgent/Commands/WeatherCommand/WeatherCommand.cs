using System.CommandLine;

namespace TravelAgent.Commands.WeatherCommand;

public class WeatherCommand : Command
{
    public WeatherCommand() : base("weather", "Gets weather forecast for a location directly")
    {
        var cityArgument = new Argument<string>("city")
        {
            Description = "City or place name, e.g. Genf, Zürich, Bern"
        };

        var daysOption = new Option<int>("--days")
        {
            Description = "Number of forecast days (1-7)"
        };
        daysOption.Aliases.Add("-d");

        Add(cityArgument);
        Add(daysOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var city = parseResult.GetValue(cityArgument);
            var days = parseResult.GetValue(daysOption);
            if (days <= 0)
            {
                days = 4;
            }

            var processor = new WeatherCommandProcessor(city!, days);
            var success = await processor.ExecuteAsync(cancellationToken);
            return success ? 0 : 1;
        });
    }
}
