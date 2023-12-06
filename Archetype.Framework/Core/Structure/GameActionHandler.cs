using Archetype.Framework.Interface;
using MediatR;

namespace Archetype.Framework.Core.Structure;

public interface IGameActionHandler
{
    IGameApi CurrentApi { get; }
    IGameApi Handle(IRequest args);
}

public class GameActionHandler(IMediator mediator, IGameLoop gameLoop) : IGameActionHandler
{
    public IGameApi CurrentApi { get; private set; } = new InitialApi();

    public IGameApi Handle(IRequest args)
    {
        var result = mediator.Send(args);
        result.Wait(); // TODO: Find out if this works

        CurrentApi = gameLoop.Advance();

        return CurrentApi;
    }
}