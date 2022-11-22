using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Rules;

public static class DependencyInjection
{
    public static IServiceCollection AddRules(this IServiceCollection services)
    {
        // TODO: Add rule handlers here
        return services;
    }
}