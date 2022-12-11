using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Cards;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Effects;
using Archetype.Core.Meta;

namespace Archetype.Components.Extensions;

public static class ContextExtensions
{
    public static IResult TargetEach<TTarget, TResult>(this IEnumerable<TTarget> source, Func<TTarget, IResult> func)
        where TTarget : IAtom
    {
        return Result.Aggregate(source.Select(func).ToList());
    }

    public static T Target<T>(this IContext context, int index)
        where T : IAtom
    {
        return context.TargetProvider.GetTarget<T>(index);
    }

    [Description("Hand")]
    public static IHand Hand(this IContext context)
    {
        return context.GameState.Player.Hand;
    }
    
    [Description("Cards in hand")]
    public static IEnumerable<ICard> CardsInHand(this IContext context)
    {
        return context.GameState.Player.Hand.Contents;
    }
    
    [Description("Cards in the discard pile")]
    public static IEnumerable<ICard> DiscardPile(this IContext context)
    {
        return context.GameState.Player.DiscardPile.Contents;
    }
}