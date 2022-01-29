using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context;

public interface IContextFactory<in TArgs> // TODO: Generalize this further
{
    IContext BindContext(TArgs args);
}

internal class CardContextFactory : IContextFactory<ICardPlayArgs>
{
    private readonly IGameState _gameState;
    private readonly IInstanceFinder _instanceFinder;
    private readonly IHistoryReader _historyReader;
    private readonly IHistoryWriter _historyWriter;

    public CardContextFactory(
        IGameState gameState,
        IInstanceFinder instanceFinder,
        IHistoryReader historyReader,
        IHistoryWriter historyWriter)
    {
        _gameState = gameState;
        _instanceFinder = instanceFinder;
        _historyReader = historyReader;
        _historyWriter = historyWriter;
    }
    
    public IContext BindContext(ICardPlayArgs args)
    {
        var node = _instanceFinder.FindAtom<IMapNode>(args.WhenceGuid);
        var card = _instanceFinder.FindAtom<ICard>(args.CardGuid);
        var chosenTargets = args.TargetGuids.Select(_instanceFinder.FindAtom);
        
        return new CardContext(card, node, chosenTargets, _gameState, _historyReader, _historyWriter);
    }
    
    private class CardContext : IContext
    {
        private readonly ICard _card;
        private readonly IHistoryWriter _historyWriter;
        
        public CardContext(
            ICard card,
            IMapNode whence,
            IEnumerable<IGameAtom> chosenTargets,
            IGameState gameState, 
            IHistoryReader history,
            IHistoryWriter historyWriter)
        {
            _card = card;
            Whence = whence;
            TargetProvider = new RangedTargetProvider(Whence, _card.Range, _card.TargetDescriptors, chosenTargets);
            GameState = gameState;
            History = history;
            _historyWriter = historyWriter;
        }

        public IGameAtom Source => _card;
        public IEffectProvider EffectProvider => _card;
        public IMapNode Whence { get; }
        public ITargetProvider TargetProvider { get; }
        public IGameState GameState { get; }
        public IHistoryReader History { get; }

        public void Dispose()
        {
            var resultsWriter = new ResultsReaderWriter();

            resultsWriter.AddResult(_card.MoveTo(Whence.DiscardPile));
            
            _historyWriter.Append(resultsWriter);
        }
    }
}

