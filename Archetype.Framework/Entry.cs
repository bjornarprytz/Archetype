using Archetype.Framework.Core;
using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework;

public static class Bootstrap
{
    public static IGameRoot Init(IRules rules, IScope? rootScope)
    {
        return new GameRoot(rootScope ?? new Game(), rules);
    }
}

file class GameRoot(IScope rootScope, IRules rules) : IGameRoot
{
    public IScope RootScope => rootScope;
    public IGameState State { get; } = rules.CreateInitialState();

    public IEnumerable<IEvent> TakeAction(IActionArgs actionArgs)
    {
        var scope = RootScope.GetEdgeScope();

        return rules.ResolveAction(State, scope, actionArgs);
    }
}

file class Rules : IRules
{
    public IGameState CreateInitialState()
    {
        return new GameState();
    }

    public IEnumerable<IEvent> ResolveAction(IGameState state, IScope edgeScope, IActionArgs actionArgs)
    {
        
        return (actionArgs, edgeScope) switch
        {
            (PlayCardArgs playCardArgs, Phase phase) => playCardArgs.BindContext(this, state, phase.NewAction()).ResolveEffects(),
            
            // TODO: Add more action resolvers here
            
            _ => throw new NotImplementedException($"Unable to resolve action {actionArgs} in scope {edgeScope}")
        };
    }

    public Func<IResolutionContext, IEnumerable<IEvent>> BindEffectResolver(EffectProto effectProto, IResolutionContext context)
    {
        throw new NotImplementedException();
    }
}


file static class Extensions
{
    
    public static IResolutionContext BindContext(this PlayCardArgs playCardArgs, IRules rules, IGameState state, GameAction actionScope)
    {
        var card = state.GetAtom<ICard>(playCardArgs.CardId) ?? throw new ArgumentException($"Unable to find card with ID <{playCardArgs.CardId}>", nameof(playCardArgs));
        
        var targets = playCardArgs.Targets
            .Select(id => state.GetAtom(id) ?? throw new ArgumentException($"Unable to find target with ID <{id}", nameof(playCardArgs)))
            .ToArray();
        
        var context = new ResolutionContext(state, actionScope, card, targets);

        if (!context.ValidateContext(out var error))
        {
            throw new ArgumentException($"Invalid context: {error}", nameof(playCardArgs));
        }

        context.BindResolvers(rules);
        
        return context;
    }
    
    private static bool ValidateContext(this IResolutionContext context, out string error)
    {
        var chosenTargets = context.ChosenTargets;
        var targetDescriptors = context.TargetDescriptors;
        
        if (chosenTargets.Count != targetDescriptors.Count)
        {
            error = $"Expected {targetDescriptors.Count} targets, but got {chosenTargets.Count}";
            return false;
        }
        
        foreach (var (target, descriptor) in chosenTargets.Zip(targetDescriptors))
        {
            if (descriptor.Predicates.All(p => p.Evaluate(context, target))) continue;
            
            error = $"Invalid target: {target} does not pass the requirements of {descriptor}";
            return false;
        }
        
        error = string.Empty;
        return true;
    }
    
    private static TAtom? GetAtom<TAtom>(this IGameState state, Guid id) where TAtom : IAtom
    {
        var atom = state.GetAtom(id);
        
        if (atom is TAtom typedAtom)
        {
            return typedAtom;
        }
        
        return default;
    }
    
    public static IScope GetEdgeScope(this IScope scope)
    {
        var edgeScope = scope;
        var loopDetector = new HashSet<IScope>(){ edgeScope };
        
        while (edgeScope.CurrentSubScope != null)
        {
            edgeScope = edgeScope.CurrentSubScope;
            
            if (!loopDetector.Add(edgeScope))
            {
                throw new InvalidOperationException("Scope loop detected");
            }
        }

        return edgeScope;
    }
}