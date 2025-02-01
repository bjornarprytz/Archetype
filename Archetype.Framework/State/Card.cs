using Archetype.Framework.Core;

namespace Archetype.Framework.State;

public interface ICard : IAtom
{
    [PathPart("name")]
    public string GetName();
    // TODO: Something like ActionBlock to describe costs, targets and effects
}

internal class Card : Atom, ICard
{
    private readonly ICardProto _proto;
    
    public Card(ICardProto proto)
    {
        _proto = proto;      
    }

    public string GetName()
    {
        return _proto.Name;
    }
}