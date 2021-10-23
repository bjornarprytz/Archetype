using Newtonsoft.Json;
using Remote.Linq;
using Remote.Linq.Expressions;
using System;

namespace Archetype.Core
{
    public class EffectData<TTarget, TResult> : IEffectMetaData 
        where TTarget : IGamePiece
    {
        private Func<TTarget, IGameState, bool> _validate;
        private Func<TTarget, IGameState, string> _rulesText;

        public EffectData() { }
        
        public EffectData(
            Func<TTarget, IGameState, TResult> effectFunc,
            Func<TTarget, IGameState, bool> validationFunc = null,
            Func<TTarget, IGameState, string> rulesTextFunc = null
            )
        {
            validationFunc ??= (piece, state) => true;
            rulesTextFunc ??= (piece, state) => string.Empty;
            
            Validate = validationFunc;
            Resolve = effectFunc;
            RulesText = rulesTextFunc;
        }

        [JsonIgnore] public Func<TTarget, IGameState, TResult> Resolve { get; set; }

        [JsonIgnore]
        public Func<TTarget, IGameState, bool> Validate
        {
            get
            {
                _validate ??= (piece, state) => true;
                return _validate;
            } 
            set => _validate = value;
        }

        [JsonIgnore]
        public Func<TTarget, IGameState, string> RulesText
        {
            get
            {
                _rulesText ??= (piece, state) => string.Empty;
                return _rulesText;
            }
            set => _rulesText = value;
        }


        public Type TargetType => typeof(TTarget);
        public Type ResultType => typeof(TResult);
        
        public string RulesTextFunctionName => nameof(RulesText);
        public string ValidationFunctionName => nameof(Validate);
        public string ResolutionFunctionName => nameof(Resolve);
    }
}
