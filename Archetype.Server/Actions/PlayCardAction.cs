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

public class PlayCardAction : IRequest
{
    public PlayCardAction(Guid cardGuid, Guid whenceNodeGuid, IEnumerable<Guid> targetGuids)
    {
        CardGuid = cardGuid;
        WhenceNodeGuid = whenceNodeGuid;
        TargetGuids = targetGuids;
    }

    public Guid CardGuid { get; set; }
    public Guid WhenceNodeGuid { get; }
    public IEnumerable<Guid> TargetGuids { get; }
}

public class PlayCardActionHandler : IRequestHandler<PlayCardAction>
{
    private readonly IContextBinder _contextBinder;
    private readonly IContextResolver _contextResolver;
    private readonly IHistoryWriter _historyWriter;

    public PlayCardActionHandler(
        IContextBinder contextBinder,
        IContextResolver contextResolver,
        IHistoryWriter historyWriter)
    {
        _contextBinder = contextBinder;
        _contextResolver = contextResolver;
        _historyWriter = historyWriter;
    }
    
    
    public Task<Unit> Handle(PlayCardAction request, CancellationToken cancellationToken)
    {
        using var context = _contextBinder.BindContext(new PlayCardArgs(request));

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