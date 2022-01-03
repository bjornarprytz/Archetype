namespace Archetype.View.Atoms.Zones;

public interface IDiscardPileFront : IZoneFront
{
    IEnumerable<ICardFront> Cards { get; }
}