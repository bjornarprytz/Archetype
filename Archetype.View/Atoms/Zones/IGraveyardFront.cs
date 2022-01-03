namespace Archetype.View.Atoms.Zones;

public interface IGraveyardFront : IZoneFront
{
    IEnumerable<ICreatureFront> Creatures { get; }
}