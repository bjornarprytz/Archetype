using Archetype.Core.Meta;

namespace Archetype.Core.Atoms.Cards;

public interface IUnit 
    : IAtom, IHealth, IZoned<IUnit>
{
    
}