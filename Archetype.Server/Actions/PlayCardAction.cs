using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using MediatR;
using Unit = MediatR.Unit;

namespace Archetype.Server.Actions;

public record PlayCardAction(Guid CardGuid, Guid WhenceNodeGuid, IEnumerable<Guid> TargetGuids) : IRequest;

public class PlayCardActionHandler : IRequestHandler<PlayCardAction>
{
    private readonly IContextFactory<ICardPlayArgs> _cardContextFactory;
    private readonly IContextResolver _contextResolver;

    public PlayCardActionHandler(
        IContextFactory<ICardPlayArgs> cardContextFactory,
        IContextResolver contextResolver)
    {
        _cardContextFactory = cardContextFactory;
        _contextResolver = contextResolver;
    }
    
    
    public Task<Unit> Handle(PlayCardAction request, CancellationToken cancellationToken)
    {
        using var context = _cardContextFactory.BindContext(new PlayCardArgs(request));

        _contextResolver.Resolve(context);
        
        return Unit.Task;
    }
    
    private class PlayCardArgs : ICardPlayArgs
    {
        public PlayCardArgs(PlayCardAction playCardAction)
        {
            CardGuid = playCardAction.CardGuid;
            WhenceGuid = playCardAction.WhenceNodeGuid;
            TargetGuids = playCardAction.TargetGuids;
        }
        public Guid CardGuid { get; }
        public Guid WhenceGuid { get; }
        public IEnumerable<Guid> TargetGuids { get; }
    }
}