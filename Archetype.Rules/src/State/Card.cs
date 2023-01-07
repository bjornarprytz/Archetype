using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Proto;

namespace Archetype.Rules.State;

public class Card : Atom, ICard
{
    // TODO: Differentiate between Units, Spells, and Structures.
    // Is Cards a useful abstraction over those types?
    // Units:
    // - Target Nodes when played
    // - Enter play in that node.
    // - Can move and attack.
    // - Can be destroyed. Going to graveyard.
    // - Can be modified.
    // - Can have effects that trigger when they enter play, move, attack, or are destroyed or attacked.

    // Structures:
    // - Target Nodes when played
    // - Enter play in that node.
    // - Can be destroyed. Does not go to graveyard, but maybe leaves a token of resources for the player?
    // - Can be modified.
    // - Can have effects that trigger when are destroyed or attacked.
    // - Can have effects that trigger when a unit enters play in the same node.
    // - Can have static effects that modify units in the same node, or globally.

    // Spells:
    // - Can target any atom.
    // - Can essentially do anything.
    // - Go to the graveyard when played
    
    
    public IZone? CurrentZone { get; set; }
    public IProtoCard Proto { get; }
}