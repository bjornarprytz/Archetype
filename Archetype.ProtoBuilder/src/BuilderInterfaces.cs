using System.Linq.Expressions;
using Archetype.Core;
using Archetype.Core.Effects;
using Archetype.Core.Proto.PlayingCard;

namespace Archetype.Components;

public interface ISpellBuilder : ICardBuilder
{
    void PushEffect(Expression<Func<IContext, IResult>> effectFunction);

    IProtoSpell Build();
}

public interface ICardBuilder
{
    public void SetName(string name);
    public void SetRarity(CardRarity rarity);
    public void SetCost(int cost);
    public void SetColor(CardColor color);
    public void SetArt(string link);
    public void SetType(CardType type);
    public void SetSubtype(string subtype);
    public void SetResources(int resources);
    public void SetCardSet(string setName);
}