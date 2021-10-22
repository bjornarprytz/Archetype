using Newtonsoft.Json;
using Remote.Linq;
using Remote.Linq.Expressions;
using System;

namespace Archetype.Core
{
    public class CardEffect<TTarget, TResult> : IEffect
        where TTarget : IGamePiece
    {
        public CardEffect()
        {
            // Only for serialization
        }
        public CardEffect(
            System.Linq.Expressions.Expression<Func<TTarget, IGameState, TResult>> effectExpression,
            System.Linq.Expressions.Expression<Func<TTarget, IGameState, bool>> validationExpression = null
            )
        {
            validationExpression ??= (piece, state) => true;
            
            EffectExpression = effectExpression.ToRemoteLinqExpression();
            TargetValidationExpression = validationExpression.ToRemoteLinqExpression();
        }

        public LambdaExpression EffectExpression { get; set; }
        public LambdaExpression TargetValidationExpression { get; set; }

        [JsonIgnore]
        public Func<TTarget, IGameState, TResult> Resolve => (Func<TTarget, IGameState, TResult>)EffectExpression.ToLinqExpression().Compile();
        
        [JsonIgnore]
        public Func<TTarget, IGameState, bool> Validate => (Func<TTarget, IGameState, bool>)TargetValidationExpression.ToLinqExpression().Compile();

        public Type TargetType => typeof(TTarget);
        public Type ResultType => typeof(TResult);
    }
}
