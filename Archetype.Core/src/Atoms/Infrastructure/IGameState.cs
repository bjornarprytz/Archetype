namespace Archetype.Core.Atoms.Infrastructure;

public interface IGameState
{
    public IPlayer Player { get; }
    public ILocation CurrentLocation { get; }
    public IWorld WorldMap { get; }
}