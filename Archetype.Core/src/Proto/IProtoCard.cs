using Archetype.Core.Atoms.Cards;
using Archetype.Core.Effects;

namespace Archetype.Core.Proto;

public interface IProtoCard
{
    public string Name { get; }
    public CardStats Stats { get; }
    public CardMetaData Meta { get; }
    public IEnumerable<ITargetDescriptor> TargetDescriptors { get; } // ordered
    public IResult Resolve(IContext context);
    public string ContextualRulesText(IContext<ICard> context);
}

public record struct CardStats(
    int Cost, 
    int Value,
    CardType Type,
    string SubType,
    CardColor Color
);

public record struct CardMetaData( // Immutable data
    string SetName,
    string ImageUri,
    CardRarity Rarity,
    string StaticRulesText
);