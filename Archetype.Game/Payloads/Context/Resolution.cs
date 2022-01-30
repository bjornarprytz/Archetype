using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
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
        TTarget Affected { get; }
    }

    public interface IAggregatedEffectResult<out TResult> : IEffectResult<IEnumerable<TResult>> { } // TODO: Refactor this to be more like a list of results. Problem now is that results and affected are two separate lists that the client will have to merge 

    internal record AggregatedEffectResult<TResult> : IAggregatedEffectResult<TResult>
    {
        internal AggregatedEffectResult(IEnumerable<IEffectResult<TResult>> results)
        {
            var nonNullResults = results.Where(r => !r.IsNull).ToList();

            IsNull = nonNullResults.IsEmpty();
            
            Verb = nonNullResults.FirstOrDefault()?.Verb;
            AllAffected = nonNullResults.SelectMany(r => r.AllAffected).ToList();
            Result = nonNullResults.Select(r => r.Result).ToList();
        }

        public bool IsNull { get; }
        
        public IEnumerable<IGameAtomFront> AllAffected { get; }
        public string Verb { get; }
        object IEffectResult.Result => Result;

        public IEnumerable<TResult> Result { get; }
    }

    internal record NullEffectResult<T>(string Verb)
        : EffectEffectResult<T>(Verb, default);

    internal record NullEffectResult<TAffected, T>(TAffected Affected, string Verb)
        : EffectEffectResult<TAffected, T>(Affected, Verb, default) where TAffected : class, IGameAtom;

    internal record EffectEffectResult<T>(string Verb, T Result) : IEffectResult<T>
    {
        public bool IsNull => Result is not null;
        public IEnumerable<IGameAtomFront> AllAffected => Enumerable.Empty<IGameAtomFront>();

        object IEffectResult.Result => Result;
    }

    internal record EffectEffectResult<TAffected, TResult>(TAffected Affected, string Verb, TResult Result)
        : IEffectResult<TAffected, TResult> where TAffected : class, IGameAtom
    {
        public bool IsNull => Result is null;
        public IEnumerable<IGameAtomFront> AllAffected => new[] { Affected };
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