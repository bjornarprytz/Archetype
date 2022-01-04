using Archetype.View.Atoms;

namespace Archetype.View.Context;

public interface IPlayCardContext
{
    ICardFront Card { get; }
    IEnumerable<IGameAtomFront> AllowedTargets { get; }
    ITurnContext Commit(Guid whenceGuid, IEnumerable<Guid> targetsGuids);
    ITurnContext Cancel();
}

