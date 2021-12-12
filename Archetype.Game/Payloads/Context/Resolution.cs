using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context
{
    public interface IResolutionContext
    {
        IGameState GameState { get; }
        IResolution PartialResults { get; }
    }
    
    public interface IResolution
    {
        IEnumerable<IEffectResult> Results { get; }
    }

    public interface IResolutionCollector: IResolution
    {
        void AddResult(IEffectResult effectResult);
    }

    public interface IEffectResult
    {
        bool IsNull { get; }
        IEnumerable<IGameAtom> AllAffected { get; }
        string Verb { get; }
        object Result { get; }
    }

    public interface IEffectResult<out T> : IEffectResult
    {
        new T Result { get; }
    }

    public interface IEffectResult<out TTarget, out TResult> : IEffectResult<TResult>
        where TTarget : class, IGameAtom
    {
        TTarget Affected { get; }
    }

    public record AggregatedEffectResult<TResult> : IEffectResult<IEnumerable<TResult>>
    {
        internal AggregatedEffectResult(ICollection<IEffectResult<TResult>> results)
        {
            var nonNullResults = results.Where(r => !r.IsNull).ToList();
            
            AllAffected = nonNullResults.SelectMany(r => r.AllAffected).ToList();
            Verb = nonNullResults.FirstOrDefault()?.Verb ?? throw new EffectResultMissingVerbException();
            Result = nonNullResults.Select(r => r.Result);
        }

        public bool IsNull => false;
        public IEnumerable<IGameAtom> AllAffected { get; }
        public string Verb { get; }
        object IEffectResult.Result => Result;

        public IEnumerable<TResult> Result { get; }
    }

    internal record NullResult<T>(string Verb)
        : EffectResult<T>(Verb, default);

    internal record NullResult<TAffected, T>(TAffected Affected, string Verb)
        : EffectResult<TAffected, T>(Affected, Verb, default) where TAffected : class, IGameAtom;
    
    internal record EffectResult<T>(string Verb, T Result) : IEffectResult<T>
    {
        public bool IsNull => Result is not null;
        public IEnumerable<IGameAtom> AllAffected => Enumerable.Empty<IGameAtom>();
        
        object IEffectResult.Result => Result;
    }

    internal record EffectResult<TAffected, TResult>(TAffected Affected, string Verb, TResult Result) 
        : IEffectResult<TAffected, TResult> where TAffected : class, IGameAtom
    {
        public bool IsNull => Result is not null;
        public IEnumerable<IGameAtom> AllAffected => new[] { Affected };
        object IEffectResult.Result => Result;
    }

    public class ResolutionCollector : IResolutionCollector
    {
        private readonly List<IEffectResult> _results = new ();
        
        public IEnumerable<IEffectResult> Results => _results;

        public void AddResult(IEffectResult effectResult)
        {
            _results.Add(effectResult);
        }
    }
}