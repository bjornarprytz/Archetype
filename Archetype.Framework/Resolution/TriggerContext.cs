using Archetype.Framework.Events;
using Archetype.Framework.State;

namespace Archetype.Framework.Resolution;

public record TriggerContext
{
    public required Guid Host { get; init; }
    public required Event Event { get; init; }
    public required GameState State { get; init; }
}