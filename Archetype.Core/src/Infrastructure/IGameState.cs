using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Infrastructure;

public interface IGameState
{
    public IPlayer Player { get; }
    public IMap Map { get; }
    public ILocation? CurrentLocation { get; }
    public IResolution ResolutionZone { get; }
    
    // TODO: Add effect queue for triggers and stuff that requires user input. Most game actions should require an empty queue.
}