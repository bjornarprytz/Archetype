namespace Archetype.Core.Effects.Targeting;

public interface ITargetDescriptor
{
    CardType TargetType { get; }
    bool IsEnemy { get; }
}
