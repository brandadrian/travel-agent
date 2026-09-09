using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TravelAgent.Commands.AskCommand;

internal class AskCommandProcessor
{
    private readonly string _prompt;
    private readonly string? _model;
    private readonly string? _endpoint;

    public AskCommandProcessor(string prompt, string? model, string? endpoint)
    {
        _prompt = prompt;
        _model = model;
        _endpoint = endpoint;
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var (kernel, chat, settings) = Helpers.KernelHelper.CreateKernel(_model, _endpoint);
            var history = Helpers.KernelHelper.CreateSystemChatHistory();

            history.AddUserMessage(_prompt);

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
            Console.WriteLine($"AI > {answer}");
            return true;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error > Ollama or a data API is unreachable: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error > {ex.Message}");
            return false;
        }
    }
}
