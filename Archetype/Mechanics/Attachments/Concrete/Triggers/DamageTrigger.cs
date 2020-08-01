using System;

namespace Archetype
{
    public class DamageTrigger : Trigger<Unit>
    {
        public DamageTrigger(EventHandler<TriggerArgs> handler) : base(handler)
        {
        }

        public override void AttachHandler(Unit host)
        {
            host.OnDamaged += Handler;
        }

        public override void DetachHandler(Unit host)
        {
            host.OnDamaged -= Handler;
        }
    }
}
