using Archetype.Framework.Core.Structure;
using Archetype.Framework.DependencyInjection;
using Archetype.Framework.Design;
using Archetype.Framework.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Prototype1;

// TODO: Make a package that I can use in Godot

// It should have:
// - A simple game state: Three proto cards
//  - (an enemy, a structure to defend, and a crew)
// - A small deck (10 cards)
// - A simple game loop that draws a card, waits for player input, and then resolves combat

public static class Game
{
    private static IServiceProvider _serviceProvider = null;
    
    public static IGameRoot Start()
    {
        _serviceProvider = 
            new ServiceCollection()
                .AddArchetype()
                .AddSingleton<IGameState, GameState>()
                .AddSingleton(BasicRules.Create())
                // TODO: This needs to be a card loader or something.
                // It needs to be initiated after the ServiceProvider
                // has been built, and the runtime can decide when it's a good idea to do that work
                //.AddSingleton<IProtoCards, CardPool>() 
            .BuildServiceProvider();
        
        return _serviceProvider.GetService<IGameRoot>()!;
    }
}