using Archetype.Core.Effects;
using Archetype.Core.Meta;

namespace Archetype.Core.Atoms.Cards;

public interface IUnit : ICard
{
    [Description("Health")]
    public int Health { get; }
    
    [Keyword("Deal {0} damage to this unit.")]
    public IResult Damage(int amount);
}