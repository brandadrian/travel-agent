using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using TravelAgent.Plugins;

namespace TravelAgent.Helpers;

public static class KernelHelper
{
    public static (Kernel kernel, IChatCompletionService chat, OllamaPromptExecutionSettings settings) CreateKernel(
        string? modelId = null,
        string? endpointUrl = null,
        HttpClient? httpClient = null)
    {
        modelId ??= Environment.GetEnvironmentVariable("OLLAMA_MODEL");
        if (string.IsNullOrWhiteSpace(modelId))
            modelId = "qwen3:4b";

        endpointUrl ??= Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT");
        if (string.IsNullOrWhiteSpace(endpointUrl))
            endpointUrl = "http://localhost:11434";

        httpClient ??= new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        if (!httpClient.DefaultRequestHeaders.UserAgent.Any(u => u.Product?.Name == "TravelAgent"))
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TravelAgent/1.0");
        }

        var endpoint = new Uri(endpointUrl);
        var builder = Kernel.CreateBuilder();

        builder.Services.ConfigureAll<HttpClientFactoryOptions>(options =>
        {
            options.HttpClientActions.Add(client => client.Timeout = TimeSpan.FromMinutes(10));
        });

        builder.AddOllamaChatCompletion(
            modelId: modelId,
            endpoint: endpoint);

        builder.Plugins.AddFromObject(new WeatherPlugin(httpClient), "weather");
        builder.Plugins.AddFromObject(new TransportPlugin(httpClient), "transport");

        var kernel = builder.Build();
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        var settings = new OllamaPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            Temperature = 0.2f
        };

        return (kernel, chat, settings);
    }

    public static ChatHistory CreateSystemChatHistory()
    {
        TimeZoneInfo zurichTimeZone;
        try
        {
            zurichTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");
        }
        catch
        {
            zurichTimeZone = TimeZoneInfo.Utc;
        }

        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zurichTimeZone);

        var history = new ChatHistory();
        history.AddSystemMessage($$"""
You are a Swiss travel assistant.
Today is {{now:yyyy-MM-dd}} {{now:HH:mm}} (Europe/Zurich).

Tools:
- weather: weather forecast for a city
- transport: Swiss public transport connections (date yyyy-MM-dd, time HH:mm)

Rules:
1. If a route or travel from A to B is mentioned (e.g. 'from Zurich to St. Gallen'), ALWAYS call 'transport' to get connections.
2. If weather or forecast is mentioned, call 'weather'.
3. If a prompt mentions both travel from A to B AND weather, call BOTH tools. Never invent data.
4. Short answer in English summarizing both connections and weather.
""");

        return history;
    }
}
