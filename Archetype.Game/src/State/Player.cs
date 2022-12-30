using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.DeckBuilding;
using Archetype.Core.Effects;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Game.State;

internal class Player : Atom, IPlayer
{

    public int Life { get; set; }
    
    public ICardCollection CardCollection { get; } = new CardCollection();
    public IDeck CurrentDeck { get; set; } = new Deck();
    public IDrawPile DrawPile { get; } = new DrawPile();
    public IHand Hand { get; } = new Hand();
    public IDiscardPile DiscardPile { get; } = new DiscardPile();    
}

internal class Hand : Zone<ICard>, IHand
{
    
}

internal class DrawPile : Zone<ICard>, IDrawPile
{
    public int Count => Atoms.Count;
}

internal class DiscardPile : Zone<ICard>, IDiscardPile
{
}

internal class Deck : IDeck
{
    private readonly List<IProtoPlayingCard> _cards;
    
    public Deck()
    {
        _cards = new List<IProtoPlayingCard>();
    }

    public IEnumerable<IProtoPlayingCard> Cards => _cards;
}

internal class CardCollection : ICardCollection
{
    private readonly List<IProtoPlayingCard> _cards;

    public CardCollection()
    {
        _cards = new List<IProtoPlayingCard>();
    }

    public IEnumerable<IProtoPlayingCard> Cards => _cards;
}