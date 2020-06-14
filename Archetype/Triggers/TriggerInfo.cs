using System;

namespace Archetype
{
    public class TriggerInfo<T> : ITrigger
        where T : TriggerArgs
    {

        public TriggerInfo(EventHandler<T> triggerHandler)
        {

        }

        public void DisableTrigger()
        {
            throw new System.NotImplementedException();
        }

        public void EnableTrigger()
        {
            throw new System.NotImplementedException();
        }
    }
}
