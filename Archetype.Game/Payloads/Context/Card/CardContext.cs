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
    public interface ICardContext : IResolutionContext
    {
        [Target("Caster")]
        IPlayer Caster { get; }
        IEnumerable<IGameAtom> Targets { get; }
    }

    public interface ICardResolver
    {
        void Resolve(ICard card, IEnumerable<IGameAtom> targets);
    }
    
    public class CardContext : ICardResolver, ICardContext
    {
        private readonly IHistoryWriter _historyWriter;
        private ICard _card;
        
        private bool _resolved;
        private readonly IResolutionCollector _result;
        public CardContext(IGameState gameState, IPlayer player, IHistoryWriter historyWriter)
        {
            _historyWriter = historyWriter;
            GameState = gameState;
            Caster = player;
            _result = new ResolutionCollector();
        }

        public IResolution PartialResults => _result;

        public IGameState GameState { get; }
        public IPlayer Caster { get; }
        public IEnumerable<IGameAtom> Targets { get; private set; }
    
        public void Resolve(ICard card, IEnumerable<IGameAtom> targets)
        {
            CommitContext(card, targets);

            Caster.Resources -= card.Cost;
            
            Resolve();
        }
        
        private void CommitContext(ICard card, IEnumerable<IGameAtom> targets)
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

        private void Resolve()
        {
            if (_resolved)
            {
                throw new ContextResolvedTwiceException(_card, this);
            }
            
            _resolved = true;

            foreach (var effect in _card.Effects)
            {
                _result.AddResult(effect.ResolveContext(this));
            }

            _result.AddResult(_card.MoveTo(Caster.DiscardPile));
            
            _historyWriter.Append(_card, this, _result);
        }


        public void Dispose() { }
    }
}