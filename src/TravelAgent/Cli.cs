using System.Collections.Generic;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace TravelAgent;

public class Cli
{
    private readonly RootCommand _rootCommand;

    public Cli(IEnumerable<Command> commands)
    {
        _rootCommand = new RootCommand("TravelAgent is a command line tool for Swiss travel assistance.");

        foreach (var cmd in commands)
        {
            _rootCommand.Add(cmd);
        }
    }

    public string? Description
    {
        get => _rootCommand.Description;
        set => _rootCommand.Description = value;
    }

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var parseResult = _rootCommand.Parse(args);
        return await parseResult.InvokeAsync(cancellationToken: cancellationToken);
    }
}
