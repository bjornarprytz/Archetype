namespace Archetype.Core.Proto.PlayingCard;

public interface IProtoStructure : IProtoPlayingCard
{
    public int Slots { get; }
    public int Strength { get; }
    public int Defense { get; }
}