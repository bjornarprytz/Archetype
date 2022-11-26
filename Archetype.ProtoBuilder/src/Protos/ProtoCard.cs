using Archetype.Core;
using Archetype.Core.Atoms;
using Archetype.Core.Effects;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Components.Protos;

internal abstract class ProtoCard : IProtoPlayingCard
{
    public string Name { get; set; }
    public string ImageUri { get; set; }
    public string SetName { get; set; }
    public CardRarity Rarity { get; set; }
    public CardType Type { get; set; }
    public string SubType { get; set; }
    public string RulesText { get; set; }
    public CardColor Color { get; set; }
    public int Cost { get; set; }
    public int Resources { get; set; }
    
    
    public abstract IEnumerable<ITargetDescriptor> TargetDescriptors { get; }
    public abstract IResult Resolve(IContext<ICard> context);
    public abstract string ContextualRulesText(IContext<ICard> context);
}