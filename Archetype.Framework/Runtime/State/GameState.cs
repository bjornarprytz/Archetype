using Archetype.Rules.Proto;

namespace Archetype.Runtime.State;

public interface IAtom
{
    Guid Id { get; set; }
    IDictionary<string, string> Characteristics { get; set; }
}

public interface IZone : IAtom
{
    IEnumerable<ICard> Cards { get; set; }
}

public interface IGameState
{
    IDictionary<Guid, IZone> Zones { get; set; }
    IDictionary<Guid, IAtom> Atoms { get; set; }
}

public interface ICard : IAtom, IActionBlock
{
    IZone CurrentZone { get; set; }
    ProtoCard Proto { get; set; }
    IReadOnlyList<IAbility> Abilities { get; set; }
    
    IDictionary<string, string> Characteristics { get; set; } // TODO: Define this, with proto and modifiers in mind
    object Modifiers { get; set; } // TODO: Define this
    object RulesText { get; set; } // TODO: Define this
}

public interface IAbility : IActionBlock
{
    public AbilityInstance Proto { get; set; }
}