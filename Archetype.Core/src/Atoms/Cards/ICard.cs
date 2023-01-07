using Archetype.Core.Meta;
using Archetype.Core.Proto;

namespace Archetype.Core.Atoms.Cards;

public interface ICard : 
    IAtom, 
    IZoned
{
    public IProtoCard Proto { get; }
}