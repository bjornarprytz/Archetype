using Archetype.Framework.Design;

namespace Archetype.Framework.Core.Structure;

public interface IMetaGameState
{
    IRules Rules { get; }
    IProtoCards ProtoCards { get; }
}

public class MetaGameState(IRules rules, IProtoCards protoCards) : IMetaGameState
{
    public IRules Rules { get; } = rules;
    public IProtoCards ProtoCards { get; } = protoCards;
}