using Remote.Linq;
using Remote.Linq.Expressions;
using System;

namespace Archetype.Core
{
    public class CardEffect
    {
        public CardEffect(System.Linq.Expressions.Expression<Func<IEnemy, IGameState, IEffectResult>> expression)
        {
            Expression = expression.ToRemoteLinqExpression();
        }

        public LambdaExpression Expression { get; set; }

        public Func<IEnemy, IGameState, IEffectResult> Func => (Func<IEnemy, IGameState, IEffectResult>)Expression.ToLinqExpression().Compile();
    }
}
