using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Infrastructure;

public interface IGameState
{
    public IPlayer Player { get; }
    public IMap Map { get; }
    public ILocation? CurrentLocation { get; }
    public IResolution ResolutionZone { get; }
    public IPrompter Prompter { get; }
}