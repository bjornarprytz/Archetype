namespace Archetype.Framework.Resolution;

public interface IActionArgs
{
    /* Marker interface for possible action arguments */
}

public record StartGameArgs() : IActionArgs;
public record PlayCardArgs(Guid CardId, Guid[] Targets) : IActionArgs;
public record EndTurnArgs() : IActionArgs;