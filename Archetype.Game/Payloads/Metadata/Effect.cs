using System;
using System.Collections.Generic;
using Archetype.Core;
using Archetype.Game.Payloads.Pieces;
using Newtonsoft.Json;

namespace Archetype.Game.Payloads.Metadata
{
    public class Effect<TTarget, TResult> : IEffect 
        where TTarget : IGamePiece
    {
        
        private Func<TTarget, IGameState, string> _rulesText;

        public Effect() { }
        
        public Effect(
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
        public object CallResolveMethod(IList<IGamePiece> availableTargets, IGameState gameState)
        {
            dynamic target = availableTargets[TargetIndex];
            
            return Resolve(target, gameState);
        }

        public string CallTextMethod(IList<IGamePiece> availableTargets, IGameState gameState)
        {
            dynamic target = availableTargets[TargetIndex];

            return RulesText(target, gameState);
        }
    }
    
    public class Effect<TResult> : IEffect
    {
        private Func<IGameState, string> _rulesText;

        public Effect() { }
        
        public Effect(
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

        public object CallResolveMethod(IList<IGamePiece> availableTargets, IGameState gameState)
        {
            return Resolve(gameState);
        }
        
        public string CallTextMethod(IList<IGamePiece> availableTargets, IGameState gameState)
        {
            return RulesText(gameState);
        }
    }
}
