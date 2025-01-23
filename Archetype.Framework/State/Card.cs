using Archetype.Framework.Core;

namespace Archetype.Framework.State;

public interface ICard : IAtom
{
    
    // TODO: Something like ActionBlock to describe costs, targets and effects
}

public class Card : Atom, ICard
{
    private readonly CardProto _proto;
    
    public Card(CardProto proto)
    {
        _proto = proto;      
    }

    public string GetName()
    {
        return _proto.Name;
    }
}