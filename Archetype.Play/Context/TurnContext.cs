using Archetype.Game.Payloads.Context.Phases;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Play.Factory;

namespace Archetype.Play.Context;

public interface ITurnContext
{
    IEnumerable<ICard> PlayableCards { get; }
    IPlayCardContext PlayCard(ICard card);
    Task EndTurn();
}

public class TurnContext : ITurnContext
{
    private readonly IMovePhaseResolver _movePhase;
    private readonly ICombatPhaseResolver _combatPhase;
    private readonly IUpkeepPhaseResolver _upkeepPhase;
    private readonly ISpawnPhaseResolver _spawnPhase;
    private readonly IFactory<IPlayCardContext> _playContextFactory;
    
    public IEnumerable<ICard> PlayableCards { get; }

    public TurnContext(
        IGameState gameState,
        IPlayer player,
        IMovePhaseResolver movePhase,
        ICombatPhaseResolver combatPhase,
        IUpkeepPhaseResolver upkeepPhase,
        ISpawnPhaseResolver spawnPhase,
        IFactory<IPlayCardContext> playContextFactory)
    {
        _movePhase = movePhase;
        _combatPhase = combatPhase;
        _upkeepPhase = upkeepPhase;
        _spawnPhase = spawnPhase;
        _playContextFactory = playContextFactory;

        PlayableCards = player.Hand.Contents
            .Where(card => card.Cost <= player.Resources);
    }

    public IPlayCardContext PlayCard(ICard card)
    {
        if (!PlayableCards.Contains(card))
            throw new InvalidOperationException("Cannot cast that card");

        var context = _playContextFactory.Create();
        context.Init(card);

        return context;
    }

    public async Task EndTurn()
    {
        // TODO: do something async?
        // TODO: Check game over?
        
        _movePhase.Resolve();
        _combatPhase.Resolve();
        _upkeepPhase.Resolve();
        _spawnPhase.Resolve();
    }

}