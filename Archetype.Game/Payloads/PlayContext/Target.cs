using System;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;
using Newtonsoft.Json;

namespace Archetype.Game.Payloads.PlayContext
{
    public interface ITarget
    {
        Type TargetType { get; }
        bool ValidateContext(ITargetValidationContext context);
    }
    
    public class Target<TTarget> : ITarget
        where TTarget : IGameAtom
    {
        private Func<ITargetValidationContext<TTarget>, bool> _validate;
        
        public Target(Func<ITargetValidationContext<TTarget>, bool> validationFunc=null)
        {
            validationFunc ??= _ => true;

            _validate = validationFunc;
        }
        
        [JsonIgnore]
        public Func<ITargetValidationContext<TTarget>, bool> Validate
        {
            get
            {
                _validate ??= _ => true;
                return _validate;
            } 
            set => _validate = value;
        }

        public Type TargetType => typeof(TTarget);

        public bool ValidateContext(ITargetValidationContext context)
        {
            return context.Target is TTarget target 
                   && Validate(new TargetValidationContext<TTarget>(context.GameState, target));
        }
    }
}