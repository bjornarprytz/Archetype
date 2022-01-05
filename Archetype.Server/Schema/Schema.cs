using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Server.Actions;
using Archetype.View.Infrastructure;
using HotChocolate;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using MediatR;

namespace Archetype.Server
{
    public class Queries
    {
        private readonly IGameStateFront _gameState;
        private readonly IProtoPoolFront _protoPool;

        public Queries(IGameStateFront gameState, IProtoPoolFront protoPool)
        {
            _gameState = gameState;
            _protoPool = protoPool;
        }

        public IGameStateFront GetGameState() => _gameState;
        public IProtoPoolFront GetCardPool() => _protoPool;
    }
    
    public class Mutations
    {
        public Mutations()
        {
            
        }
        
        public async Task<StartGamePayload> StartGame(
            ITopicEventSender eventSender,
            [Service] IMediator mediator,
            CancellationToken cancellationToken
        )
        {
            await mediator.Send(new StartGameAction(), cancellationToken);

            var payload = new StartGamePayload("Game started!");

            await eventSender.SendAsync(nameof(Subscriptions.OnGameStarted), payload, cancellationToken);
            
            return payload;
        }
        public record StartGamePayload(string Message); // TODO: Placeholder value
        
        public async Task<PlayCardPayload> PlayCard(
            PlayCardInput playCardInput,
            ITopicEventSender eventSender,
            [Service] IMediator mediator,
            CancellationToken cancellationToken
        )
        {
            var (cardGuid, whenceNodeGuid, targetGuids) = playCardInput;
            
            await mediator.Send(new PlayCardAction(cardGuid, whenceNodeGuid, targetGuids), cancellationToken);

            var payload = new PlayCardPayload($"Card played ({cardGuid})");

            await eventSender.SendAsync(nameof(Subscriptions.OnCardPlayed), payload, cancellationToken);
            
            return payload;
        }

        public record PlayCardInput(Guid CardGuid, Guid WhenceNodeGuid, IEnumerable<Guid> TargetGuids);
        public record PlayCardPayload(string Message);// TODO: replace placeholder value 
    }
    
    public class Subscriptions
    {
        [Subscribe]
        [Topic]
        public Mutations.StartGamePayload OnGameStarted([EventMessage] Mutations.StartGamePayload startGamePayload) => startGamePayload;
        
        [Subscribe]
        [Topic]
        public Mutations.PlayCardPayload OnCardPlayed([EventMessage] Mutations.PlayCardPayload playCardPayload) => playCardPayload;
    }
}