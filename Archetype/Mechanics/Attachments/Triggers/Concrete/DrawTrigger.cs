using System;

namespace Archetype
{
    public class DrawTrigger : Trigger<Unit>
    {
        public DrawTrigger(EventHandler<TriggerArgs> handler) : base(handler)
        {
        }

        public override void AttachHandler(Unit host)
        {
            host.OnCardDrawn += Handler;
        }

        public override void DetachHandler(Unit host)
        {
            host.OnCardDrawn -= Handler;
        }
    }
}
