using Archetype.Builder.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Design.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDesign(this IServiceCollection serviceCollection)
        {

            serviceCollection
                .AddSingleton(sp => TestDesign.BuildMap(sp.GetRequiredService<IMapBuilder>()))
                .AddSingleton(sp => TestDesign.BuildCardPool(sp.GetRequiredService<IPoolBuilder>()));


            return serviceCollection;
        }
    }
}