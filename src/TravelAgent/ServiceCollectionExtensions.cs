using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace TravelAgent;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCliCommand<T>(this IServiceCollection services)
        where T : Command
    {
        return services.AddSingleton<Command, T>();
    }
}
