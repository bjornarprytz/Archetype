using Archetype.Framework.Effects;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Core;

public interface ICardProto
{
    public string Name { get; }
    public Dictionary<string, INumber> Costs { get; }
    public IEnumerable<TargetProto> Targets { get; }
    
    public Dictionary<string, INumber> Stats { get; }
    public Dictionary<string, string[]> Facets { get; }
    public IEnumerable<string> Tags { get; }
    
    public IEnumerable<EffectProto> Effects { get; }
    public Dictionary<string, INumber> Variables { get; }
}

public record CardProto : ICardProto
{
    public required string Name { get; init; }
    public required Dictionary<string, INumber> Costs { get; init; }
    public required IEnumerable<TargetProto> Targets { get; init; }
    public required Dictionary<string, INumber> Stats { get; init; }
    public required Dictionary<string, string[]> Facets { get; init; }
    public required IEnumerable<string> Tags { get; init; }
    public required IEnumerable<EffectProto> Effects { get; init; }
    public required Dictionary<string, INumber> Variables { get; init; }
}

public enum Whence
{
    /// <summary>
    /// 1, "Card", etc.
    /// </summary>
    Immediate,
    /// <summary>
    /// Stats, Facets, Tags, etc.
    /// </summary>
    Atom,
    /// <summary>
    /// Scope, State, Targets, etc.
    /// </summary>
    Context,
}

public record EffectProto
{
    public required string Keyword { get; init; }
    public required IEnumerable<INumber> Parameters { get; init; }
}

public record TargetProto
{
    public required IEnumerable<IAtomPredicate> Predicates { get; init; }
}


public enum ComparisonOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    NotContains,
}
public interface IAtomPredicate<out T> : IAtomPredicate
{
    new IAtomValue<T> AtomValue { get; }
    new ComparisonOperator Operator { get; }
    new IValue<IValueWhence, T> CompareValue { get; }
    
}

public interface IAtomPredicate
{
    IAtomValue AtomValue { get; }
    ComparisonOperator Operator { get; }
    IValue CompareValue { get; }
    public bool Evaluate(IResolutionContext context, IAtom atom);
    
}

