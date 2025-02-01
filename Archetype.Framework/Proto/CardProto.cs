using Archetype.Framework.Effects;
using Archetype.Framework.Parsing;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Core;

public interface ICardProto
{
    public string Name { get; }
    public Dictionary<string, IValue<int?>> Costs { get; }
    public IEnumerable<TargetProto> Targets { get; }
    
    public Dictionary<string, IValue<int?>> Stats { get; }
    public Dictionary<string, string[]> Facets { get; }
    public IEnumerable<string> Tags { get; }
    
    public IEnumerable<EffectProto> Effects { get; }
    public Dictionary<string, IValue<int?>> Variables { get; }
}

internal record CardProto : ICardProto
{
    public required string Name { get; init; }
    public required Dictionary<string, IValue<int?>> Costs { get; init; }
    public required IEnumerable<TargetProto> Targets { get; init; }
    public required Dictionary<string, IValue<int?>> Stats { get; init; }
    public required Dictionary<string, string[]> Facets { get; init; }
    public required IEnumerable<string> Tags { get; init; }
    public required IEnumerable<EffectProto> Effects { get; init; }
    public required Dictionary<string, IValue<int?>> Variables { get; init; }
}

/// <summary>
/// Represents where the getter is rooted. (card) => card.Name, (stats) => stats.Health, etc.
/// </summary>
public enum Whence // TODO: Evaluate if this is necessary
{
    /// <summary>
    /// 1, "SomeString", etc.
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
    public required IEnumerable<IValue> Parameters { get; init; }
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
public interface IAtomPredicate<in TAtom, out TValue> : IAtomPredicate<TAtom>
    where TAtom : IAtom
{
    IValue<TAtom, TValue> AtomValue { get; }
    IValue<TValue> CompareValue { get; }
}

public interface IAtomGroupPredicate<in TAtom, out TItem> : IAtomPredicate<TAtom>
    where TAtom : IAtom
{
    IGroup<TAtom, TItem> AtomValue { get; }
    IValue<TItem> CompareValue { get; }
}

public interface IAtomPredicate<in TAtom> : IAtomPredicate
 where TAtom : IAtom
{
    public bool EvaluateTyped(IResolutionContext context, TAtom atom);
}

public interface IAtomPredicate
{
    Type LeftType { get; }
    ComparisonOperator Operator { get; }
    Type RightType { get; }
    
    public bool Evaluate(IResolutionContext context, IAtom atom);
}

