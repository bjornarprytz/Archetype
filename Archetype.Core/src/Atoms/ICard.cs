using Archetype.Core.Atoms.Zones;

namespace Archetype.Core.Atoms;

public interface ICard : IAtom
{
    public string ProtoId { get; }
    public IZone<ICard> CurrentZone { get; }


    public void MoveTo(IZone<ICard> newZone);
}