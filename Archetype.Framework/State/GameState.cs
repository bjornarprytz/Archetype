using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;

namespace Archetype.Framework.State;

public interface IGameState
{
    [PathPart("zones")]
    IEnumerable<IZone> GetZones();
    [PathPart("drawPile")]
    IZone GetDrawPile();
    [PathPart("hand")]
    IZone GetHand();
    [PathPart("discardPile")]
    IZone GetDiscardPile();
    [PathPart("exile")]
    IZone GetExile();

    ICardPool GetCardPool();
    
    internal IAtom? GetAtom(Guid id);
    internal void AddAtom(IAtom atom);
}

internal class GameState : IGameState
{
    public IEnumerable<IZone> GetZones()
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

    public ICardPool GetCardPool()
    {
        throw new NotImplementedException();
    }

    public IAtom? GetAtom(Guid id)
    {
        throw new NotImplementedException();
    }

    public void AddAtom(IAtom atom)
    {
        throw new NotImplementedException();
    }
}