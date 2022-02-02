using Archetype.View.Atoms;

namespace Archetype.View.Infrastructure;

public interface IGameStateFront
{
    IPlayerFront Player { get; }
    IMapFront Map { get; }
}