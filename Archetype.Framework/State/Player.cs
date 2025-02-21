namespace Archetype.Framework.State;

public interface IPlayer : IAtom
{
    [PathPart("resource")]
    int? GetResouceCount(string key);
    void SetResourceCount(string key, int value);
    
    [PathPart("drawPile")]
    IZone GetDrawPile();
    [PathPart("hand")]
    IZone GetHand();
    [PathPart("discardPile")]
    IZone GetDiscardPile();
}

internal class Player : Atom, IPlayer
{
    public int? GetResouceCount(string key)
    {
        throw new NotImplementedException();
    }

    public void SetResourceCount(string key, int value)
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
}