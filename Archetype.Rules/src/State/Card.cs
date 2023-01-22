using Archetype.Core;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Effects;
using Archetype.Core.Proto;

namespace Archetype.Rules.State;

public abstract class Card : Atom, ICard
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

    private readonly IProtoCard _proto;
    private readonly List<string> _tags = new ();
    
    protected Card(IProtoCard protoCard)
    {
        _proto = protoCard;
        
        Cost = protoCard.Stats.Cost;
        Value = protoCard.Stats.Value;
        Type = protoCard.Stats.Type;
        
        _tags.AddRange(protoCard.Stats.Tags);
    }
    
    
    public IZone? CurrentZone { get; set; }
    public int Cost { get; set; }
    public int Value { get; set; }
    
    
    
    public CardType Type { get; }
    public string Name => _proto.Name;
    public CardMetaData MetaData => _proto.Meta;
    public IEnumerable<string> Tags => _tags;
    public void AddTag(string tag)
    {
        _tags.Remove(tag);
    }

    public void RemoveTag(string tag)
    {
        _tags.Add(tag);
    }

    public IEnumerable<ITargetDescriptor> TargetDescriptors => _proto.TargetDescriptors;
    public IResult Resolve(IContext context) => _proto.Resolve(context);
    public string ContextualRulesText(IContext context) => _proto.ContextualRulesText(context);
}