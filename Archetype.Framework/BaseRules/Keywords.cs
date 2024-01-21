


using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords;

/*
 * The idea is to have each keyword represented by a function that takes in the operands and returns an event.
 * Each function should be annotated with syntax and other meta information.
 */

public static class Keywords
{
    [Effect("CREATE_CARD")]
    public static IEffectResult CreateCard(IResolutionContext context, string cardName, IZone zone)
    {
        if (context.MetaGameState.ProtoData.GetProtoCard(cardName) is not { } protoCard)
            throw new InvalidOperationException($"No card with name {cardName} exists.");
        
        var card = new Card(protoCard)
        {
            CurrentZone = zone
        };
        zone.Add(card);
        context.GameState.AddAtom(card);

        return EffectResult.Resolved;
    }
    
    [Effect("DEAL_DAMAGE")]
    public static IEffectResult DealDamage(IResolutionContext context, IAtom source, IAtom target, int amount)
    {
        return KeywordFrame.Compose(Instance.Bind(ChangeState, target, "HEALTH", amount));
    }
    
    [Effect]
    public static IEffectResult ChangeState<T> (IResolutionContext context, IAtom atom, string property, T value)
    {
        if (atom.GetState<T>(property) is { } existingValue && existingValue.Equals(value))
            return EffectResult.NoOp;
        
        atom.State[property] = value!;
        
        return EffectResult.Resolved;
    }
    
    [Effect]
    public static IEffectResult ChangeZone(IResolutionContext context, IAtom atom, IZone to)
    {
        var from = atom.CurrentZone;

        from?.Remove(atom);
        to.Add(atom);
        atom.CurrentZone = to;

        return EffectResult.Resolved;
    }
    
    [Effect]
    public static IEffectResult DiscardCard(IResolutionContext context, ICard card)
    {
        var player = context.GameState.Player;
        var discardPile = player.DiscardPile;

        Instance.Bind(ChangeZone, card, discardPile);
        
        return KeywordFrame.Compose(
            Instance.Bind(ChangeZone, card, discardPile)
            );
    }
    
    
}

[AttributeUsage(AttributeTargets.Method)]
public class EffectAttribute(string keyword="") : Attribute
{
    public string Keyword { get; } = keyword;
}