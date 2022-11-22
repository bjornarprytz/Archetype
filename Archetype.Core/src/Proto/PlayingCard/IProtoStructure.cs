namespace Archetype.Core.Proto;

public interface IProtoStructure : IProtoPlayingCard
{
    public int Slots { get; }
    public int Cost { get; }
    public int Strength { get; }
    public int Defense { get; }
}