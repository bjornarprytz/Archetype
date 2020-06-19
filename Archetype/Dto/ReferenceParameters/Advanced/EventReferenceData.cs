using System;
using System.Reflection;

namespace Archetype
{
    public class EventReferenceData<T> : EventReferenceData
    {
        public EventReferenceData(string memberName)
        {
            EventName = memberName;
            ReferenceType = typeof(T);
        }
    }

    public class EventReferenceData
    {
        public virtual string EventName { get; set; }
        public virtual Type ReferenceType { get; set; }

        public EventInfo GetEventInfo() => ReferenceType.GetEvent(EventName);
    }
}
