using Archetype.View.Atoms;

namespace Archetype.View.Context;

public interface IPlayCardContext
{
    ICardFront Card { get; }
    IEnumerable<IGameAtomFront> AllowedTargets { get; }
}