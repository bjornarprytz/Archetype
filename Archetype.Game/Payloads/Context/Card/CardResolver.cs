using System.Threading.Tasks;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Card
{
    public interface ICardResolver
    {
        void Resolve(ICardPlayArgs playArgs);
    }
    
    public interface ICardContext : IContext
    {
        ICardPlayArgs PlayArgs { get; }
    }

    public class CardResolver : ICardResolver
    {
        private readonly IGameState _gameState;
        private readonly IHistoryWriter _historyWriter;

        public CardResolver(IGameState gameState, IHistoryWriter historyWriter)
        {
            _gameState = gameState;
            _historyWriter = historyWriter;
        }
    
        public void Resolve(ICardPlayArgs playArgs)
        {
            var results = new ResolutionCollector();

            playArgs.ValidateTargets(_gameState); // TODO: This should probably be done elsewhere, or by a separate service
            
            playArgs.Player.Resources -= playArgs.Card.Cost; // TODO: Make this more expressive? (e.g. pay costs in different ways)

            var context = new CardContext(_gameState, playArgs, results);
            
            foreach (var effect in playArgs.Card.Effects)
            {
                results.AddResult(effect.ResolveContext(context));
            }

            results.AddResult(playArgs.Card.MoveTo(playArgs.Whence.DiscardPile));
            
            _historyWriter.Append(context, results);
        }
        
        private record CardContext(
                IGameState GameState,
                ICardPlayArgs PlayArgs,
                IResolution PartialResults)
            : ICardContext
        {
            public IGameAtom Owner => PlayArgs.Player;
        }
    }
}