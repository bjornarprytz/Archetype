using Newtonsoft.Json;
using Remote.Linq;
using Remote.Linq.Expressions;
using System;

namespace Archetype.Core
{
    public class CardEffect
    {
        public CardEffect()
        {
            // Only for serialization
        }
        public CardEffect(System.Linq.Expressions.Expression<Func<IEnemy, IGameState, IEffectResult>> expression)
        {
            Expression = expression.ToRemoteLinqExpression();
        }

        public LambdaExpression Expression { get; set; }

        [JsonIgnore]
        public Func<IEnemy, IGameState, IEffectResult> Func => (Func<IEnemy, IGameState, IEffectResult>)Expression.ToLinqExpression().Compile();
    }
}
