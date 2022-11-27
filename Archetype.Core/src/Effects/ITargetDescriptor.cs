namespace Archetype.Core.Effects;

public interface ITargetDescriptor
{
    Type TargetType { get; }
    int TargetIndex { get; }
}
