namespace Archetype.Core.Proto.Location;

public interface IProtoQuestGiver : IProtoLocation
{
    public string Description { get; }
    public Guid Goal { get; } // Proto card
}