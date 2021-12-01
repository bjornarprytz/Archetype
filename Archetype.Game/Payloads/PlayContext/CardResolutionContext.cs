using System;
using System.Collections.Generic;
using Archetype.Game.Attributes;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.PlayContext
{
    
    public interface ICardResolutionContext : IDisposable
    {
        [Target("Player")]
        IGameAtom Caster { get; }
        IEnumerable<IGameAtom> Targets { get; }
        
        [Target("World")]
        IGameState GameState { get; }
        
    }

    public interface ICardResolver
    {
        ICardResult Resolve(ICard card);
    } 
    
    public class CardResolutionContext : ICardResolutionContext, ICardResolver
    {
        private bool _resolved;
        private readonly IHistoryWriter _historyWriter;
        private readonly ICardResultCollector _result;
        
        public CardResolutionContext(IGameState gameState, IGameAtom caster, IEnumerable<IGameAtom> targets, IHistoryWriter historyWriter)
        {
            _historyWriter = historyWriter;
            GameState = gameState;
            Caster = caster;
            Targets = targets;
            _result = new CardResultCollector();
        }
        
        public IGameState GameState { get; }

        public ICardResult Resolve(ICard card)
        {
            if (_resolved) throw new ContextResolvedTwiceException(card, this);
            _resolved = true;
            
            foreach (var effect in card.Effects)
            {
                _result.AddResult(effect.ResolveContext(this));
            }
            
            _historyWriter.AddEntry(card, this, _result);

            return _result;
        }
        
        public IGameAtom Caster { get; }
        public IEnumerable<IGameAtom> Targets { get; }

        public void Dispose()
        {
            if (!_resolved) throw new ContextUnresolvedException();
        }
    }
}