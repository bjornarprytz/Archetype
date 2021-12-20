using Archetype.Builder.Extensions;
using Archetype.Design;
using Archetype.Design.Extensions;
using Archetype.Game.Extensions;
using Archetype.Play.Context;
using Archetype.Play.Extensions;
using Archetype.Play.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Play
{
    
    
    public static class Game
    {
        public static IGameContext Create()
        {
            var serviceProvider = new ServiceCollection()
                .AddDesign()
                .AddBuilders()
                .AddArchetype()
                .AddPlayContext()
                .BuildServiceProvider();

            serviceProvider.GetService<IDesign>()!.Create();
            
            return serviceProvider.GetService<IFactory<IGameContext>>()!.Create();
        }
    }
}

