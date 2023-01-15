namespace Archetype.Core.Proto;

public interface IProtoStructure : IProtoCard
{
    StructureStats StructureStats { get; }
}

public record struct StructureStats(int Slots, int Power, int Health);