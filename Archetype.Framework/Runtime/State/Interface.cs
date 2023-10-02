using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public interface IAtom
{
    Guid Id { get; }
    IReadOnlyDictionary<string, KeywordInstance> Characteristics { get; }
    IDictionary<string, object> State { get; }
}

public interface IZone : IAtom
{
    IList<ICard> Cards { get; }
}

public interface IGameRoot
{
    IMetaGameState MetaGameState { get; }
    IGameState GameState { get; }
    IInfrastructure Infrastructure { get; }
}

public interface IGameState
{
    IDictionary<Guid, IZone> Zones { get; }
    IDictionary<Guid, IAtom> Atoms { get; }
}

public interface IMetaGameState
{
    IDefinitions Definitions { get; }
    IProtoCards ProtoCards { get; }
}

public interface IInfrastructure
{
    IEventHistory EventHistory { get; }
    IActionQueue ActionQueue { get; }
    IGameLoop GameLoop { get; }
    IGameActionHandler GameActionHandler { get; }
}


public interface IActionBlock
{
    IAtom Source { get; }
    IReadOnlyList<CardTargetDescription> TargetsDescriptors { get; }
    IReadOnlyList<KeywordInstance> Effects { get; }
    IReadOnlyList<KeywordInstance> Costs { get; }
    IReadOnlyList<KeywordInstance> Conditions { get; }
    IReadOnlyList<int> ComputedValues { get; }
    
    void UpdateComputedValues(IDefinitions definitions, IGameState gameState);
}

public interface ICard : IAtom, IActionBlock
{
    IReadOnlyDictionary<string, IAbility> Abilities { get; }
    IZone? CurrentZone { get; set; }
}

public interface IAbility : IActionBlock
{
    
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