using System.Linq.Expressions;
using Archetype.Core.Effects;

namespace Archetype.Components.Meta;

internal interface IEffect
{
    public Expression<Func<IContext, IResult>> EffectExpression { get; }
}

internal record Effect(Expression<Func<IContext, IResult>> EffectExpression) : IEffect;