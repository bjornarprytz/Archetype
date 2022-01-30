using System;
using Archetype.Game.Attributes;
using Archetype.Game.Payloads.Context;
using Archetype.View.Atoms;
using Archetype.View.Events;

namespace Archetype.Game.Payloads.Atoms.Base
{
    public interface IPiece : IGameAtom, IPieceFront{}
    
    public interface IPiece<T> : IPiece
        where  T : IPiece
    {
        new IObservable<IAtomMutation<T>> OnMutation { get; }
        
        new IZone<T> CurrentZone { get; }

        IResult<IPiece<T>, IZone<T>> MoveTo(IZone<T> zone);
    }
}