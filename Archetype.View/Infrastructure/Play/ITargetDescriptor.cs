namespace Archetype.View.Infrastructure;

public interface ITargetDescriptor
{
    Type TargetType { get; }
    string TypeId { get; }
}