using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context.Effect.Base;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.View.Atoms;
using Archetype.View.Infrastructure;

namespace Archetype.Game.Payloads.Context
{
    public interface IContext<out TSource> : IContext
        where TSource : IGameAtom
    {
        new TSource Source { get; }
    }
    
    public interface IContext : IDisposable
    {
        IGameState GameState { get; }
        IHistoryReader History { get; }
        IGameAtom Source { get; }
        IMapNode Whence { get; }
        IEffectProvider EffectProvider { get; }
        ITargetProvider TargetProvider { get; }
    }

    public interface IResultsReader
    {
        IEnumerable<IEffectResult> Results { get; }
    }

    internal interface IResultsWriter
    {
        void AddResult(IEffectResult effectEffectResult);
    }

    internal interface IResultsReaderWriter : IResultsReader, IResultsWriter { }

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

    internal record NullEffectResult<TAffected, T>(TAffected Affected, string Verb)
        : EffectResult<TAffected, T>(Affected, Verb, default, Enumerable.Empty<IEffectResult>()) where TAffected : class, IGameAtom;

    internal record AggregateEffectResult(IEnumerable<IEffectResult> SideEffects) : IAggregateEffectResult
    {
        public bool IsNull => false;
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

    internal class ResultsReaderWriter : IResultsReaderWriter, IResultsReader
    {
        private readonly List<IEffectResult> _results = new();

        public IEnumerable<IEffectResult> Results => _results;

        public void AddResult(IEffectResult effectEffectResult)
        {
            _results.Add(effectEffectResult);
        }
    }
}