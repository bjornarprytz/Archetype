using System;
using Newtonsoft.Json;

namespace Archetype.Core
{
    public class TargetData<TTarget> : ITargetMetaData
        where TTarget : IGamePiece
    {
        private Func<TTarget, IGameState, bool> _validate;
        
        public TargetData(Func<TTarget, IGameState, bool> validationFunc=null)
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
        
        public Type TargetType => typeof(TTarget);
        public string ValidationFunctionName => nameof(Validate);
    }
}