﻿namespace Archetype
{
    public class OffensiveActionModifier<THost, TAct> : ActionModifier<THost, TAct>
        where THost : ISource
        where TAct : ModifiableActionInfo
    {
        public OffensiveActionModifier(int modifier=0, float multiplier=1f) : base(modifier, multiplier)
        {
        }

        public override void AttachHandler(THost host)
        {
            host.OnSourceOfActionBefore += Handler;
        }

        public override void DetachHandler(THost host)
        {
            host.OnSourceOfActionBefore -= Handler;
        }
        
    }
}