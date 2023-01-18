using Archetype.Core.Effects;
using Archetype.Core.Triggers;

namespace Archetype.Core.Proto;

public interface IProtoCard
{
    string Name { get; }
    CardStats Stats { get; }
    CardMetaData Meta { get; }
    IEnumerable<ITrigger> Triggers { get; }
    IEnumerable<ITargetDescriptor> TargetDescriptors { get; } // ordered
    IResult Resolve(IContext context);
    string ContextualRulesText(IContext context);
    
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