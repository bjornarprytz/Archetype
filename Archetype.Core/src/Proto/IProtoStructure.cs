namespace Archetype.Core.Proto;

public interface IProtoStructure : IProtoCard
{
    public int Slots { get; }
    public int Strength { get; }
    public int Defense { get; }
}