using System.Linq.Expressions;
using Archetype.Core;
using Archetype.Core.Effects;

namespace Archetype.Components.Extensions;

public static class BuilderExtensions
{
    public static TBuilder AddEffect<TBuilder>(this TBuilder builder, Expression<Func<IContext, IResult>> effectFunction)
    where TBuilder : ISpellBuilder
    {
        builder.PushEffect(effectFunction);
        
        return builder;
    }
    public static TBuilder WithName<TBuilder>(this TBuilder builder, string name)
    where TBuilder : ICardBuilder
    {
        builder.SetName(name);
        return builder;
    }
    public static TBuilder WithRarity<TBuilder>(this TBuilder builder, CardRarity rarity)
    where TBuilder : ICardBuilder
    {
        builder.SetRarity(rarity);
        return builder;
    }
    public static TBuilder WithCost<TBuilder>(this TBuilder builder, int cost)
    where TBuilder : ICardBuilder
    {
        builder.SetCost(cost);
        return builder;
    }
    public static TBuilder WithColor<TBuilder>(this TBuilder builder, CardColor color)
    where TBuilder : ICardBuilder
    {
        builder.SetColor(color);
        return builder;
    }
    public static TBuilder WithArt<TBuilder>(this TBuilder builder, string link)
    where TBuilder : ICardBuilder
    {
        builder.SetArt(link);
        return builder;
    }
    public static TBuilder WithType<TBuilder>(this TBuilder builder, CardType type)
    where TBuilder : ICardBuilder
    {
        builder.SetType(type);
        return builder;
    }
    public static TBuilder WithSubtype<TBuilder>(this TBuilder builder, string subtype)
    where TBuilder : ICardBuilder
    {
        builder.SetSubtype(subtype);
        return builder;
    }
    public static TBuilder WithResources<TBuilder>(this TBuilder builder, int resources)
    where TBuilder : ICardBuilder
    {
        builder.SetResources(resources);
        return builder;
    }
    public static TBuilder FromSet<TBuilder>(this TBuilder builder, string setName)
    where TBuilder : ICardBuilder
    {
        builder.SetCardSet(setName);
        return builder;
    }
    public static TBuilder OverrideRulesText<TBuilder>(this TBuilder builder, string text)
    where TBuilder : ICardBuilder
    {
        builder.OverrideRulesText(text);
        return builder;
    }
}