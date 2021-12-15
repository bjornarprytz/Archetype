using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Actions;
using Archetype.Game.Payloads.Infrastructure;
using HotChocolate;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using MediatR;

namespace Archetype.Server
{
    public class Queries
    {
        private readonly IGameState _gameState;
        private readonly IProtoPool _protoPool;

        public Queries(IGameState gameState, IProtoPool protoPool)
        {
            _gameState = gameState;
            _protoPool = protoPool;
        }

        public IGameState GetGameState() => _gameState;
        public IProtoPool GetCardPool() => _protoPool;
    }
    
    public class Mutations
    {
        private readonly IMediator _mediator;

        public Mutations(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<PlayCardPayload> PlayCard(
            PlayCardInput playCardInput,
            ITopicEventSender eventSender,
            CancellationToken cancellationToken
            )
        {
            var (cardId, nodeId, targetIds) = playCardInput;
    
            // TODO: Put errors (exceptions) into the schema (example: https://youtu.be/3_4nt2QQSeE?t=4064)
            
            await _mediator.Send(new PlayCardAction(cardId, nodeId, targetIds), cancellationToken);

            var payload = new PlayCardPayload();

            await eventSender.SendAsync(nameof(Subscriptions.OnCardPlayed), payload, cancellationToken);
            
            return payload;
        }

        public record PlayCardPayload;
        public record PlayCardInput(Guid CardId, Guid MapNodeGuid, IEnumerable<Guid> TargetIds);
        
        
        public async Task<StartGamePayload> StartGame(
            StartGameInput startGameInput,
            ITopicEventSender eventSender,
            CancellationToken cancellationToken
        )
        {
            await _mediator.Send(new StartGameAction(startGameInput.CardNames, startGameInput.HqStructureName, startGameInput.HqLocationId), cancellationToken);

            var payload = new StartGamePayload();

            await eventSender.SendAsync(nameof(Subscriptions.OnGameStarted), payload, cancellationToken);
            
            return payload;
        }

        public record StartGameInput(IEnumerable<string> CardNames, string HqStructureName, Guid HqLocationId);
        public record StartGamePayload();
        
        public async Task<TurnStartedPayload> EndTurn(
            ITopicEventSender eventSender,
            CancellationToken cancellationToken
        )
        {
            await _mediator.Send(new EndTurnAction(), cancellationToken);

            var payload = new TurnStartedPayload();

            await eventSender.SendAsync(nameof(Subscriptions.OnTurnStarted), payload, cancellationToken);
            
            return payload;
        }

        public record TurnStartedPayload();
    }
    
    public class Subscriptions
    {
        [Subscribe]
        [Topic]
        public Mutations.PlayCardPayload OnCardPlayed([EventMessage] Mutations.PlayCardPayload playCardPayload) => playCardPayload;
        
        [Subscribe]
        [Topic]
        public Mutations.StartGamePayload OnGameStarted([EventMessage] Mutations.StartGamePayload startGamePayload) => startGamePayload;
        
        [Subscribe]
        [Topic]
        public Mutations.TurnStartedPayload OnTurnStarted([EventMessage] Mutations.TurnStartedPayload turnStartedPayload) => turnStartedPayload;
    }
}