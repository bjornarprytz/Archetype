using System.Linq.Expressions;
using Archetype.Components.Meta;
using Archetype.Components.Proto;
using Archetype.Core.Effects;
using Archetype.Core.Proto;

namespace Archetype.Components.Builders;

internal class SpellBuilder : CardBuilder<ProtoSpell>, ISpellBuilder
{
    protected override ProtoSpell Proto { get; }
    
    public SpellBuilder()
    {
        Proto = new ProtoSpell();
    }
    
    public void PushEffect(Expression<Func<IContext, IResult>> effectFunction)
    {
        Proto.AddEffect(new Effect(effectFunction));
    }

    public IProtoSpell Build()
    {
        return Proto;
    }

    private record Effect(Expression<Func<IContext, IResult>> EffectExpression) : IEffect;
}
