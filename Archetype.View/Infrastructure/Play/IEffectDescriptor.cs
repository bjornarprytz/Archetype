namespace Archetype.View.Infrastructure;

public interface IEffectDescriptor
{
    string Affected { get; } // Who will be affected? Usually just the target, but may be a group of things (e.g. each unit in target zone)
    string Keyword { get; } // In what way? (e.g. attack)
    IEnumerable<IOperand> Arguments { get; } // By how much?
}