using Archetype.Core.Atoms;

namespace Archetype.Core.Infrastructure;

public interface IGameState
{
    public int Seed { get; }
    public IPlayer Player { get; }
    public ICard? CurrentLocation { get; }
    public IWorld WorldMap { get; }
}