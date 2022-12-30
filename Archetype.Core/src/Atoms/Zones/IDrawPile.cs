using Archetype.Core.Atoms.Cards;
using Archetype.Core.Effects;

namespace Archetype.Core.Atoms.Zones;

public interface IDrawPile : IZone<ICard>
{
    public int Count { get; }
    
    public ICard Draw(); 
    /*
     * TODO: there is some funky stuff going on with this design.
     * There is a problem with how accessible the Draw method is.
     * It's convenient to have it here, but it's also a bit of a leaky abstraction.
     *
     * Maybe keywords should be implemented separately, and the atoms should be oblivious to how they can be changed.
    */ 
    public IResult Shuffle();
}