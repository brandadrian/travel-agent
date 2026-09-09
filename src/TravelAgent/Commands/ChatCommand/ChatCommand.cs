using System.CommandLine;

namespace TravelAgent.Commands.ChatCommand;

public class ChatCommand : Command
{
    public ChatCommand() : base("chat", "Starts an interactive chat session with the Swiss travel assistant")
    {
        var modelOption = new Option<string?>("--model")
        {
            Description = "Ollama model name (default: OLLAMA_MODEL env or qwen3:4b)"
        };
        modelOption.Aliases.Add("-m");

        var endpointOption = new Option<string?>("--endpoint")
        {
            Description = "Ollama endpoint URI (default: OLLAMA_ENDPOINT env or http://localhost:11434)"
        };
        endpointOption.Aliases.Add("-e");

        Add(modelOption);
        Add(endpointOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var model = parseResult.GetValue(modelOption);
            var endpoint = parseResult.GetValue(endpointOption);

            var processor = new ChatCommandProcessor(model, endpoint);
            var success = await processor.ExecuteAsync(cancellationToken);
            return success ? 0 : 1;
        });
    }
}
