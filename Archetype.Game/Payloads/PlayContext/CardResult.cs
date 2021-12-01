using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.PlayContext
{
    public interface ICardResult
    {
        IReadOnlyList<IEffectResult> Results { get; }
        IReadOnlyDictionary<string, int> VerbTotal { get; }
    }

    public interface ICardResultCollector : ICardResult
    {
        void AddResult(IEffectResult effectResult);
    }

    public interface IEffectResult
    {
        IEnumerable<IGameAtom> Targets { get; }
        string Verb { get; }
        int Result { get; } // Is this the right result type? Probably
    }

    public interface IEffectResult<out T> : IEffectResult 
        where T : IGameAtom
    {
        T Target { get; }
    }

    public record AggregatedEffectResult : IEffectResult
    {
        internal AggregatedEffectResult(ICollection<IEffectResult> results)
        {
            Targets = results.SelectMany(r => r.Targets).ToList();
            Verb = results.FirstOrDefault()?.Verb ?? throw new EffectResultMissingVerbException();
            Result = results.Sum(r => r.Result);
        }
        
        public IEnumerable<IGameAtom> Targets { get; }
        public string Verb { get; }
        public int Result { get; }
    }

    internal record EffectResult(string Verb, int Result) : IEffectResult
    {
        public IEnumerable<IGameAtom> Targets => Enumerable.Empty<IGameAtom>();
    }

    internal record EffectResult<T>(T Target, string Verb, int Result) : IEffectResult<T> where T : IGameAtom
    {
        public IEnumerable<IGameAtom> Targets => new[] { Target as IGameAtom };
    }

    public class CardResultCollector : ICardResultCollector
    {
        private readonly List<IEffectResult> _results = new ();
        private readonly Dictionary<string, int> _verbTotal = new ();
        
        public IReadOnlyList<IEffectResult> Results => _results;

        public IReadOnlyDictionary<string, int> VerbTotal => _verbTotal;

        public void AddResult(IEffectResult effectResult)
        {
            _results.Add(effectResult);

            var currentTotal = _verbTotal[effectResult.Verb];
            _verbTotal[effectResult.Verb] = currentTotal + effectResult.Result;
        }
    }
}