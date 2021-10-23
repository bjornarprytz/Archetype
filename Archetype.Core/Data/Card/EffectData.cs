using Newtonsoft.Json;
using Remote.Linq;
using Remote.Linq.Expressions;
using System;

namespace Archetype.Core
{
    public class EffectData<TTarget, TResult> : IEffectMetaData 
        where TTarget : IGamePiece
    {
        private Func<TTarget, IGameState, TResult> _resolve;
        private Func<TTarget, IGameState, bool> _validate;
        
        private readonly LambdaExpression _effectExpression;
        private readonly LambdaExpression _targetValidationExpression;
        
        public EffectData(
            System.Linq.Expressions.Expression<Func<TTarget, IGameState, TResult>> effectExpression,
            System.Linq.Expressions.Expression<Func<TTarget, IGameState, bool>> validationExpression = null
            )
        {
            validationExpression ??= (piece, state) => true;
            
            _effectExpression = effectExpression.ToRemoteLinqExpression();
            _targetValidationExpression = validationExpression.ToRemoteLinqExpression();
        }
        
        [JsonIgnore]
        public Func<TTarget, IGameState, TResult> Resolve
        {
            get
            {
                _resolve ??= (Func<TTarget, IGameState, TResult>)_effectExpression.ToLinqExpression().Compile();
                
                return _resolve;
            }
        }

        [JsonIgnore]
        public Func<TTarget, IGameState, bool> Validate
        {
            get
            {
                _validate ??= (Func<TTarget, IGameState, bool>)_targetValidationExpression.ToLinqExpression().Compile();

                return _validate;
            }
        } 
            

        public Type TargetType => typeof(TTarget);
        public Type ResultType => typeof(TResult);
        public string ValidationFunctionName => nameof(Validate);
        public string ResolutionFunctionName => nameof(Resolve);
    }
}
