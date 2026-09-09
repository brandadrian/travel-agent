using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using TravelAgent;
using TravelAgent.Commands.AskCommand;
using TravelAgent.Commands.ChatCommand;
using TravelAgent.Commands.TestCommand;
using TravelAgent.Commands.TransportCommand;
using TravelAgent.Commands.WeatherCommand;

var services = new ServiceCollection()
    .AddCliCommand<ChatCommand>()
    .AddCliCommand<AskCommand>()
    .AddCliCommand<WeatherCommand>()
    .AddCliCommand<TransportCommand>()
    .AddCliCommand<TestCommand>()
    .AddSingleton<Cli>();

var provider = services.BuildServiceProvider();
var cli = provider.GetRequiredService<Cli>();
cli.Description = "TravelAgent is a command line tool for Swiss travel assistance powered by Ollama and Semantic Kernel.";

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Cancelling...");
    cts.Cancel();
    e.Cancel = true;
};

return await cli.ExecuteAsync(args, cts.Token);
