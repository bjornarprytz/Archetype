using System;
using System.Text.Json.Serialization;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Context.Card
{
    public interface ITarget
    {
        string TypeId => TargetType.ReadableFullName();
        Type TargetType { get; }
        bool ValidateContext(ITargetValidationContext context);
    }
    
    public class Target<TTarget> : ITarget
        where TTarget : IGameAtom
    {
        private Func<ITargetValidationContext<TTarget>, bool> _validate;
        
        [JsonIgnore]
        public Func<ITargetValidationContext<TTarget>, bool> Validate
        {
            get
            {
                _validate ??= _ => true;
                return _validate;
            } 
            init => _validate = value;
        }

        public Type TargetType => typeof(TTarget);

        public bool ValidateContext(ITargetValidationContext context)
        {
            return context.Target is TTarget target 
                   && Validate(new TargetValidationContext<TTarget>(context.GameState, target));
        }
    }
}