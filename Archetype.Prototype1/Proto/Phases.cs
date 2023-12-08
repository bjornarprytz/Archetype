using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.Extensions;
using Archetype.Framework.Interface;
using Archetype.Framework.Interface.Actions;
using Archetype.Framework.State;
using Archetype.Prototype1.Keywords;

namespace Archetype.Prototype1.Proto;

public class MainPhase(IRules rules) : Phase
{
    public override string Name { get; } = "Main Phase";
    public override IReadOnlyList<IKeywordInstance> Steps { get; } = new []
    {
        rules.GetOrThrow<DrawCard>().CreateInstance()
    };

    public override IReadOnlyList<ActionDescription> AllowedActions { get; } = new[]
    {
        new ActionDescription(ActionType.PlayCard), 
        new ActionDescription(ActionType.PassTurn), 
        new ActionDescription(ActionType.UseAbility)
    };
}

public class EnemyPhase(IRules rules) : Phase
{
    public override string Name { get; } = "Enemy Phase";
    public override IReadOnlyList<IKeywordInstance> Steps { get; } = new []
    {
        rules.GetOrThrow<EnemyPhaseResolver>().CreateInstance()
    };

    public override IReadOnlyList<ActionDescription> AllowedActions { get; } = ArraySegment<ActionDescription>.Empty;
}