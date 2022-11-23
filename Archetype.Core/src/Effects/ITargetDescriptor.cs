namespace Archetype.Core.Effects;

public interface ITargetDescriptor
{
    Type TargetType { get; }
    bool IsEnemy { get; }
}
