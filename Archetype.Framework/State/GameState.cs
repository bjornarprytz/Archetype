using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;

namespace Archetype.Framework.State;

public interface IGameState
{
    [PathPart("player")]
    IPlayer GetPlayer();
    
    [PathPart("zones")]
    IEnumerable<IZone> GetZones();
    
    [PathPart("exile")]
    IZone GetExile();

    ICardPool GetCardPool();
    
    internal IAtom? GetAtom(Guid id);
    internal void AddAtom(IAtom atom);
}

internal class GameState : IGameState
{
    public IPlayer GetPlayer()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IZone> GetZones()
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