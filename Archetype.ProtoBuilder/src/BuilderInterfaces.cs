using System.Linq.Expressions;
using Archetype.Core;
using Archetype.Core.Effects;
using Archetype.Core.Proto;

namespace Archetype.Components;

public interface ISpellBuilder : ICardBuilder
{
    void PushEffect(Expression<Func<IContext, IResult>> effectFunction);

    IProtoSpell Build();
}

public interface ICardBuilder
{
    void SetName(string name);
    void SetRarity(CardRarity rarity);
    void SetCost(int cost);
    void SetColor(CardColor color);
    void SetArt(string link);
    void SetType(CardType type);
    void WithTags(params string[] rest);
    void SetResources(int resources);
    void SetCardSet(string setName);
}