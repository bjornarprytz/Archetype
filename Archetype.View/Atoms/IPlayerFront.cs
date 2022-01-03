using Archetype.View.Atoms.Zones;

namespace Archetype.View.Atoms;

public interface IPlayerFront : IGameAtomFront
{
    int MaxHandSize { get; }
    int Resources { get; set; }
    IStructureFront HeadQuarters { get; }
    IDeckFront Deck { get; }
    IHandFront Hand { get; }
}