using System;

namespace Archetype
{
    public class DefensiveActionModifier<TAct> : ActionModifier<ITarget, TAct>
        where TAct : ModifiableActionInfo
    {

        public DefensiveActionModifier(int modifier=0, float multiplier=1f) : base (modifier, multiplier)
        {
        }

        public override void AttachHandler(ITarget host)
        {
            host.OnTargetOfActionBefore += Handler;
        }

        public override void DetachHandler(ITarget host)
        {
            host.OnTargetOfActionBefore -= Handler;
        }
    }
}
