namespace Archetype.View.Atoms.Zones;

public interface IHandFront : IZoneFront
{
    IEnumerable<ICardFront> Cards { get; }
}