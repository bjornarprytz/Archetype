namespace Archetype.View.Atoms.Zones;

public interface IMapNodeFront : IZoneFront
{
    int MaxStructures { get; }
    
    IEnumerable<IUnitFront> Units { get; }

    IEnumerable<IMapNodeFront> Neighbours { get; }
        
    IGraveyardFront Graveyard { get; }
    IDiscardPileFront DiscardPile { get; }
}