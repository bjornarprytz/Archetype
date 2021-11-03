using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Core.Data.Instance;
using Archetype.Game.Actions;
using Archetype.Game.Payloads;
using HotChocolate;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using MediatR;

namespace Archetype.Server
{
    public class Queries
    {
        private readonly IGameState _gameState;

        public Queries(IGameState gameState)
        {
            _gameState = gameState;
        }

        public GameStateData GetGameState()
        {
            var player  = new PlayerData(); 
        
        
            var gameState = new GameStateData { Player = player };
        
            return gameState;
        } 
    }
    
    public class Mutations
    {
        private readonly IGameState _gameState;
        private readonly IMediator _mediator;

        public Mutations(IMediator mediator, IGameState gameState)
        {
            _gameState = gameState;
            _mediator = mediator;
        }

        public async Task<PlayCardPayload> PlayCard(
            
            PlayCardInput playCardInput,
            [Service] ITopicEventSender eventSender,
            CancellationToken cancellationToken
            )
        {
            var (cardId, targetIds) = playCardInput;
            
            var result = await _mediator.Send(new PlayCardAction(cardId, targetIds), cancellationToken);

            var payload = new PlayCardPayload(result);

            await eventSender.SendAsync(nameof(Subscriptions.OnCardPlayed), payload, cancellationToken);
            
            return payload;
        }

        public record PlayCardPayload(string Message);
        public record PlayCardInput(long CardId, IEnumerable<long> TargetIds);
    }
    
    public class Subscriptions
    {
        private readonly IGameState _gameState;

        public Subscriptions(IGameState gameState)
        {
            _gameState = gameState;
        }
    
        [Subscribe]
        [Topic]
        public Mutations.PlayCardPayload OnCardPlayed([EventMessage] Mutations.PlayCardPayload playCardPayload) => playCardPayload;
    }
}