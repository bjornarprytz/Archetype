using Archetype.Core.DeckBuilding;

namespace Archetype.Game;

public interface IDeckBuilding
{
    public Task<Guid> SaveDeck(IDeck deck);
    public Task ChooseDeck(Guid deckId);
}