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
    IReadOnlyList<IEvent> KeywordEvents { get; }
    IReadOnlyList<IActionBlockEvent> ActionBlockEvents { get; }
}

public interface IEventBus
{
    void Publish<T>(T @event) where T : IActionBlockEvent;
    void Subscribe<T>(IAtom subscriber, Action<T> handler) where T : IEvent;
    void Unsubscribe<T>(IAtom subscriber, Action<T> handler) where T : IEvent;
    
}

public interface IActionQueue
{
    IResolutionFrame? CurrentFrame { get; }
    void Push(IResolutionFrame frame);
    IEvent? ResolveNextKeyword();
}

public interface IGameLoop
{
    IPhase? CurrentPhase { get; }
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