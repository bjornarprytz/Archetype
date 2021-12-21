using Archetype.Game.Payloads.Context.Phases;
using Archetype.Game.Payloads.Pieces;
using Archetype.Play.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Play.Context;

public interface ITurnContext
{
    IEnumerable<ICard> PlayableCards { get; }
    IPlayCardContext PlayCard(ICard card);
    Task<ITurnContext> EndTurn();
}

internal class TurnContext : ITurnContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMovePhaseResolver _movePhase;
    private readonly ICombatPhaseResolver _combatPhase;
    private readonly IUpkeepPhaseResolver _upkeepPhase;
    private readonly ISpawnPhaseResolver _spawnPhase;
    private readonly IPlayCardContextFactory _playContextFactory;
    private readonly IFactory<ITurnContext> _turnContextFactory;

    public IEnumerable<ICard> PlayableCards { get; }

    public TurnContext(
        IServiceProvider serviceProvider,
        IPlayer player,
        IMovePhaseResolver movePhase,
        ICombatPhaseResolver combatPhase,
        IUpkeepPhaseResolver upkeepPhase,
        ISpawnPhaseResolver spawnPhase,
        IPlayCardContextFactory playContextFactory,
        IFactory<ITurnContext> turnContextFactory)
    {
        _serviceProvider = serviceProvider;
        _movePhase = movePhase;
        _combatPhase = combatPhase;
        _upkeepPhase = upkeepPhase;
        _spawnPhase = spawnPhase;
        _playContextFactory = playContextFactory;
        _turnContextFactory = turnContextFactory;

        PlayableCards = player.Hand.Contents
            .Where(card => card.Cost <= player.Resources);
    }

    public IPlayCardContext PlayCard(ICard card)
    {
        if (!PlayableCards.Contains(card))
            throw new InvalidOperationException("Cannot cast that card");

        using var scope = _serviceProvider.CreateScope();
        
        
        return _playContextFactory.Create(card);
    }

    public async Task<ITurnContext> EndTurn()
    {
        // TODO: do something async?
        // TODO: Check game over?
        
        _movePhase.Resolve();
        _combatPhase.Resolve();
        _upkeepPhase.Resolve();
        _spawnPhase.Resolve();

        return _turnContextFactory.Create();
    }

}