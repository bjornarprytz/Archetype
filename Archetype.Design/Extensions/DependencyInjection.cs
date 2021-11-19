using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Design.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDesign(this IServiceCollection serviceCollection)
        {

            serviceCollection
                .AddSingleton(_ => TestDesign.BuildMap())
                .AddSingleton(_ => TestDesign.BuildCardPool());


            return serviceCollection;
        }
    }
}