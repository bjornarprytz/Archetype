using Archetype.Framework.Core;
using Archetype.Framework.Events;
using Archetype.Framework.Resolution;

namespace Archetype.Framework.State;

public interface ICard : IAtom, IActionBlock
{
    [PathPart("name")]
    public string GetName();
    
    public ICardProto GetProto();
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

    public ICardProto GetProto()
    {
        return _proto;
    }

    public IAtom Source => this;
    public IReadOnlyDictionary<string, IValue<int?>> Costs => _proto.Costs;
    public IEnumerable<TargetProto> Targets => _proto.Targets;
    public IEnumerable<EffectProto> Effects => _proto.Effects;
}