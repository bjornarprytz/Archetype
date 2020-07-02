using System;

namespace Archetype
{
    public class ShuffleTrigger : Trigger<Unit>
    {
        public ShuffleTrigger(EventHandler<TriggerArgs> handler) : base(handler)
        {
        }

        public override void AttachHandler(Unit host)
        {
            host.OnDeckShuffled += Handler;
        }

        public override void DetachHandler(Unit host)
        {
            host.OnDeckShuffled -= Handler;
        }
    }
}
