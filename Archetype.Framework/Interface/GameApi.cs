using Archetype.Framework.Interface.Actions;

namespace Archetype.Framework.Interface;

public interface IGameApi
{
    IReadOnlyList<ActionDescription> AvailableActions { get; }
}

public record GameApi(IReadOnlyList<ActionDescription> AvailableActions) : IGameApi;

public record ActionDescription(ActionType Type);

public record InitialApi : IGameApi
{
    public IReadOnlyList<ActionDescription> AvailableActions { get; } =
        new[] { new ActionDescription(ActionType.StartGame) };
}

public record PromptApi : IGameApi
{
    public IReadOnlyList<ActionDescription> AvailableActions { get; } = new []{ new ActionDescription(ActionType.AnswerPrompt) };
}


