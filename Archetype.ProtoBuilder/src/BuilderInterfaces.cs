using System.Linq.Expressions;
using Archetype.Core;
using Archetype.Core.Effects;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Components;

public interface ISpellBuilder : ICardBuilder
{
    void AddEffect(Expression<Func<IContext, IResult>> effectFunction);

    IProtoSpell Build();
}

public interface ICardBuilder
{
    public void WithName(string name);
    public void WithRarity(CardRarity rarity);
    public void WithCost(int cost);
    public void WithColor(CardColor color);
    public void WithArt(string link);
    public void WithType(CardType type);
    public void WithSubtype(string subtype);
    public void WithResources(int resources);
    public void FromSet(string setName);
    public void OverrideRulesText(string text);
}