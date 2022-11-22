using Archetype.Core.Atoms;

namespace Archetype.Core.Proto.Location;

public interface IProtoEncounter : IProtoLocation
{
    public IDrawPile EncounterDeck { get; }
    public IEnumerable<string> Connections { get; } // e.g. "A-B", "B-C", "C-A" TODO: Make this more useful than string
}