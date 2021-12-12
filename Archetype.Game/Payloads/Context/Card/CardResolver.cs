using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Exceptions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Card
{
    public interface ICardResolver
    {
        void Resolve(ICardPlayArgs playArgs);
    }
    
    public interface ICardContext : IResolutionContext
    {
        ICardPlayArgs PlayArgs { get; }
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
    
        public void Resolve(ICardPlayArgs playArgs)
        {
            var results = new ResolutionCollector();

            playArgs.ValidateTargets(_gameState);
            
            playArgs.Player.Resources -= playArgs.Card.Cost; // TODO: Make this more expressive? (e.g. pay costs in different ways)

            var context = new CardContext(_gameState, playArgs, results);
            
            foreach (var effect in playArgs.Card.Effects)
            {
                results.AddResult(effect.ResolveContext(context));
            }

            results.AddResult(playArgs.Card.MoveTo(_player.DiscardPile));
            
            _historyWriter.Append(context, results);
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
                ICardPlayArgs PlayArgs,
                IResolution PartialResults)
            : ICardContext;
    }
}