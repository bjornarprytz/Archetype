using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Infrastructure;

namespace Archetype.Core;

public interface IGameState
{
    public int Seed { get; }
    public IPlayer Player { get; }
    public ICard? CurrentLocation { get; }
    public IWorld WorldMap { get; }
}