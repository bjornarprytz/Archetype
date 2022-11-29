using Archetype.Core.Atoms.Zones;
using Archetype.Core.Effects;
using Archetype.Core.Meta;

namespace Archetype.Core.Atoms;

public interface ICard : IAtom
{
    public string ProtoId { get; }
    public IZone<ICard> CurrentZone { get; }


    [Keyword("Move card to {0}")]
    public IResult MoveTo(IZone<ICard> newZone);
}