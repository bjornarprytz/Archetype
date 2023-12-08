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
                .AddSingleton<Bootstrapper>()
            .BuildServiceProvider();
        
        var bootstrapper = _serviceProvider.GetService<Bootstrapper>()!;
        
        bootstrapper.Bootstrap( // TODO: Make this an actual usable string
            @"{
                ""name"": ""Test Set"",
                ""cards"": [
                    {
                        ""name"": ""Test Card"",
                        ""keywords"": [""Test""],
                        ""cost"": 1,
                        ""attack"": 1,
                        ""health"": 1
                    }
                ]
            }");
        
        return _serviceProvider.GetService<IGameRoot>()!;
    }
}