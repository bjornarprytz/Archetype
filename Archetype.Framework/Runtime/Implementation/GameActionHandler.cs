using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime.Implementation;

public class GameActionHandler : IGameActionHandler
{
    private readonly IMediator _mediator;
    private readonly IGameLoop _gameLoop;

    public GameActionHandler(IMediator mediator, IGameLoop gameLoop, IGameState gameState)
    {
        _mediator = mediator;
        _gameLoop = gameLoop;

        CurrentApi = new InitialApi();
    }

    public IGameAPI CurrentApi { get; private set; }

    public IGameAPI Handle(IRequest args)
    {
        var result = _mediator.Send(args);
        result.Wait(); // TODO: Find out if this works

        CurrentApi = _gameLoop.Advance();

        return CurrentApi;
    }
}