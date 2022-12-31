using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.DeckBuilding;
using Archetype.Core.Extensions;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Game.State;

internal class Player : Atom, IPlayer
{

    public Player(Random random)
    {
        DrawPile = new DrawPile(random);
    }

    public int Life { get; set; }
    
    public ICardCollection CardCollection { get; } = new CardCollection();
    public IDeck CurrentDeck { get; set; } = new Deck();
    public IDrawPile DrawPile { get; }
    public IHand Hand { get; } = new Hand();
    public IDiscardPile DiscardPile { get; } = new DiscardPile();    
}

internal class Hand : Zone<ICard>, IHand
{
    
}

internal class DrawPile : Zone<ICard>, IDrawPile
{
    private Stack<int> _order = new ();
    private readonly Random _random;

    public DrawPile(Random random)
    {
        _random = random;
    }
    
    public int Count => Atoms.Count;
    public ICard? PeekTopCard() // TODO: Write tests for this
    {
        if (_order.Count == 0 || Atoms.Count == 0)
            return null;

        var index = _order.Peek();
        
        while (_order.Count > 0 && index >= Atoms.Count)
        {
            index = _order.Pop();
        }

        return Atoms[_order.Peek()];
    }

    public void Shuffle()
    {
        _order = new Stack<int>(Enumerable.Range(0, Atoms.Count).Shuffle(_random));
    }
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