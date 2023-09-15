using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public interface IAtom
{
    Guid Id { get; set; }
    IDictionary<string, string> Characteristics { get; set; }
}

public interface IZone : IAtom
{
    IList<ICard> Cards { get; set; }
    
}

public interface IGameState
{
    IDictionary<Guid, IZone> Zones { get; set; }
    IDictionary<Guid, IAtom> Atoms { get; set; }
}

public interface IActionBlock
{
    public IAtom Source { get; }
    public IReadOnlyList<EffectInstance> Effects { get; }
    public IReadOnlyList<CostInstance> Costs { get; }

    public object? GetComputedValue(string key);
    void UpdateComputedValues(IDefinitions definitions, IGameState gameState);
    
    IEnumerable<TargetDescription> GetTargetDescriptors();
}

public interface ICard : IAtom, IActionBlock
{
    ProtoCard Proto { get; set; }
    IReadOnlyList<IAbility> Abilities { get; set; }
    
    IZone CurrentZone { get; set; }
    IDictionary<string, string> Characteristics { get; set; }
    object RulesText { get; set; } // TODO: Define this
}

public interface IAbility : IActionBlock
{
    public AbilityInstance Proto { get; set; }
}