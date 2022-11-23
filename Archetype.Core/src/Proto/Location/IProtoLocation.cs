namespace Archetype.Core.Proto.Location;

public interface IProtoLocation : IProtoData
{
    public static abstract LocationType Type { get; }
}