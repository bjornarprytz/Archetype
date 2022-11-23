using Archetype.Core.Atoms;
using Archetype.Core.Infrastructure;

namespace Archetype.Core.Effects;

public interface IEffectContext
{
    IGameState GameState { get; }
    IAtom Source { get; }
    ITargetProvider TargetProvider { get; }
}
