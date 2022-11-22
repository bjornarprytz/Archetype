using Archetype.Core.Atoms.Zones;
using Archetype.Core.Proto.DeckBuilding;

namespace Archetype.Core.Atoms.Infrastructure;

public interface IPlayer : IAtom
{
    public int Life { get; set; }
    
    public ICardCollection CardCollection { get; }

    public IDeck CurrentDeck { get; set; }
    
    public IDrawPile DrawPile { get; }
    public IHand Hand { get; }
    public IDiscardPile DiscardPile { get; }
}