using System;
using System.Reflection;

namespace Archetype
{
    public class ApplyLastingEffectActionArgs : ActionInfo
    {
        public LastingEffect LastingEffect;

        public ApplyLastingEffectActionArgs(ISource source, ITarget target, LastingEffect lastingEffect) : base(source, target)
        {
            LastingEffect = lastingEffect;
        }

        protected override void Resolve()
        {
            LastingEffect.Apply(Target);
        }
    }
}
