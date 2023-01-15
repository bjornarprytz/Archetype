namespace Archetype.Core.Proto;

public interface IProtoUnit : IProtoCard
{
    UnitStats UnitStats { get; }
}


public record struct UnitStats {
    public int Power { get; }
    public int Health { get; }
    public int Movement { get; }
}