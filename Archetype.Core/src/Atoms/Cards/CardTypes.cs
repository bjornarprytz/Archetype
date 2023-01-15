using Archetype.Core.Effects;
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
    IZoned,
    ICost,
    IValue,
    IType,
    ITags
{
    CardMetaData MetaData { get; }
    string Name { get; }
    string StaticRulesText { get; }
    public IEnumerable<ITargetDescriptor> TargetDescriptors { get; } // ordered
    public IResult Resolve(IContext context);
    public string ContextualRulesText(IContext context);
}