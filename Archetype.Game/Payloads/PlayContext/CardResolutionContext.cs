using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        
        ICardResult PartialResults { get; }
    }

    public interface ICardResolver
    {
        ICardResult Resolve(ICard card);
    }
    
    public class CardResolutionContext : ICardResolutionContext, ICardResolver
    {
        private bool _resolved;
        private readonly ICardResultCollector _result;
        public CardResolutionContext(IGameState gameState, IGameAtom caster, IEnumerable<IGameAtom> targets)
        {
            GameState = gameState;
            Caster = caster;
            Targets = targets;
            _result = new CardResultCollector();
        }

        public ICardResult PartialResults => _result;
        public IGameState GameState { get; }

        public ICardResult Resolve(ICard card)
        {
            if (_resolved)
            {
                throw new ContextResolvedTwiceException(card, this);
            }
            
            _resolved = true;

            foreach (var effect in card.Effects)
            {
                _result.AddResult(effect.ResolveContext(this));
            }

            return _result;
        }
        
        public IGameAtom Caster { get; }

        public IEnumerable<IGameAtom> Targets { get; }

        void IDisposable.Dispose() { }
    }
}