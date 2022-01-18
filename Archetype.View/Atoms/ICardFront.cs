using Archetype.View.Atoms.MetaData;
using Archetype.View.Infrastructure;

namespace Archetype.View.Atoms;

public interface ICardFront : IZonedFront, IGameAtomFront
{
    CardMetaData MetaData { get; }
    int Cost { get; }
    int Range { get; }
    
    IEnumerable<ITargetDescriptor> TargetDescriptors { get; } // In-order
    IEnumerable<IEffectDescriptor> EffectDescriptors { get; } // In-order
}