using System.Linq.Expressions;

namespace Archetype.Core.Effects;

public interface IEffect
{
    public Expression<Func<IContext, IResult>> ResolveExpression { get; }
}