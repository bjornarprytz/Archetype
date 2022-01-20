using OneOf;

namespace Archetype.View.Infrastructure;

public interface IEffectDescriptor
{
    
    IAffected Affected { get; }
    string Keyword { get; } // In what way? (e.g. attack)
    IEnumerable<IOperand> Arguments { get; } // By how much?
}

public interface IAffected
{
    OneOf<ITargetProperty, IContextProperty> Description { get; }
}