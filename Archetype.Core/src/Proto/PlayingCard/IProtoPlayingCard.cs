using Archetype.Core.Atoms;
using Archetype.Core.Effects;
using Archetype.Core.Infrastructure;

namespace Archetype.Core.Proto.PlayingCard;

public interface IProtoPlayingCard : IProtoData
{
    public string ImageUri { get; }
    public string SetName { get; }
    public CardRarity Rarity { get; }
    public CardType Type { get; }
    public string SubType { get; }
    public string RulesText { get; }
    public CardColor Color { get; }
    
    public int Cost { get; }
    public int Resources { get; } // To pay for other cards' costs
    public IEnumerable<ITargetDescriptor> TargetDescriptors { get; } // ordered
    public IResult Resolve(IContext<ICard> context);
    public string ContextualRulesText(IContext<ICard> context);
}