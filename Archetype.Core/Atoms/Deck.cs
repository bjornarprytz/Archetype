using Archetype.Core.Atoms.Base;
using Archetype.Core.Extensions;
using Archetype.Core.Play;
using Archetype.View.Atoms.Zones;
using Archetype.View.Infrastructure;

namespace Archetype.Core.Atoms;

public interface IDeck : IZone<ICard>, IDeckFront
{
    ICard PopCard();
    IEffectResult<IDeck> Shuffle();
    IEffectResult<IDeck, ICard> PutCardOnTop(ICard card);
    IEffectResult<IDeck, ICard> PutCardOnBottom(ICard card);
}

internal class Deck : Zone<ICard>, IDeck
{
    private readonly Stack<ICard> _cards = new();

    public Deck(IGameAtom owner) : base(owner) {}

    public ICard PopCard()
    {
        var card = _cards.Pop();

        return card;
    }

    public IEffectResult<IDeck> Shuffle()
    {
        var shuffledCards = _cards.Shuffle();
            
        _cards.Clear();

        foreach (var card in shuffledCards)
        {
            _cards.Push(card);
        }
            
        return ResultFactory.Create(this);
    }

    public IEffectResult<IDeck, ICard> PutCardOnTop(ICard newCard)
    {
        _cards.Push(newCard);
            
        var sideEffects = new List<IEffectResult>
        {
            newCard.MoveTo(this)
        };
            
        return ResultFactory.Create(this, newCard, sideEffects);
    }

    public IEffectResult<IDeck, ICard> PutCardOnBottom(ICard newCard)
    {
        var newOrder = _cards.Prepend(newCard).ToList();
            
        _cards.Clear();
            
        foreach (var card in newOrder)
        {
            _cards.Push(card);
        }

        var sideEffects = new List<IEffectResult>
        {
            newCard.MoveTo(this)
        };
            
        return ResultFactory.Create(this, newCard, sideEffects);
    }

    public int NumberOfCards => Contents.Count();
}