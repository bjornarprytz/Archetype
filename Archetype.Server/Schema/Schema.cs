using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Server.Actions;
using Archetype.View;
using Archetype.View.Context;
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
        private readonly IMediator _mediator;

        public Mutations(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        public async Task<StartGamePayload> StartGame(
            ITopicEventSender eventSender,
            CancellationToken cancellationToken
        )
        {
            var context = await _mediator.Send(new StartGameAction(), cancellationToken);

            var payload = new StartGamePayload(context);

            await eventSender.SendAsync(nameof(Subscriptions.OnGameStarted), payload, cancellationToken);
            
            return payload;
        }
        public record StartGamePayload(ITurnContext TurnContext);
    }
    
    public class Subscriptions
    {
        [Subscribe]
        [Topic]
        public Mutations.StartGamePayload OnGameStarted([EventMessage] Mutations.StartGamePayload startGamePayload) => startGamePayload;
    }
}