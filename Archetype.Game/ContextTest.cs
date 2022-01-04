using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Factory;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.View.Atoms;
using Archetype.View.Context;

namespace Archetype.Game;

internal class TurnContext : ITurnContext
{
    private readonly IFactory<IUnarmedPlayCardContext> _playCardContext;
    private readonly List<ICardFront> _playableCards;

    public TurnContext(IGameState gameState, IFactory<IUnarmedPlayCardContext> playCardContext)
    {
        _playCardContext = playCardContext;
        _playableCards = gameState.Player.Hand.Cards.ToList();
    }

    public IEnumerable<ICardFront> PlayableCards => _playableCards;
    public IPlayCardContext PlayCard(Guid cardGuid)
    {
        var card = _playableCards.First(c => c.Guid == cardGuid);



        var context = _playCardContext.Create();

        context.Inject(card);

        return context;
    }

    public ITurnContext EndTurn()
    {
        throw new System.NotImplementedException();
    }
}


internal interface IUnarmedPlayCardContext : IPlayCardContext // TODO: Remove this class after test
{
    void Inject(ICardFront card);
}

internal class PlayCardContext : IUnarmedPlayCardContext
{
    private readonly IFactory<ITurnContext> _turnFactory;
    private readonly ICardResolver _cardResolver;
    private readonly IInstanceFinder _instanceFinder;
    private readonly IPlayer _player;

    public PlayCardContext(
        IFactory<ITurnContext> turnFactory,
        ICardResolver cardResolver, 
        IInstanceFinder instanceFinder,
        IPlayer player
        )
    {
        _turnFactory = turnFactory;
        _cardResolver = cardResolver;
        _instanceFinder = instanceFinder;
        _player = player;
    }

    public ICardFront Card { get; private set; }
    public IEnumerable<IGameAtomFront> AllowedTargets => Enumerable.Empty<IGameAtomFront>(); // TODO: This should be searched up
    public ITurnContext Commit(Guid whenceGuid, IEnumerable<Guid> targetsGuids)
    {
        var node = _instanceFinder.FindAtom<IMapNode>(whenceGuid);
        var card = _instanceFinder.FindAtom<ICard>(Card.Guid);
        var targets = targetsGuids.Select(_instanceFinder.FindAtom);

        _cardResolver.Resolve(new PlayCardArgs(_player, card, node, targets));

        return _turnFactory.Create();
    }

    public ITurnContext Cancel()
    {
        return _turnFactory.Create(); // TODO: Maybe this should return a reference to the parent context instead? since it should be unchanged.
    }

    public void Inject(ICardFront card)
    {
        Card = card;
    }
    
    private class PlayCardArgs : ICardPlayArgs
    {
        public PlayCardArgs(IPlayer player, ICard card, IMapNode whence, IEnumerable<IGameAtom> targets)
        {
            var chosenTargets = targets.ToList();
            
            var requiredTargetCount = card.Targets.Count();
            
            if (chosenTargets.Count != requiredTargetCount)
            {
                throw new ArgumentException($"Mismatching number of targets in card and arguments. Expected: {requiredTargetCount}, Actual: {chosenTargets.Count}");
            }

            if (player.Resources < card.Cost)
                throw new InvalidOperationException(
                    $"Player does not have enough resources. Cost: {card.Cost}, Actual: {player.Resources}");
                
            Player = player;
            Card = card;
            Whence = whence;
            Targets = chosenTargets;
        }

        public IPlayer Player { get; }
        public ICard Card { get; }
        public IMapNode Whence { get; }
        public IEnumerable<IGameAtom> Targets { get; }
    }
}
