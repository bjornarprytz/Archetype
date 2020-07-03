using System;

namespace Archetype
{
    public abstract class Trigger<THost> : Attachment<THost, TriggerArgs>
        where THost : class, ITriggerAttachee<THost>
    {
        private EventHandler<TriggerArgs> _handler;

        protected Trigger(EventHandler<TriggerArgs> handler)
        {
            _handler = handler;
        }

        protected override void Handler(object sender, TriggerArgs args)
        {
            _handler(sender, args);
        }
    }
}
