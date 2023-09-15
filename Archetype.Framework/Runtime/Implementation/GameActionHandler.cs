using MediatR;

namespace Archetype.Framework.Runtime.Implementation;

public class GameActionHandler : IGameActionHandler
{
    private readonly IMediator _mediator;
    private readonly IGameLoop _gameLoop;

    public GameActionHandler(IMediator mediator, IGameLoop gameLoop)
    {
        _mediator = mediator;
        _gameLoop = gameLoop;
    }

    public IGameAPI CurrentApi { get; private set; } // TODO: Initialize this

    public IGameAPI Handle(IRequest args)
    {
        var result = _mediator.Send(args);
        result.Wait();

        CurrentApi = _gameLoop.Advance();

        return CurrentApi;
    }
}