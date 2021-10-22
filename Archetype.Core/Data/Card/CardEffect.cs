using Newtonsoft.Json;
using Remote.Linq;
using Remote.Linq.Expressions;
using System;

namespace Archetype.Core
{
    public class CardEffect<TTarget, TResult> : IEffect
        where TTarget : IGamePiece
    {
        private Func<TTarget, IGameState, TResult> _resolve;
        private Func<TTarget, IGameState, bool> _validate;
        
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
        public Func<TTarget, IGameState, TResult> Resolve
        {
            get
            {
                _resolve ??= (Func<TTarget, IGameState, TResult>)EffectExpression.ToLinqExpression().Compile();
                
                return _resolve;
            }
        }

        [JsonIgnore]
        public Func<TTarget, IGameState, bool> Validate
        {
            get
            {
                _validate ??= (Func<TTarget, IGameState, bool>)TargetValidationExpression.ToLinqExpression().Compile();

                return _validate;
            }
        } 
            

        public Type TargetType => typeof(TTarget);
        public Type ResultType => typeof(TResult);
    }
}
