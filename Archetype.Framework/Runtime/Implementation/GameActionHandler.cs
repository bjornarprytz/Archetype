using MediatR;

namespace Archetype.Runtime.Implementation;

public class GameActionHandler : IGameActionHandler
{
    private readonly IMediator _mediator;
    private readonly IGameLoop _gameLoop;

    public GameActionHandler(IMediator mediator, IGameLoop gameLoop)
    {
        _mediator = mediator;
        _gameLoop = gameLoop;
    }
    
    public ActionResult Handle(IRequest args)
    {
        var result = _mediator.Send(args); // TODO: Deal with async

        return _gameLoop.Advance();
    }
}