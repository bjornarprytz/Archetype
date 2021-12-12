using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Exceptions;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using MediatR;

namespace Archetype.Game.Actions
{
    public class PlayCardAction : IRequest
    {
        public Guid CardGuid { get; }
        public IEnumerable<Guid> TargetsGuids { get; }
        
        public PlayCardAction(Guid cardGuid, IEnumerable<Guid> targetsGuids)
        {
            CardGuid = cardGuid;
            TargetsGuids = targetsGuids;
        }
        
        public PlayCardAction(Guid cardGuid, params Guid[] targetsGuids)
        {
            CardGuid = cardGuid;
            TargetsGuids = targetsGuids;
        }
        
        public PlayCardAction(Guid cardGuid)
        {
            CardGuid = cardGuid;
            TargetsGuids = Enumerable.Empty<Guid>();
        }
    }
    
    public class PlayCardActionHandler : IRequestHandler<PlayCardAction>
    {
        private readonly ICardResolver _cardResolver;
        private readonly IInstanceFinder _instanceFinder;

        public PlayCardActionHandler(ICardResolver cardResolver, IInstanceFinder instanceFinder)
        {
            _cardResolver = cardResolver;
            _instanceFinder = instanceFinder;
        }
        
        public Task<Unit> Handle(PlayCardAction request, CancellationToken cancellationToken)
        {
            var card = _instanceFinder.FindAtom<ICard>(request.CardGuid);
            var targets = request.TargetsGuids.Select(_instanceFinder.FindAtom);

            _cardResolver.Resolve(card, targets);

            return Unit.Task;
        }
    }
}