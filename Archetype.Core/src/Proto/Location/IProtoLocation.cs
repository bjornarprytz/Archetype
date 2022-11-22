namespace Archetype.Core.Proto;

public interface IProtoLocation : IProtoData
{
    public static abstract LocationType Type { get; }
}