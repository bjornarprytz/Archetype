using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context.Card
{
    public interface ICardResolver
    {
        void Resolve(ICardPlayArgs playArgs);
    }
    
    public interface ICardContext : IResolutionContext
    {
        ICardPlayArgs PlayArgs { get; }
        IInstanceFactory InstanceFactory { get; }
    }

    public class CardResolver : ICardResolver
    {
        private readonly IGameState _gameState;
        private readonly IHistoryWriter _historyWriter;
        private readonly IInstanceFactory _instanceFactory;

        public CardResolver(IGameState gameState, IHistoryWriter historyWriter, IInstanceFactory instanceFactory)
        {
            _gameState = gameState;
            _historyWriter = historyWriter;
            _instanceFactory = instanceFactory;
        }
    
        public void Resolve(ICardPlayArgs playArgs)
        {
            var results = new ResolutionCollector();

            playArgs.ValidateTargets(_gameState); // TODO: This should probably be done elsewhere, or by a separate service
            
            playArgs.Player.Resources -= playArgs.Card.Cost; // TODO: Make this more expressive? (e.g. pay costs in different ways)

            var context = new CardContext(_gameState, _instanceFactory, playArgs, results);
            
            foreach (var effect in playArgs.Card.Effects)
            {
                results.AddResult(effect.ResolveContext(context));
            }

            results.AddResult(playArgs.Card.MoveTo(playArgs.Whence.DiscardPile));
            
            _historyWriter.Append(context, results);
        }

        private record CardContext(
                IGameState GameState,
                IInstanceFactory InstanceFactory,
                ICardPlayArgs PlayArgs,
                IResolution PartialResults)
            : ICardContext
        {
        }
    }
}