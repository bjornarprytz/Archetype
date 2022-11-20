using Archetype.Core.Atoms.Zones;
using Archetype.Core.Effects;

namespace Archetype.Core.Atoms;

public interface ICard : IAtom
{
    public Guid ProtoId { get; }
}