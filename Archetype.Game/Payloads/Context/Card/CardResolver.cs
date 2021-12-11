using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Card
{
    public interface ICardResolver
    {
        void Resolve(ICard card, IEnumerable<IGameAtom> targets);
    }
    
    public interface ICardContext : IResolutionContext
    {
        IPlayer Caster { get; }
        IEnumerable<IGameAtom> Targets { get; }
    }

    public class CardResolver : ICardResolver
    {
        private readonly IGameState _gameState;
        private readonly IPlayer _player;
        private readonly IHistoryWriter _historyWriter;
        
        public CardResolver(IGameState gameState, IPlayer player, IHistoryWriter historyWriter)
        {
            _gameState = gameState;
            _player = player;
            _historyWriter = historyWriter;
        }
    
        public void Resolve(ICard card, IEnumerable<IGameAtom> targets)
        {
            // TODO: Cleanup in here
            
            var results = new ResolutionCollector();
            
            var validTargets = ValidateTargets(card, targets);  

            _player.Resources -= card.Cost; // TODO: Make this more expressive?

            var context = new CardContext(_gameState, validTargets, _player, results);
            
            foreach (var effect in card.Effects)
            {
                results.AddResult(effect.ResolveContext(context));
            }

            results.AddResult(card.MoveTo(_player.DiscardPile));
            
            _historyWriter.Append(card, context, results);
        }
        
        private IEnumerable<IGameAtom> ValidateTargets(ICard card, IEnumerable<IGameAtom> targets)
        {
            var chosenTargets = targets.ToList();
            
            var requiredTargetCount = card.Targets.Count();
            
            if (chosenTargets.Count != requiredTargetCount)
            {
                throw new TargetCountMismatchException(requiredTargetCount, chosenTargets.Count);
            }
            
            foreach (var (targetData, chosenTarget) in card.Targets.Zip(chosenTargets))
            {
                if (!targetData.ValidateContext(new TargetValidationContext(_gameState, chosenTarget)))
                {
                    throw new InvalidTargetChosenException();
                }
            }
            
            return chosenTargets;
        }

        private record CardContext(
                IGameState GameState,
                IEnumerable<IGameAtom> Targets,
                IPlayer Caster,
                IResolution PartialResults)
            : ICardContext;
    }
}