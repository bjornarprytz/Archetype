using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Play.Context;

namespace Archetype.Play.Factory;

internal interface IPlayCardContextFactory
{
    IPlayCardContext Create(ICard card);
}

internal class  PlayCardContextFactory : IPlayCardContextFactory
{
    private readonly IFactory<PlayCardContext> _cardContextFactory;

    public PlayCardContextFactory(IFactory<PlayCardContext> cardContextFactory )
    {
        _cardContextFactory = cardContextFactory;
    }
    
    public IPlayCardContext Create(ICard card)
    {
        var cardContext = _cardContextFactory.Create();
        
        cardContext.Init(card);

        return cardContext;
    }
}