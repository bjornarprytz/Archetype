using System;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Client
{
    public static class Factory
    {
        public static IArchetypeGraphQLClient CreateClient()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddArchetypeGraphQLClient()
                .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://localhost:7006/graphql"));

            var services = serviceCollection.BuildServiceProvider();

            return services.GetRequiredService<IArchetypeGraphQLClient>();
        }
    }
}