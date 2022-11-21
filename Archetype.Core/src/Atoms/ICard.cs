using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Atoms;

public interface ICard : IAtom
{
    public Guid ProtoId { get; } // TODO: Or reference to the proto card?
    public IZone CurrentZone { get; }
}