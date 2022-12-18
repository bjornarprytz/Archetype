using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Effects;

namespace Archetype.Core.Proto.PlayingCard;

public interface IProtoPlayingCard : IProtoData
{
    public CardStats Stats { get; }
    public CardMetaData Meta { get; }
    public IEnumerable<ITargetDescriptor> TargetDescriptors { get; } // ordered
    public IResult Resolve(IContext<ICard> context);
    public string ContextualRulesText(IContext<ICard> context);
}

public record struct CardStats(
    int Cost, 
    int Resources, // To pay for other cards' costs
    CardType Type,
    string SubType,
    CardColor Color
);

public record struct CardMetaData( // Immutable data
    string Name,
    string SetName,
    string ImageUri,
    CardRarity Rarity,
    string StaticRulesText
    );