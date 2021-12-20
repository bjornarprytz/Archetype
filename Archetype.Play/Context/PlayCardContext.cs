using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Play.Context;

public interface IPlayCardContext
{
    IEnumerable<ITarget> RequiredTargets { get; }
    IGameState GameState { get; }

    bool ValidateArgs(IMapNode whence, IEnumerable<IGameAtom> targets);
    void Play(IMapNode whence, IEnumerable<IGameAtom> targets);
}

internal class PlayCardContext : IPlayCardContext
{
    private readonly IPlayer _player;
    private readonly ICardResolver _cardResolver;
    private ICard? _card;

    public PlayCardContext(IPlayer player, IGameState gameState, ICardResolver cardResolver)
    {
        _player = player;
        _cardResolver = cardResolver;
        GameState = gameState;
    }
    
    internal void Init(ICard card)
    {
        if (_player.Resources < card.Cost)
            throw new InvalidOperationException("Player cannot afford to play card");
        
        _card = card;
    }

    public IEnumerable<ITarget> RequiredTargets => _card?.Targets;
    public IGameState GameState { get; }
    
    public bool ValidateArgs(IMapNode whence, IEnumerable<IGameAtom> targets)
    {
        if (_card is null)
            throw new InvalidOperationException("Card is null");
        
        var requiredTargets = RequiredTargets.ToList();
        var chosenTargets = targets.ToList();
            
        if (requiredTargets.Count != chosenTargets.Count)
        {
            return false; // throw new InvalidOperationException("Wrong target count");
        }
            
        foreach (var (targetData, chosenTarget) in requiredTargets.Zip(chosenTargets))
        {
            if (chosenTarget is IUnit { CurrentZone: IMapNode node }
                && node.DistanceTo(whence) is var distance and > -1 
                && distance > _card.Range)
            {
                return false; //throw new InvalidOperationException($"Target is too far from whence. Card Range: {_card.Range}, Distance: {distance}");
            }
                
            if (!targetData.ValidateContext(new TargetValidationContext(GameState, chosenTarget)))
            {
                return false;// throw new InvalidTargetChosenException();
            }
        }

        return true;
    }

    public void Play(IMapNode whence, IEnumerable<IGameAtom> targets)
    {
        if (!ValidateArgs(whence, targets))
            throw new InvalidOperationException("Invalid args");
        
        _cardResolver.Resolve(new PlayCardArgs(_player, _card!, whence, targets!));
    }

    private record PlayCardArgs
        (IPlayer Player, ICard Card, IMapNode Whence, IEnumerable<IGameAtom> Targets) : ICardPlayArgs;

}