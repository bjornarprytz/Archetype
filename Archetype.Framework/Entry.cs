using System.Reflection;
using Archetype.Framework.Core;
using Archetype.Framework.Effects;
using Archetype.Framework.Effects.Atomic;
using Archetype.Framework.Events;
using Archetype.Framework.GameLoop;
using Archetype.Framework.Parsing;
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

file class Rules(IEnumerable<MethodInfo> effectResolvers, IEnumerable<CardProto> cardPool) : IRules
{
    private readonly Dictionary<string, KeywordBinder> _keywords = effectResolvers.ToDictionary(method => method.GetRequiredAttribute<EffectAttribute>().Keyword, method => new KeywordBinder(method));
    private readonly ICardPool _cardPool = new CardPool(cardPool);
    // TODO: Tests for this implementation
    
    public IGameState CreateInitialState()
    {
        return new GameState();
    }

    public IEnumerable<IEvent> ResolveAction(IGameState state, IScope edgeScope, IActionArgs actionArgs)
    {
        
        return (actionArgs, edgeScope) switch
        {
            (PlayCardArgs playCardArgs, Phase phase) => playCardArgs.BindContext(this, state, phase.NewAction()).ResolveEffects(),
            
            (EndTurnArgs endTurnArgs, Phase phase) => endTurnArgs.BindContext(this, state, phase.NewAction()).ResolveEffects(),
            
            _ => throw new NotImplementedException($"Unable to resolve action {actionArgs} in scope {edgeScope}")
        };
    }

    public Func<IResolutionContext, IEvent> BindEffectResolver(EffectProto effectProto)
    {
        if (!_keywords.TryGetValue(effectProto.Keyword, out var keywordData))
        {
            throw new InvalidOperationException($"Unable to find resolver for effect {effectProto.Keyword}");
        }
        
        return keywordData.BindResolver(effectProto);
    }

    public Func<IResolutionContext, IEvent> BindCostResolver(CostProto costProto)
    {
        // TODO: This should probably be more dynamic, and not hardcoded to the PayResource effect, which could enable more interesting costs, and ways of paying.
        return ctx =>
        {
            var result = AtomicEffect.PayResource(ctx, costProto.ResourceType, costProto.Amount.GetValue(ctx) ?? 0);
            
            return new Event(result, ctx.GetScope());
        };
    }
}

file class CardPool(IEnumerable<ICardProto> cards) : ICardPool
{
    private readonly Dictionary<string, ICardProto> _cards = cards.ToDictionary(card => card.Name, card => card);
    
    public IEnumerable<ICardProto> GetCards()
    {
        return _cards.Values;
    }

    public ICardProto? GetCard(string cardName)
    {
        return _cards.TryGetValue(cardName, out var card) ? card : null;
    }
}


file static class Extensions
{
    
    public static IResolutionContext BindContext(this EndTurnArgs endTurnArgs, IRules rules, IGameState state, GameAction actionScope)
    {
        // TODO: Create action block for ending the turn. Resolve enemy moves, upkeep, etc.
        
        var context = new ResolutionContext(state, actionScope, null, Array.Empty<IAtom>());
        
        context.BindResolvers(rules);
        
        return context;
    }
    
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
        foreach (var cost in context.Costs.Values)
        {
            var playerResource = context.GetState().GetPlayer().GetResouceCount(cost.ResourceType);
            var amountToPay = cost.Amount.GetValue(context);
            
            if (playerResource == null)
            {
                error = $"Resource not found: {cost.ResourceType}";
                return false;
            }
            
            if (amountToPay == null)
            {
                error = $"Invalid amount: {cost.Amount}";
                return false;
            }
            
            if (playerResource < amountToPay)
            {
                error = $"Insufficient resources: {cost.ResourceType} required {cost.Amount.GetValue(context)}, but player only has {playerResource}";
                return false;
            }
        }
        
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