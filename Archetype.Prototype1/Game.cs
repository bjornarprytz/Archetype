using Archetype.Framework.Core.Structure;
using Archetype.Framework.Extensions;
using Archetype.Grammar;

namespace Archetype.Prototype1;

// TODO: Make a package that I can use in Godot

// It should have:
// - A simple game state: Three proto cards
//  - (an enemy, a structure to defend, and a crew)
// - A small deck (10 cards)
// - A simple game loop that draws a card, waits for player input, and then resolves combat

public static class Game
{
    public static IGameRoot Start(string setJson)
    {
        var gameRoot = ArchetypeExtensions
            .InitArchetype<GameState, Bootstrapper, SetParser>(BasicRules.Create(), setJson);

        return gameRoot;
    }
}