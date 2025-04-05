using Archetype.Framework.Events;

namespace Archetype.Framework.State;

public record GameState
{
    public Dictionary<Guid, Atom> Atoms { get; init; } = new();
}