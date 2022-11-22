namespace Archetype.Core.Effects;

public interface ITargetDescriptor
{
    CardType TargetType { get; }
    bool IsEnemy { get; }
}
