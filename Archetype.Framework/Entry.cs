using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework;

public static class Bootstrap
{
    public static IGameRoot StartGame(IRules rules, ICardPool cardPool)
    {
        return new GameRoot(rules, cardPool);
    }
}

file class GameRoot(IRules rules, ICardPool cardPool) : IGameRoot
{
    public Game RootScope { get; } = new Game();
    public IGameState? State { get; private set; }
    public ICardPool CardPool => cardPool;

    public IEnumerable<IEvent> TakeAction(IActionArgs actionArgs)
    {
        var scope = RootScope.GetEdgeScope();

        if (!rules.ValidateAction(State, scope, actionArgs, out var error))
        {
            throw new InvalidOperationException($"Invalid action <{actionArgs}>. {error}");
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
        var scope = new GameAction(); // TODO: Create Action context properly based on the current phase
        
        if (!rules.TryBindContext(State!, scope, args, out var context))
        {
            throw new ArgumentException($"Invalid action <{args}>. Could not bind context.");
        }

        // TODO: Resolve costs

        foreach (var effectProto in context!.GetEffects())
        {
            foreach (var e in  rules.ResolveEffect(context, effectProto))
            {
                scope.AddEvent(e);
            }
        }
        
        return scope.Events;
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
