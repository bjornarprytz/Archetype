namespace Archetype.Core.Proto;

public interface IProtoUnit : IProtoCard
{
    UnitStats UnitStats { get; }
}


public record struct UnitStats(int Power, int Health, int Movement);