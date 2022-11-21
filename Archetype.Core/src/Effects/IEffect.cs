using System.Linq.Expressions;
using Archetype.Core.Effects.Resolution;

namespace Archetype.Core.Effects;

public interface IEffect
{
    public Expression<Func<IEffectContext, IEffectResult>> ResolveExpression { get; }
}