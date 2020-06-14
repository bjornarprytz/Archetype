using System;

namespace Archetype
{
    public class ZoneChangeTriggerArgs<T> : TriggerArgs 
        where T : GamePiece
    {
        public T Target { get; set; }
        public Zone<T> From { get; set; }
        public Zone<T> To { get; set; }
        public ZoneChangeTriggerArgs(T target, Zone<T> from, Zone<T> to)
        {
            Target = target;
            From = from;
            To = to;
        }
    }
}
