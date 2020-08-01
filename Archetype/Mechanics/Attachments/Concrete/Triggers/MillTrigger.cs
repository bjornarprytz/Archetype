using System;

namespace Archetype
{
    public class MillTrigger : Trigger<Unit>
    {
        public MillTrigger(EventHandler<TriggerArgs> handler) : base(handler)
        {
        }

        public override void AttachHandler(Unit host)
        {
            host.OnCardMilled += Handler;    
        }

        public override void DetachHandler(Unit host)
        {
            host.OnCardMilled -= Handler;
        }
    }
}
