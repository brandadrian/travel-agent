using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TravelAgent.Commands.ChatCommand;

internal class ChatCommandProcessor
{
    private readonly string? _model;
    private readonly string? _endpoint;

    public ChatCommandProcessor(string? model, string? endpoint)
    {
        _model = model;
        _endpoint = endpoint;
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var (kernel, chat, settings) = Helpers.KernelHelper.CreateKernel(_model, _endpoint);
            var history = Helpers.KernelHelper.CreateSystemChatHistory();

            var endpointUrl = _endpoint ?? Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434";
            var modelId = _model ?? Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "qwen3:4b";

            Console.WriteLine("Swiss Travel Assistant");
            Console.WriteLine($"Ollama: {endpointUrl} | Model: {modelId}");
            Console.WriteLine("Example: Tomorrow at 08:00 from Zurich to St. Gallen. What is the weather like there?");
            Console.WriteLine("Type 'exit' or 'quit' to end.\n");

            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("You > ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    break;

                history.AddUserMessage(input);

                try
                {
                    Console.WriteLine($"[Log {DateTime.Now:HH:mm:ss}] Sending request to Ollama...");

                    ChatMessageContent response;
                    using (new Helpers.LoadingSpinner("Thinking..."))
                    {
                        response = await chat.GetChatMessageContentAsync(
                            history,
                            executionSettings: settings,
                            kernel: kernel,
                            cancellationToken: cancellationToken);
                    }

                    Console.WriteLine($"[Log {DateTime.Now:HH:mm:ss}] Received response from Ollama.");

                    var answer = response.Content ?? "No response received.";
                    Console.WriteLine($"AI > {answer}\n");
                    history.AddAssistantMessage(answer);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error > Ollama or a data API is unreachable: {ex.Message}\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error > {ex.Message}\n");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Initialization error: {ex.Message}");
            return false;
        }
    }
}
