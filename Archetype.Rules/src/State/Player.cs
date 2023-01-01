using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.DeckBuilding;
using Archetype.Core.Extensions;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Rules.State;

public class Player : Atom, IPlayer
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

public class Hand : Zone<ICard>, IHand
{
    
}

public class DrawPile : Zone<ICard>, IDrawPile
{
    private Stack<int> _order = new ();
    private readonly Random _random;

    public DrawPile(Random random)
    {
        _random = random;
    }
    
    public int Count => Atoms.Count;
    public ICard? PeekTopCard()
    {
        return Atoms.Count == 0 ? null : Atoms[_order.Peek()];
    }

    public void Shuffle()
    {
        _order = new Stack<int>(Enumerable.Range(0, Atoms.Count).Shuffle(_random));
    }

    protected override void OnAtomAdded(ICard atom)
    {
        _order.Push(_order.Count);
    }

    protected override void OnAtomRemoved(ICard atom)
    {
        _order = new Stack<int>(_order.Where(i => i < Atoms.Count));
    }
}

public class DiscardPile : Zone<ICard>, IDiscardPile
{
}

public class Deck : IDeck
{
    private readonly List<IProtoPlayingCard> _cards;
    
    public Deck()
    {
        _cards = new List<IProtoPlayingCard>();
    }

    public IEnumerable<IProtoPlayingCard> Cards => _cards;
}

public class CardCollection : ICardCollection
{
    private readonly List<IProtoPlayingCard> _cards;

    public CardCollection()
    {
        _cards = new List<IProtoPlayingCard>();
    }

    public IEnumerable<IProtoPlayingCard> Cards => _cards;
}