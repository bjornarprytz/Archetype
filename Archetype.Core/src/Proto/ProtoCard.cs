using Archetype.Core.Effects;

namespace Archetype.Core.Proto;

public interface IProtoCard : IProto
{
    public int Cost { get; }
    public string RulesText { get; }
    
    public IEnumerable<IEffect> Effects { get; }
    public IEnumerable<ITargetDescriptor> TargetDescriptors { get; } // ordered

    public string SetName { get; }
    public CardRarity Rarity { get; }
    public CardColor Color { get; }
    public string ImageUri { get; }
}

