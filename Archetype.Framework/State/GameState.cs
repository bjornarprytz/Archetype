using Archetype.Framework.Events;

namespace Archetype.Framework.State;

public interface IGameState
{
    IAtom? GetAtom(Guid id);
    IZone[] GetZones();
    
    IZone GetDrawPile();
    IZone GetHand();
    IZone GetDiscardPile();
    IZone GetExile();
    
    void AddAtom(IAtom atom);
}

public class GameState : IGameState
{
    public IAtom? GetAtom(Guid id)
    {
        throw new NotImplementedException();
    }

    public IZone[] GetZones()
    {
        throw new NotImplementedException();
    }

    public IZone GetDrawPile()
    {
        throw new NotImplementedException();
    }

    public IZone GetHand()
    {
        throw new NotImplementedException();
    }

    public IZone GetDiscardPile()
    {
        throw new NotImplementedException();
    }

    public IZone GetExile()
    {
        throw new NotImplementedException();
    }

    public void AddAtom(IAtom atom)
    {
        throw new NotImplementedException();
    }
}