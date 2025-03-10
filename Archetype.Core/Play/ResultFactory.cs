using System.Runtime.CompilerServices;
using Archetype.Core.Atoms.Base;
using Archetype.View.Infrastructure;

namespace Archetype.Core.Play;

internal static class ResultFactory
{
    public static IEffectResult<TAffected, TResult> Create<TAffected, TResult>(TAffected affected, TResult result, IEnumerable<IEffectResult> sideEffects=default, [CallerMemberName] string verb=default)
        where TAffected : class, IGameAtom
    {
        return new EffectResult<TAffected, TResult>(affected, verb, result, sideEffects ?? Enumerable.Empty<IEffectResult>());
    }
        
    public static IEffectResult<TResult> Create<TResult>(TResult result, IEnumerable<IEffectResult> sideEffects=default, [CallerMemberName] string verb=default)
    {
        return new EffectResult<TResult>(verb, result, sideEffects ?? Enumerable.Empty<IEffectResult>());
    }

    public static IEffectResult CreateAggregate(IEnumerable<IEffectResult> effectResults)
    {
        return new AggregateEffectResult(effectResults ?? Enumerable.Empty<IEffectResult>());
    }

    public static IEffectResult<TResult> Null<TResult>([CallerMemberName] string verb=default)
    {
        return new NullEffectResult<TResult>(verb);
    }
        
    public static IEffectResult<TAffected, TResult> Null<TAffected, TResult>([CallerMemberName] string verb=default) 
        where TAffected : class, IGameAtom
    {
        return new NullEffectResult<TAffected, TResult>(verb);
    }
}