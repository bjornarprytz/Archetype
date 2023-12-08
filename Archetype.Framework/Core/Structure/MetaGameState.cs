using Archetype.Framework.Design;

namespace Archetype.Framework.Core.Structure;

public interface IMetaGameState
{
    IRules Rules { get; }
    IProtoData ProtoData { get; }
}

public class MetaGameState(IRules rules, IProtoData protoData) : IMetaGameState
{
    public IRules Rules { get; } = rules;
    public IProtoData ProtoData { get; } = protoData;
}