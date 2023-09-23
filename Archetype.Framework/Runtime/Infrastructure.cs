using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;
using MediatR;

namespace Archetype.Framework.Runtime;

public interface IProtoCards
{
    ProtoCard? GetProtoCard(string name);
}

public interface IDefinitions
{
    KeywordDefinition? GetKeyword(string keyword);
}

public interface IDefinitionBuilder
{
    void AddKeyword(KeywordDefinition keywordDefinition);
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


public interface IGameAPI
{
    IGameState State { get; set; }
    IReadOnlyList<ActionDescription> AvailableActions { get; set; }
}