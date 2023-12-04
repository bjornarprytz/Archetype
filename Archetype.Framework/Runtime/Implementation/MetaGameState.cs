using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Implementation;

public class MetaGameState : IMetaGameState
{
    public MetaGameState(IRules rules, IProtoCards protoCards)
    {
        Rules = rules;
        ProtoCards = protoCards;
    }

    public IRules Rules { get; }
    public IProtoCards ProtoCards { get; }
}