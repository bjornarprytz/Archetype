using Archetype.Core.Meta;
using Archetype.Core.Proto;

namespace Archetype.Core.Atoms.Cards;

public interface ISpell : 
    ICard
{ }

public interface IStructure : 
    ICard,
    IHealth,
    IPower,
    ISlots
{ }

public interface IUnit : 
    ICard,
    IHealth,
    IPower,
    IMovement
{ }


public interface ICard : 
    IAtom,
    IPlayable,
    IValue,
    IZoned
{
    CardMetaData MetaData { get; }
    string Name { get; }
}