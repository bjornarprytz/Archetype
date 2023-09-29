using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime;

public interface IProtoCards
{
    IProtoCard? GetProtoCard(string name);
}

public interface IDefinitions
{
    IKeywordDefinition? GetDefinition(string keyword);
}

public interface IDefinitionBuilder
{
    void AddKeyword(IKeywordDefinition keywordDefinition);
}

public interface IEventHistory
{
    public void Push(IEvent e);
    public IReadOnlyList<IEvent> Events { get; set; }
}

public interface IActionQueue
{
    IResolutionFrame? CurrentFrame { get; }
    void Push(IResolutionFrame frame);
    IEvent? ResolveNext();
}

public interface IGameLoop
{
    IGameAPI Advance();
}

public interface IGameActionHandler
{
    IGameAPI CurrentApi { get; }
    IGameAPI Handle(IRequest args);
}

public record ActionDescription(ActionType Type);

public record InitialApi : IGameAPI
{
    public IGameState State { get; } = null;

    public IReadOnlyList<ActionDescription> AvailableActions { get; set; } =
        new[] { new ActionDescription(ActionType.StartGame) };
}

public interface IGameAPI
{
    IGameState State { get; }
    IReadOnlyList<ActionDescription> AvailableActions { get; }
}