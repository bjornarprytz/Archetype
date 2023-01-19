using System.Linq.Expressions;
using Archetype.Core;
using Archetype.Core.Effects;
using Archetype.Core.Proto;
using Archetype.Core.Triggers;

namespace Archetype.Components;

public interface ITriggerBuilder
{
    void PushEffect(Expression<Func<IContext, IResult>> effectFunction);
    public void WithPreCondition(Expression<Func<IContext, bool>> condition);
    public void WhenEventMatches(Expression<Func<IEffectResult, bool>> eventMatchFunction);
    
    ITrigger Build();
}

public interface ISpellBuilder : ICardBuilder
{
    void PushEffect(Expression<Func<IContext, IResult>> effectFunction);

    IProtoSpell Build();
}

public interface ICardBuilder
{
    void PushTrigger(ITrigger trigger);
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