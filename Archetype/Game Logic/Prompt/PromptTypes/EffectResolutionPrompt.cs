using System;

namespace Archetype
{
    public class EffectResolutionPrompt : UserPrompt
    {
        public Effect EffectToResolve { get; private set; }

        protected override Type _typeRestriction => typeof(object); // TODO: Specify this more

        public EffectResolutionPrompt(int x, Type requiredType)
            : base (x, requiredType)
        {
        }

        public EffectResolutionPrompt(int min, int max, Type requiredType)
            : base(min, max, requiredType)
        {
        }
    }
}
