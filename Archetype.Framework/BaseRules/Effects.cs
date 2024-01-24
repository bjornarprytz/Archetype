


using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords;

/*
 * The idea is to have each keyword represented by a function that takes in the operands and returns a result.
 * The result can be anything useful for the engine to know how to resolve the keyword. Some keywords unravel into other keywords. Some "resolve" into a prompt.
 * Each function should be annotated with syntax and other meta information.
 */

public static class Effects
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
    public static IEffectResult Shuffle(IResolutionContext context, IOrderedZone zone)
    {
        zone.Shuffle();
        return EffectResult.Resolved;
    }
    
    [Effect]
    public static IEffectResult ChooseAndDiscardCard(IResolutionContext context)
    {
        var player = context.GameState.Player;
        
        var promptId = Guid.NewGuid();
        
        return KeywordFrame.Compose(
            Instance.BindArgs(Prompt.PickOne, promptId, player.Hand.ToAtomProvider(), "Choose a card to discard."),
            Instance.Bind(DiscardCards, new PromptRef<ICard>(promptId)));
    }
    
    [Effect]
    public static IEffectResult DiscardCards(IResolutionContext context, IEnumerable<ICard> cards)
    {
        var player = context.GameState.Player;
        var discardPile = player.DiscardPile;
        
        return KeywordFrame.Compose(
            cards.Select(card => Instance.BindArgs(ChangeZone, card, discardPile)).ToArray()
            );
    }
    
    
}

[AttributeUsage(AttributeTargets.Method)]
public class EffectAttribute(string keyword="") : Attribute
{
    public string Keyword { get; } = keyword;
}