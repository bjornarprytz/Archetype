
using System;

namespace Archetype
{
    public class ZoneChangeArgs<T> : EventArgs
        where T : IZoned<T>
    {
        public T Object { get; set; }
        public Zone<T> From { get; set; }
        public Zone<T> To { get; set; }

        public ZoneChangeArgs(T obj, Zone<T> from, Zone<T> to)
        {
            Object = obj;
            From = from;
            To = to;
        }
    }
}
