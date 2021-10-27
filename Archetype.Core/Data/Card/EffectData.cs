using Newtonsoft.Json;
using System;

namespace Archetype.Core
{
    public class EffectData<TTarget, TResult> : IEffectMetaData 
        where TTarget : IGamePiece
    {
        
        private Func<TTarget, IGameState, string> _rulesText;

        public EffectData() { }
        
        public EffectData(
            int targetIndex,
            Func<TTarget, IGameState, TResult> effectFunc,
            Func<TTarget, IGameState, string> rulesTextFunc = null
            )
        {
            rulesTextFunc ??= (_, _) => string.Empty;
            
            Resolve = effectFunc;
            RulesText = rulesTextFunc;
            TargetIndex = targetIndex;
        }

        [JsonIgnore] public Func<TTarget, IGameState, TResult> Resolve { get; set; }

        [JsonIgnore]
        public Func<TTarget, IGameState, string> RulesText
        {
            get
            {
                _rulesText ??= (_, _) => string.Empty;
                return _rulesText;
            }
            set => _rulesText = value;
        }

        public int TargetIndex { get; set; }

        public Type TargetType => typeof(TTarget);
        public Type ResultType => typeof(TResult);

        public string RulesTextFunctionName => nameof(RulesText);
        public string ResolutionFunctionName => nameof(Resolve);
    }
    
    public class EffectData<TResult> : IEffectMetaData
    {
        
        private Func<IGameState, string> _rulesText;

        public EffectData() { }
        
        public EffectData(
            Func<IGameState, TResult> effectFunc,
            Func<IGameState, string> rulesTextFunc = null
        )
        {
            rulesTextFunc ??= _ => string.Empty;
            
            Resolve = effectFunc;
            RulesText = rulesTextFunc;
        }

        [JsonIgnore] public Func<IGameState, TResult> Resolve { get; set; }

        [JsonIgnore]
        public Func<IGameState, string> RulesText
        {
            get
            {
                _rulesText ??= _ => string.Empty;
                return _rulesText;
            }
            set => _rulesText = value;
        }

        public int TargetIndex => -1;

        public Type TargetType => default;
        public Type ResultType => typeof(TResult);

        public string RulesTextFunctionName => nameof(RulesText);
        public string ResolutionFunctionName => nameof(Resolve);
    }
}
