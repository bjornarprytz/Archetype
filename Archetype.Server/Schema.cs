using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Actions;
using Archetype.Game.Exceptions;
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
        private readonly ICardPool _cardPool;

        public Queries(IGameState gameState, ICardPool cardPool)
        {
            _gameState = gameState;
            _cardPool = cardPool;
        }

        public IGameState GetGameState() => _gameState;
        public ICardPool GetCardPool() => _cardPool;
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
            [Service] ITopicEventSender eventSender,
            CancellationToken cancellationToken
            )
        {
            var (cardId, targetIds) = playCardInput;
    
            // TODO: Put errors (exceptions) into the schema (example: https://youtu.be/3_4nt2QQSeE?t=4064)
            
            await _mediator.Send(new PlayCardAction(cardId, targetIds), cancellationToken);

            var payload = new PlayCardPayload();

            await eventSender.SendAsync(nameof(Subscriptions.OnCardPlayed), payload, cancellationToken);
            
            return payload;
        }

        public record PlayCardPayload;
        public record PlayCardInput(Guid CardId, IEnumerable<Guid> TargetIds);
        
        
        public async Task<StartGamePayload> StartGame(
            StartGameInput startGameInput,
            [Service] ITopicEventSender eventSender,
            CancellationToken cancellationToken
        )
        {
            var result = await _mediator.Send(new StartGameAction(startGameInput.ProtoCardIds), cancellationToken);

            var payload = new StartGamePayload(result);

            await eventSender.SendAsync(nameof(Subscriptions.OnGameStarted), payload, cancellationToken);
            
            return payload;
        }

        public record StartGameInput(IEnumerable<Guid> ProtoCardIds);
        public record StartGamePayload(string Message);
        
        public async Task<TurnStartedPayload> EndTurn(
            [Service] ITopicEventSender eventSender,
            CancellationToken cancellationToken
        )
        {
            var result = await _mediator.Send(new EndTurnAction(), cancellationToken); // Should resolve enemy turn

            var payload = new TurnStartedPayload(result);

            await eventSender.SendAsync(nameof(Subscriptions.OnTurnStarted), payload, cancellationToken);
            
            return payload;
        }

        public record TurnStartedPayload(string Message);
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