using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework;

public static class Bootstrap
{
    public static IGameRoot StartGame(IGameState initialState, IRules rules)
    {
        return new GameRoot(initialState, rules);
    }
}

file class GameRoot(IGameState initialState, IRules rules) : IGameRoot
{
    private readonly IRules _rules = rules;
    private readonly IGameLoop _loop = new Loop();
    private readonly IGameEvents _events = new GameEvents();

    public IGameState State { get; } = initialState;

    public IEnumerable<IEvent> TakeAction(IActionArgs actionArgs)
    {
        var scope = _loop.GetCurrentScope();
        
        if (!scope.IsActonAllowed(actionArgs))
        {
            throw new InvalidOperationException($"Action {{{actionArgs}}} not allowed in current scope: <{scope.Level}>.");
        }
        
        return (actionArgs) switch {
            PlayCardArgs args => PlayCard(args),
            EndTurnArgs args => EndTurn(args),
            StartGameArgs args => StartGame(args),
            _ => throw new NotImplementedException()
        };
    }
    
    private IEnumerable<IEvent> PlayCard(PlayCardArgs args)
    {
        var card = State.GetHand().GetAtom<ICard>(args.CardId) ?? throw new ArgumentException($"Invalid card ID <{args.CardId}>. Card was not found in hand.");
        var targets = args.Targets.Select(targetId => State.GetAtom(targetId) ?? throw new ArgumentException($"Invalid target ID <{targetId}>. Target was not found in game state.")).ToArray();
        
        var context = new ResolutionContext(_loop.GetCurrentScope(), card, targets);

        var events = new List<IEvent>();

        // TODO: Resolve costs

        foreach (var effectProto in card.GetProto().Effects)
        {
            foreach (var e in  _rules.ResolveEffect(context, effectProto))
            {
                _events.PushEvent(e);
                events.Add(e);
            }
        }
        
        return events;
    }
    
    private IEnumerable<IEvent> EndTurn(EndTurnArgs args)
    {
        throw new NotImplementedException();
    }
    
    private IEnumerable<IEvent> StartGame(StartGameArgs args)
    {
        throw new NotImplementedException();
    }
}

file class GameEvents : IGameEvents
{
    private readonly List<IEvent> _events = new();
    
    public IEnumerable<IEvent> GetEvents()
    {
        return _events;
    }

    public void PushEvent(IEvent @event)
    {
        _events.Add(@event);
    }
}

file class Loop : IGameLoop
{
    public IScope GetCurrentScope()
    {
        throw new NotImplementedException();
    }
}