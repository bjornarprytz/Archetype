using System;

namespace Archetype
{
    public class HealTrigger : Trigger<Unit>
    {
        public HealTrigger(EventHandler<TriggerArgs> handler) : base(handler)
        {
        }

        public override void AttachHandler(Unit host)
        {
            host.OnHealed += Handler;
        }

        public override void DetachHandler(Unit host)
        {
            host.OnHealed -= Handler;
        }
    }
}
