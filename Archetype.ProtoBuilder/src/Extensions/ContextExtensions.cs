using Archetype.Components.Meta;
using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Zones;
using Archetype.Core.Effects;

namespace Archetype.Components.Extensions;

internal static class ContextExtensions
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
    
    [Description("Cards in your hand")]
    public static IEnumerable<ICard> Hand(this IContext context)
    {
        return context.GameState.Player.Hand.Contents;
    }
}