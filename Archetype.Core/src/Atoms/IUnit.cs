using Archetype.Core.Effects;
using Archetype.Core.Meta;

namespace Archetype.Core.Atoms;

public interface IUnit : IAtom
{
    [Description("Health")]
    public int Health { get; }
    
    [Keyword("Deal {0} damage to this unit.")]
    public IResult Damage(int amount);
}