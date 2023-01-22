using Archetype.Core;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Effects;
using Archetype.Core.Proto;

namespace Archetype.Rules.State;

public abstract class Card : Atom, ICard
{
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