using Archetype.View.Atoms;

namespace Archetype.View.Context;

public interface ITurnContext
{
    IEnumerable<ICardFront> PlayableCards { get; }
    IPlayCardContext PlayCard(Guid cardGuid);
    ITurnContext EndTurn();
}

