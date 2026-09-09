using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TravelAgent.Commands.TestCommand;

internal class TestCommandProcessor
{
    private readonly string? _model;
    private readonly string? _endpoint;

    public TestCommandProcessor(string? model, string? endpoint)
    {
        _model = model;
        _endpoint = endpoint;
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var endpointUrl = _endpoint ?? Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434";
            var modelId = _model ?? Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "qwen3:4b";

            Console.WriteLine($"[Test] Connecting to Ollama at {endpointUrl} with model '{modelId}'...");

            var (kernel, chat, settings) = Helpers.KernelHelper.CreateKernel(_model, _endpoint);

            var history = new ChatHistory();
            history.AddUserMessage("Say hello in one sentence.");

            Console.WriteLine($"[Test {DateTime.Now:HH:mm:ss}] Sending simple ping request to Ollama...");

            ChatMessageContent response;
            using (new Helpers.LoadingSpinner("Waiting for Ollama response..."))
            {
                response = await chat.GetChatMessageContentAsync(
                    history,
                    executionSettings: settings,
                    kernel: kernel,
                    cancellationToken: cancellationToken);
            }

            Console.WriteLine($"[Test {DateTime.Now:HH:mm:ss}] Received response from Ollama!");
            Console.WriteLine($"Ollama > {response.Content}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test Error] {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }
}
