using Archetype.Core.Play;
using Archetype.View.Atoms;

namespace Archetype.Core.Atoms.Base;

public interface IPiece : IGameAtom, IPieceFront{}
    
public interface IPiece<T> : IPiece
    where  T : IPiece
{
    new IZone<T> CurrentZone { get; }

    IEffectResult<IPiece<T>, IZone<T>> MoveTo(IZone<T> zone);
}