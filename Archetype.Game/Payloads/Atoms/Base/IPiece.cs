using System;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Context;
using Archetype.View.Atoms;

namespace Archetype.Game.Payloads.Atoms.Base
{
    public interface IPiece<T> : IGameAtom, IPieceFront
        where  T : IGameAtom, IPiece<T>
    {
        IObservable<ZoneTransition<T>> Transition { get; }
        new IZone<T> CurrentZone { get; }

        [Keyword("Move")]
        IResult<IPiece<T>, ZoneTransition<T>> MoveTo(IZone<T> zone);
    }

    public record ZoneTransition<T>(IZone<T> From, IZone<T> To, IPiece<T> Who) where T : IPiece<T>;
}