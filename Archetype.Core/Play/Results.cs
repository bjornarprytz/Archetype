using Archetype.Core.Atoms.Base;
using Archetype.Core.Extensions;
using Archetype.View.Atoms;
using Archetype.View.Infrastructure;

namespace Archetype.Core.Play;

public interface IEffectResult
{
    IEnumerable<IEffectResult> SideEffects { get; }

    string Verb { get; }
    bool IsNull { get; }
    IGameAtomFront Affected { get; }
    object Result { get; }
}

public interface IEffectResult<out T> : IEffectResult
{
    new T Result { get; }
}

public interface IEffectResult<out TTarget, out TResult> : IEffectResult<TResult>
    where TTarget : class, IGameAtom
{
    new TTarget Affected { get; }
}
    
public interface IAggregateEffectResult : IEffectResult { } // Marker interface

internal record NullEffectResult<T>(string Verb)
    : EffectResult<T>(Verb, default, Enumerable.Empty<IEffectResult>());

internal record NullEffectResult<TAffected, T>(string Verb)
    : EffectResult<TAffected, T>(default, Verb, default, Enumerable.Empty<IEffectResult>()) where TAffected : class, IGameAtom;

internal record AggregateEffectResult(IEnumerable<IEffectResult> SideEffects) : IAggregateEffectResult
{
    public bool IsNull => SideEffects is null || SideEffects.IsEmpty();
    public string Verb => null;
    public IGameAtomFront Affected => null;
    public object Result => null;
}

internal record EffectResult<T>(
        string Verb, 
        T Result, 
        IEnumerable<IEffectResult> SideEffects) 
    : IEffectResult<T>
{
    public bool IsNull => Result is not null;
    public IGameAtomFront Affected => null;

    object IEffectResult.Result => Result;
}

internal record EffectResult<TAffected, TResult>(
        TAffected Affected, 
        string Verb, 
        TResult Result, 
        IEnumerable<IEffectResult> SideEffects)
    : IEffectResult<TAffected, TResult> where TAffected : class, IGameAtom
{
    public bool IsNull => Result is null;
    IGameAtomFront IEffectResult.Affected => Affected;
    object IEffectResult.Result => Result;
}