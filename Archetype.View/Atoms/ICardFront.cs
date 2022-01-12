using Archetype.View.Atoms.MetaData;
using Archetype.View.Infrastructure;

namespace Archetype.View.Atoms;

public interface ICardFront : IZonedFront, IGameAtomFront
{
    CardMetaData MetaData { get; }
    string RulesText { get; }
    int Cost { get; }
    int Range { get; }
    IEnumerable<ITargetDescriptor> Targets { get; }
}