

using System;

namespace Archetype
{
    public interface IZoned<T> 
        where T : IZoned<T>
    {
        event EventHandler<ZoneChangeArgs<T>> OnZoneChanged;

        Zone<T> CurrentZone { get; }

        void MoveTo(Zone<T> newZone);
    }
}
