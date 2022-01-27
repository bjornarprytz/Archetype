using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Infrastructure;

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
        IEnumerable<IResult> Results { get; }
    }

    internal interface IResultsWriter
    {
        void AddResult(IResult effectResult);
    }

    internal interface IResultsReaderWriter : IResultsReader, IResultsWriter { }

    public interface IResult
    {
        bool IsNull { get; }
        IEnumerable<IGameAtom> AllAffected { get; }
        string Verb { get; }
        object Result { get; }
    }

    public interface IResult<out T> : IResult
    {
        new T Result { get; }
    }

    public interface IResult<out TTarget, out TResult> : IResult<TResult>
        where TTarget : class, IGameAtom
    {
        TTarget Affected { get; }
    }

    internal record AggregatedEffectResult<TResult> : IResult<IEnumerable<TResult>>
    {
        internal AggregatedEffectResult(IEnumerable<IResult<TResult>> results)
        {
            var nonNullResults = results.Where(r => !r.IsNull).ToList();

            AllAffected = nonNullResults.SelectMany(r => r.AllAffected).ToList();
            Verb = nonNullResults.FirstOrDefault()?.Verb ?? throw new ArgumentException("Effect result missing Verb.");
            Result = nonNullResults.Select(r => r.Result);
        }

        public bool IsNull => false;
        public IEnumerable<IGameAtom> AllAffected { get; }
        public string Verb { get; }
        object IResult.Result => Result;

        public IEnumerable<TResult> Result { get; }
    }

    internal record NullResult<T>(string Verb)
        : EffectResult<T>(Verb, default);

    internal record NullResult<TAffected, T>(TAffected Affected, string Verb)
        : EffectResult<TAffected, T>(Affected, Verb, default) where TAffected : class, IGameAtom;

    internal record EffectResult<T>(string Verb, T Result) : IResult<T>
    {
        public bool IsNull => Result is not null;
        public IEnumerable<IGameAtom> AllAffected => Enumerable.Empty<IGameAtom>();

        object IResult.Result => Result;
    }

    internal record EffectResult<TAffected, TResult>(TAffected Affected, string Verb, TResult Result)
        : IResult<TAffected, TResult> where TAffected : class, IGameAtom
    {
        public bool IsNull => Result is null;
        public IEnumerable<IGameAtom> AllAffected => new[] { Affected };
        object IResult.Result => Result;
    }

    internal class ResultsReaderWriter : IResultsReaderWriter, IResultsReader
    {
        private readonly List<IResult> _results = new();

        public IEnumerable<IResult> Results => _results;

        public void AddResult(IResult effectResult)
        {
            _results.Add(effectResult);
        }
    }
}