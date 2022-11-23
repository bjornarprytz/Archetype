using System.Linq.Expressions;

namespace Archetype.Core.Effects;

public interface IEffect
{
    public Expression<Func<IEffectContext, IResult>> ResolveExpression { get; }
}