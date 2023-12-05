using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime.Implementation;

public class GameActionHandler(IMediator mediator, IGameLoop gameLoop) : IGameActionHandler
{
    public IGameAPI CurrentApi { get; private set; } = new InitialApi();

    public IGameAPI Handle(IRequest args)
    {
        var result = mediator.Send(args);
        result.Wait(); // TODO: Find out if this works

        CurrentApi = gameLoop.Advance();

        return CurrentApi;
    }
}