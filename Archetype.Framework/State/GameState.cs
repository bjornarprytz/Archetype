using Archetype.Framework.Events;

namespace Archetype.Framework.State;

internal record GameState
{
    public Dictionary<Guid, Atom> Atoms { get; init; } = new();
    public Dictionary<string, Atom> NamedAtoms { get; init; } = new();
}