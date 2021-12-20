using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;
using MediatR;
using Unit = MediatR.Unit;

namespace Archetype.Server.Actions
{
    public class PlayCardAction : IRequest
    {
        public Guid CardGuid { get; }
        public Guid MapNodeGuid { get; }
        public IEnumerable<Guid> TargetsGuids { get; }
        
        public PlayCardAction(Guid cardGuid, Guid mapNodeGuid, IEnumerable<Guid> targetsGuids)
        {
            CardGuid = cardGuid;
            MapNodeGuid = mapNodeGuid;
            TargetsGuids = targetsGuids;
        }
    }
    
    public class PlayCardActionHandler : IRequestHandler<PlayCardAction>
    {
        private readonly IPlayer _player;
        private readonly ICardResolver _cardResolver;
        private readonly IInstanceFinder _instanceFinder;

        public PlayCardActionHandler(IPlayer player, ICardResolver cardResolver, IInstanceFinder instanceFinder)
        {
            _player = player;
            _cardResolver = cardResolver;
            _instanceFinder = instanceFinder;
        }
        
        public Task<Unit> Handle(PlayCardAction request, CancellationToken cancellationToken)
        {
            var node = _instanceFinder.FindAtom<IMapNode>(request.MapNodeGuid);
            var card = _instanceFinder.FindAtom<ICard>(request.CardGuid);
            var targets = request.TargetsGuids.Select(_instanceFinder.FindAtom);

            _cardResolver.Resolve(new PlayCardArgs(_player, card, node, targets));

            return Unit.Task;
        }
        
        private class PlayCardArgs : ICardPlayArgs
        {
            public PlayCardArgs(IPlayer player, ICard card, IMapNode whence, IEnumerable<IGameAtom> targets)
            {
                var chosenTargets = targets.ToList();
            
                var requiredTargetCount = card.Targets.Count();
            
                if (chosenTargets.Count != requiredTargetCount)
                {
                    throw new ArgumentException($"Mismatching number of targets in card and arguments. Expected: {requiredTargetCount}, Actual: {chosenTargets.Count}");
                }

                if (player.Resources < card.Cost)
                    throw new InvalidOperationException(
                        $"Player does not have enough resources. Cost: {card.Cost}, Actual: {player.Resources}");
                
                Player = player;
                Card = card;
                Whence = whence;
                Targets = chosenTargets;
            }

            public IPlayer Player { get; }
            public ICard Card { get; }
            public IMapNode Whence { get; }
            public IEnumerable<IGameAtom> Targets { get; }
        }
    }
}