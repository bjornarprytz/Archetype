namespace Archetype.View.Atoms.Zones;

public interface IMapNodeFront : IZoneFront
{
    IEnumerable<IUnitFront> Units { get; }

    IEnumerable<IMapNodeFront> Neighbours { get; }
        
    IGraveyardFront Graveyard { get; }
    IDiscardPileFront DiscardPile { get; }
}