using System.Collections.Generic;

namespace Archetype
{
    public interface ITriggerAttachee<THost>
    {
        void AttachTrigger(Trigger<THost> trigger);
        void DetachTrigger(Trigger<THost> trigger);
    }
}
