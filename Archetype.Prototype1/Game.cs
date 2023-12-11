using Archetype.Framework.Core.Structure;
using Archetype.Framework.Extensions;

namespace Archetype.Prototype1;

// TODO: Make a package that I can use in Godot

// It should have:
// - A simple game state: Three proto cards
//  - (an enemy, a structure to defend, and a crew)
// - A small deck (10 cards)
// - A simple game loop that draws a card, waits for player input, and then resolves combat

public static class Game
{
    private static string corpus = @"{
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
            }";
    
    public static IGameRoot Start()
    {
        var gameRoot = ArchetypeExtensions
            .InitArchetype<Bootstrapper, GameState>(new Bootstrapper(corpus));

        return gameRoot;
    }
}