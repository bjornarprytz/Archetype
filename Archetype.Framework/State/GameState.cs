using Archetype.Framework.Events;

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