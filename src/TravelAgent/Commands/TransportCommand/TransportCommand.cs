using System.CommandLine;

namespace TravelAgent.Commands.TransportCommand;

public class TransportCommand : Command
{
    public TransportCommand() : base("connections", "Gets Swiss public transport connections directly")
    {
        var fromArgument = new Argument<string>("from")
        {
            Description = "Departure station/place, e.g. Zürich HB"
        };

        var toArgument = new Argument<string>("to")
        {
            Description = "Destination station/place, e.g. Rorschach"
        };

        var dateOption = new Option<string?>("--date")
        {
            Description = "Travel date in yyyy-MM-dd format"
        };

        var timeOption = new Option<string?>("--time")
        {
            Description = "Departure time in HH:mm format"
        };

        Add(fromArgument);
        Add(toArgument);
        Add(dateOption);
        Add(timeOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var from = parseResult.GetValue(fromArgument);
            var to = parseResult.GetValue(toArgument);
            var date = parseResult.GetValue(dateOption);
            var time = parseResult.GetValue(timeOption);

            var processor = new TransportCommandProcessor(from!, to!, date, time);
            var success = await processor.ExecuteAsync(cancellationToken);
            return success ? 0 : 1;
        });
    }
}
