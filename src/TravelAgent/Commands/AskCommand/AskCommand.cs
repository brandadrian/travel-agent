using System.CommandLine;

namespace TravelAgent.Commands.AskCommand;

public class AskCommand : Command
{
    public AskCommand() : base("ask", "Asks a single query to the Swiss travel assistant")
    {
        var promptArgument = new Argument<string>("prompt")
        {
            Description = "The travel question or query to ask"
        };

        var modelOption = new Option<string?>("--model")
        {
            Description = "Ollama model name"
        };
        modelOption.Aliases.Add("-m");

        var endpointOption = new Option<string?>("--endpoint")
        {
            Description = "Ollama endpoint URI"
        };
        endpointOption.Aliases.Add("-e");

        Add(promptArgument);
        Add(modelOption);
        Add(endpointOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var prompt = parseResult.GetValue(promptArgument);
            var model = parseResult.GetValue(modelOption);
            var endpoint = parseResult.GetValue(endpointOption);

            var processor = new AskCommandProcessor(prompt!, model, endpoint);
            var success = await processor.ExecuteAsync(cancellationToken);
            return success ? 0 : 1;
        });
    }
}
