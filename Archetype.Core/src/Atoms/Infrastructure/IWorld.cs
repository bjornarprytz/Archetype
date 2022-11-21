using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Atoms.Infrastructure;

public interface IWorld : IAtom
{
    public IDeck WorldDeck { get; }
    public IEnumerable<ILocation> Locations { get; }
}