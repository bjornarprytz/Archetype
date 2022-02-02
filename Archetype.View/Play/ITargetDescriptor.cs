namespace Archetype.View.Play;

public interface ITargetDescriptor
{
    Type TargetType { get; }
    string TypeId { get; }
}