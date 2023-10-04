using Archetype.BasicRules.Primitives;
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

public interface IRules
{
    IReadOnlyList<IPhase> Phases { get; } // TODO: Is this the right place for this?
    IKeywordDefinition? GetDefinition(string keyword);
    T? GetDefinition<T>() where T : IKeywordDefinition;
}

public interface IRulesBuilder
{
    void AddKeyword(IKeywordDefinition keywordDefinition);
    void SetTurnSequence(IReadOnlyList<IPhase> phase);
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
    IEvent? ResolveNextKeyword();
}

public interface IGameLoop
{
    IPhase CurrentPhase { get; }
    IGameAPI Advance();
    IGameAPI EndPhase();
}

public interface IGameActionHandler
{
    IGameAPI CurrentApi { get; }
    IGameAPI Handle(IRequest args);
}

public record ActionDescription(ActionType Type);

public record InitialApi : IGameAPI
{
    public IReadOnlyList<ActionDescription> AvailableActions { get; } =
        new[] { new ActionDescription(ActionType.StartGame) };
}

// TODO: Does this need something from the event?
public record PromptApi : IGameAPI
{
    public IReadOnlyList<ActionDescription> AvailableActions { get; } = new []{ new ActionDescription(ActionType.AnswerPrompt) };
}

public record GameAPI(IReadOnlyList<ActionDescription> AvailableActions) : IGameAPI;

public interface IGameAPI
{
    IReadOnlyList<ActionDescription> AvailableActions { get; }
}