using System;
using Archetype.Game.Payloads.Context;

namespace Archetype.Game.Payloads.Pieces.Base
{
    public interface IZoned<T> : IGameAtom
        where  T : IGameAtom, IZoned<T>
    {
        IObservable<ZoneTransition<T>> Transition { get; }
        public IZone<T> CurrentZone { get; }

        IEffectResult<IZoned<T>, ZoneTransition<T>> MoveTo(IZone<T> zone);
    }
    
    public record ZoneTransition<T>(IZone<T> From, IZone<T> To, IZoned<T> Who) where T : IZoned<T>;
}