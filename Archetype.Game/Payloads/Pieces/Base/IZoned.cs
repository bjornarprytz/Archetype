using System;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Context;

namespace Archetype.Game.Payloads.Pieces.Base
{
    public interface IZoned<T> : IGameAtom
        where  T : IGameAtom, IZoned<T>
    {
        IObservable<ZoneTransition<T>> Transition { get; }
        public IZone<T> CurrentZone { get; }

        [Template("Move {0} to {1}")]
        IEffectResult<IZoned<T>, ZoneTransition<T>> MoveTo(IZone<T> zone);
    }
    
    public record ZoneTransition<T>(IZone<T> From, IZone<T> To, IZoned<T> Who) where T : IZoned<T>;
}