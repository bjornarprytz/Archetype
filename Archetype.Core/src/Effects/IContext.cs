using Archetype.Core.Atoms.Cards;
using Archetype.Core.Infrastructure;

namespace Archetype.Core.Effects;

public interface IContext
{
    IGameState GameState { get; }
    ITargetProvider TargetProvider { get; }
    
    ICard Source { get; }
}
