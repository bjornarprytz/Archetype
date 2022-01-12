using System.Linq;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;

namespace Archetype.Game.Payloads.Context;

public interface IContextBinder
{
    IContext BindContext(ICardPlayArgs args);
    
    // TODO: bind other types of contexts too
}

internal class ContextBinder : IContextBinder
{
    private readonly IGameState _gameState;
    private readonly IInstanceFinder _instanceFinder;
    private readonly IHistoryReader _historyReader;
    private readonly IHistoryWriter _historyWriter;

    public ContextBinder(
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

        var targetProvider = new RangedTargetProvider(node, card.Range, card.Targets, chosenTargets);
        
        return new CardContext(_gameState, _historyReader, _historyWriter, node, targetProvider, card, card);
    }
    
    private record CardContext(
            IGameState GameState, 
            IHistoryReader History,
            IHistoryWriter HistoryWriter,
            IMapNode Whence,
            ITargetProvider TargetProvider,
            IEffectProvider EffectProvider,
            ICard Source
            ) 
        : IContext<ICard>
    {
        IGameAtom IContext.Source => Source;

        public void Dispose()
        {
            var resultsWriter = new ResultsReaderWriter();

            resultsWriter.AddResult(Source.MoveTo(Whence.DiscardPile));
            
            HistoryWriter.Append(resultsWriter);
        }
    }
}