using Archetype.Framework.Events;

namespace Archetype.Framework.State;

public interface IGameState
{
    IEnumerable<IZone> GetZones();
    
    IZone GetDrawPile();
    IZone GetHand();
    IZone GetDiscardPile();
    IZone GetExile();
}

public class GameState : IGameState
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
}