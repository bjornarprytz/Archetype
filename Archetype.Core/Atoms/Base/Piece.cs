using Archetype.Core.Play;
using Archetype.View.Atoms;
using Archetype.View.Atoms.Zones;

namespace Archetype.Core.Atoms.Base;

internal abstract class Piece<T> : Atom, IPiece<T>
    where T : IPiece
{
    protected Piece(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public IZone<T> CurrentZone { get; private set; }

    public IEffectResult<IPiece<T>, IZone<T>> MoveTo(IZone<T> zone)
    {
        if (zone == CurrentZone)
            return ResultFactory.Null<IPiece<T>, IZone<T>>();

        CurrentZone?.Remove(Self);
        CurrentZone = zone;
        CurrentZone?.Add(Self);

        return ResultFactory.Create(this, CurrentZone);
    }

    protected abstract T Self { get; }
    IZoneFront IPieceFront.CurrentZone => CurrentZone;
}