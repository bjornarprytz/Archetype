using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Runtime.Implementation;

public class MetaGameState(IRules rules, IProtoCards protoCards) : IMetaGameState
{
    public IRules Rules { get; } = rules;
    public IProtoCards ProtoCards { get; } = protoCards;
}