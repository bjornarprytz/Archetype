
using System;

namespace Archetype
{
    public class ZoneChangeEventArgs<T> : EventArgs
        where T : IZoned<T>
    {
        public T Object { get; set; }
        public Zone<T> From { get; set; }
        public Zone<T> To { get; set; }

        public ZoneChangeEventArgs(T obj, Zone<T> from, Zone<T> to)
        {
            Object = obj;
            From = from;
            To = to;
        }
    }
}
