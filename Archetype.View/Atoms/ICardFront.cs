using Archetype.View.Atoms.MetaData;
using Archetype.View.Infrastructure;

namespace Archetype.View.Atoms;

public interface ICardFront : IZonedFront
{
    CardMetaData MetaData { get; }
    int Cost { get; }
    int Range { get; }
    string RulesText { get; } 
    IEnumerable<ITargetFront> Targets { get; }
}