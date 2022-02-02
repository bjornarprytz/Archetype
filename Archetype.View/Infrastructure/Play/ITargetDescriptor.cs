namespace Archetype.View.Infrastructure.Play;

public interface ITargetDescriptor
{
    Type TargetType { get; }
    string TypeId { get; }
}