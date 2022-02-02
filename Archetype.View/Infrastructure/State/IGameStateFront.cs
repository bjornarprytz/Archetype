using Archetype.View.Atoms;

namespace Archetype.View.Infrastructure.State;

public interface IGameStateFront
{
    IPlayerFront Player { get; }
    IMapFront Map { get; }
}