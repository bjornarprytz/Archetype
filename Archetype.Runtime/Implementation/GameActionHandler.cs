using MediatR;

namespace Archetype.Runtime.Implementation;

public class GameActionHandler : IGameActionHandler
{
    private readonly IMediator _mediator;
    private readonly IEffectQueue _effectQueue;

    public GameActionHandler(IMediator mediator, IEffectQueue effectQueue)
    {
        _mediator = mediator;
        _effectQueue = effectQueue;
    }
    
    public ActionResult Handle(IRequest args)
    {
        var result = _mediator.Send(args);

        return new ActionResult
        {
            Events = _effectQueue.ResolveAll().ToList()
            // TODO: Add AvailableActions
        };
    }
}