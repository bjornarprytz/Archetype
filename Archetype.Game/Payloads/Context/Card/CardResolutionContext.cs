using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Attributes;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Card
{
    
    public interface ICardResolutionContext : IDisposable
    {
        [Target("Player")]
        IGameAtom Caster { get; }
        IEnumerable<IGameAtom> Targets { get; }
        
        [Target("World")]
        IGameState GameState { get; }
        
        IResolution PartialResults { get; }
    }

    public interface ICardResolver
    {
        IResolution Resolve();
    }

    public interface ITargetValidator
    {
        void CommitContext(ICard card, IEnumerable<IGameAtom> targets);
    }
    
    public class CardResolutionContext : ICardResolutionContext, ICardResolver, ITargetValidator
    {
        private ICard _card;
        
        private bool _resolved;
        private readonly IResolutionCollector _result;
        public CardResolutionContext(IGameState gameState, IGameAtom caster)
        {
            GameState = gameState;
            Caster = caster;
            _result = new ResolutionCollector();
        }

        public IResolution PartialResults => _result;
        public IGameState GameState { get; }
        public IGameAtom Caster { get; }
        public IEnumerable<IGameAtom> Targets { get; private set; }
    

        public IResolution Resolve()
        {
            if (_resolved)
            {
                throw new ContextResolvedTwiceException(_card, this);
            }
            
            if (_card is null)
            {
                throw new CardMissingFromResolutionException();
            }

            if (Targets is null)
            {
                throw new TargetsMissingFromResolutionException();
            }
            
            _resolved = true;

            foreach (var effect in _card.Effects)
            {
                _result.AddResult(effect.ResolveContext(this));
            }

            return _result;
        }
        
        public void CommitContext(ICard card, IEnumerable<IGameAtom> targets)
        {
            var chosenTargets = targets.ToList();
            
            var requiredTargetCount = card.Targets.Count();
            
            if (chosenTargets.Count != requiredTargetCount)
            {
                throw new TargetCountMismatchException(requiredTargetCount, chosenTargets.Count);
            }
            
            foreach (var (targetData, chosenTarget) in card.Targets.Zip(chosenTargets))
            {
                if (!targetData.ValidateContext(new TargetValidationContext(GameState, chosenTarget)))
                {
                    throw new InvalidTargetChosenException();
                }
            }
            
            Targets = chosenTargets;
            _card = card;
        }

        void IDisposable.Dispose() { }
    }
}