using System.CommandLine;

namespace TravelAgent.Commands.TestCommand;

public class TestCommand : Command
{
    public TestCommand() : base("test", "Tests basic connection and simple response from Ollama")
    {
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

        Add(modelOption);
        Add(endpointOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var model = parseResult.GetValue(modelOption);
            var endpoint = parseResult.GetValue(endpointOption);

            var processor = new TestCommandProcessor(model, endpoint);
            var success = await processor.ExecuteAsync(cancellationToken);
            return success ? 0 : 1;
        });
    }
}
