using System;
using Archetype.Core;
using Archetype.Game.Payloads.Pieces;
using Newtonsoft.Json;

namespace Archetype.Game.Payloads.Metadata
{
    public class Target<TTarget> : ITarget
        where TTarget : IGamePiece
    {
        private Func<TTarget, IGameState, bool> _validate;
        
        public Target(Func<TTarget, IGameState, bool> validationFunc=null)
        {
            validationFunc ??= (_, _) => true;

            _validate = validationFunc;
        }
        
        [JsonIgnore]
        public Func<TTarget, IGameState, bool> Validate
        {
            get
            {
                _validate ??= (_, _) => true;
                return _validate;
            } 
            set => _validate = value;
        }
        
        public bool CallTargetValidationMethod(IGamePiece gamePiece, IGameState gameState)
        {
            dynamic dynGamePiece = gamePiece;

            return Validate(dynGamePiece, gameState);
        }

        public TargetData CreateReadOnlyData()
        {
            return new TargetData
            {
                TargetType = typeof(TTarget),
                ValidationFunctionName = nameof(Validate)
            };
        }
    }
}