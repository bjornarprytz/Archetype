using System.Linq.Expressions;
using Archetype.Components.Meta;
using Archetype.Components.Protos;
using Archetype.Core.Effects;

namespace Archetype.Components.Builders;

internal class SpellBuilder : CardBuilder<ProtoSpell>
{
    protected override ProtoSpell Proto { get; }
    
    public SpellBuilder()
    {
        Proto = new ProtoSpell();
    }
    
    public void AddEffect(Expression<Func<IContext, IResult>> effectFunction)
    {
        Proto.AddEffect(new Effect(effectFunction));
    }

    private record Effect(Expression<Func<IContext, IResult>> ResolveExpression) : IEffect;
}
