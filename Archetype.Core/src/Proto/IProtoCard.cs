using Archetype.Core.Effects;

namespace Archetype.Core.Proto;

public interface IProtoCard
{
    public string Name { get; }
    public CardStats Stats { get; }
    public CardMetaData Meta { get; }
    public IEnumerable<ITargetDescriptor> TargetDescriptors { get; } // ordered
    public IResult Resolve(IContext context);
    public string ContextualRulesText(IContext context);
}

public record struct CardStats(
    int Cost, 
    int Value,
    CardType Type,
    IReadOnlyList<string> Tags,
    CardColor Color
);

public record struct CardMetaData( // Immutable data
    string SetName,
    string ImageUri,
    CardRarity Rarity,
    string StaticRulesText
);