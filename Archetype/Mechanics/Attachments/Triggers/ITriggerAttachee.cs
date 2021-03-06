﻿using System.Collections.Generic;

namespace Archetype
{
    public interface ITriggerAttachee<THost>
        where THost : class, ITriggerAttachee<THost>
    {
        List<Trigger<THost>> Triggers { get; }

        public void AttachTrigger(Trigger<THost> trigger)
        {
            trigger.AttachHandler(this as THost);
            Triggers.Add(trigger);

        }
        public void DetachTrigger(Trigger<THost> trigger)
        {
            Triggers.Remove(trigger);
            trigger.DetachHandler(this as THost);
        }
    }
}
