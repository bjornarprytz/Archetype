using Archetype.Framework.Core;

namespace Archetype.Framework.State;

public interface ICard : IAtom
{
    public string GetName();
    
    // TODO: Something like ActionBlock to describe costs, targets and effects
}

public class Card : Atom, ICard
{
    private CardProto _proto;
    
    public Card(CardProto proto)
    {
        _proto = proto;      
    }

    public string GetName()
    {
        return _proto.Name;
    }
}