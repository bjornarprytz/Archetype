using System;

namespace Archetype
{
    public class DiscardTrigger : Trigger<Unit>
    {
        public DiscardTrigger(EventHandler<TriggerArgs> handler) : base(handler)
        {
        }

        public override void AttachHandler(Unit host)
        {
            host.OnCardDiscarded += Handler;
        }

        public override void DetachHandler(Unit host)
        {
            host.OnCardDiscarded -= Handler;
        }
    }
}
