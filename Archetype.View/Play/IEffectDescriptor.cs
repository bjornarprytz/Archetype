using OneOf;

namespace Archetype.View.Play;

public interface IEffectDescriptor
{
    IAffected Affected { get; }
    string Keyword { get; }
    IEnumerable<IOperand> Operands { get; }
}

public interface IAffected
{
    OneOf<ITargetProperty, IContextProperty> Description { get; }
}

