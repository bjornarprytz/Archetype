using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public interface IAtom
{
    Guid Id { get; }
    IReadOnlyDictionary<string, string> Characteristics { get; }
}

public interface IZone : IAtom
{
    IList<ICard> Cards { get; }
}

public interface IGameState
{
    IDictionary<Guid, IZone> Zones { get; }
    IDictionary<Guid, IAtom> Atoms { get; }
}

public interface IActionBlock
{
    IAtom Source { get; }
    IReadOnlyList<TargetDescription> TargetsDescriptors { get; }
    IReadOnlyList<EffectInstance> Effects { get; }
    IReadOnlyList<CostInstance> Costs { get; }
    IReadOnlyList<ConditionInstance> Conditions { get; }

    object? GetComputedValue(int index);
    void UpdateComputedValues(IDefinitions definitions, IGameState gameState);
}

public interface ICard : IAtom, IActionBlock
{
    IReadOnlyList<IAbility> Abilities { get; }
    IReadOnlyList<FeatureInstance> Features { get; }
    IReadOnlyList<ReactionInstance> Reactions { get; }

    IZone CurrentZone { get; set; }
}

public interface IAbility : IActionBlock
{
    IReadOnlyList<FeatureInstance> Features { get; }
}

public interface IGamePhase
{
    string Name { get; }
    IReadOnlyList<IGameStep> Steps { get; }
    IReadOnlyList<ActionDescription> GameActions { get; }
}

public interface IGameStep : IActionBlock
{
    string Name { get; }
}