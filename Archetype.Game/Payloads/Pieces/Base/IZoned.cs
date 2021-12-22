using System;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Context;

namespace Archetype.Game.Payloads.Pieces.Base
{
    public interface IZonedFront
    {
        IZoneFront CurrentZone { get; }
    }
    
    internal interface IZoned<T> : IGameAtom, IZonedFront
        where  T : IGameAtom, IZoned<T>
    {
        IObservable<ZoneTransition<T>> Transition { get; }
        new IZone<T> CurrentZone { get; }

        [Template("Move {0} to {1}")]
        IResult<IZoned<T>, ZoneTransition<T>> MoveTo(IZone<T> zone);
    }
    
    internal record ZoneTransition<T>(IZone<T> From, IZone<T> To, IZoned<T> Who) where T : IZoned<T>;
}