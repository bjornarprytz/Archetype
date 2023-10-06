using Archetype.BasicRules.Primitives;
using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Tests.Rules.Inscryption;

public class UpkeepPhase : Phase
{
    public UpkeepPhase()
    {
        Steps = new[] { new RefreshStep(this) };
    }
    
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = new Dictionary<string, IKeywordInstance>();
    public override IReadOnlyList<IStep> Steps { get; } 
    public override IReadOnlyList<ActionDescription> AllowedActions { get; } = Array.Empty<ActionDescription>();
}

public class RefreshStep : Step
{
    public RefreshStep(IPhase source) : base(source)
    {
        Effects = Declare.KeywordInstances(Declare.KeywordInstance("DRAW_CARD_STEP"));
    }

    public override string Name => "REFRESH_STEP";
    public override IReadOnlyList<IKeywordInstance> Effects { get; }
}

public class MainPhase : Phase
{
    public MainPhase()
    {
        Steps = ArraySegment<IStep>.Empty;
    }
    
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = new Dictionary<string, IKeywordInstance>();
    public override IReadOnlyList<IStep> Steps { get; } 
    public override IReadOnlyList<ActionDescription> AllowedActions { get; } = new List<ActionDescription>()
    {
        new (ActionType.PlayCard),
        new (ActionType.PassTurn)
    };
}

public class CombatPhase : Phase
{
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = new Dictionary<string, IKeywordInstance>();
    public override IReadOnlyList<IStep> Steps { get; }
    public override IReadOnlyList<ActionDescription> AllowedActions { get; }
}




public class DrawCardStep : EffectCompositeDefinition
{
    public override string Name => "DRAW_CARD_STEP";
    public override string ReminderText => "Draw a card from either deck.";
    public override IReadOnlyList<IKeywordInstance> Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var promptDefinition = context.MetaGameState.Rules.GetOrThrow<Prompt>();
        var drawCardDefinition = context.MetaGameState.Rules.GetOrThrow<DrawCard>();

        IEnumerable<IAtom> GetDrawPiles(IResolutionContext ctx) => ctx.GameState.Zones.Values.OfType<IDrawPile>();

        var prompt = promptDefinition.CreateInstance(Declare.Operands(Declare.Operand(GetDrawPiles), Declare.Operand(1), Declare.Operand(1), Declare.Operand("Pick a deck to draw from")), Declare.Targets());
        var draw = drawCardDefinition.CreateInstance(Declare.Operands(),
            Declare.Targets(Declare.Target(ctx => ctx.PromptResponses[0][0])));

        return Declare.KeywordInstances(prompt, draw);
    }
}

public class PlayerCombatStep : EffectCompositeDefinition
{
    public override string Name => "PLAYER_COMBAT_STEP";
    public override string ReminderText => "The player attacks Leshy.";
    public override IReadOnlyList<IKeywordInstance> Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var lane1 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "1", context));
        var lane2 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "2", context));
        var lane3 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "3", context));
        var lane4 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "4", context));

        var attackLeshyDefinition = context.MetaGameState.Rules.GetOrThrow<AttackLeshy>();

        return new[] { lane1, lane2, lane3, lane4 }.Select(lane => 
            attackLeshyDefinition.CreateInstance(Declare.Operands(), Declare.Targets(Declare.Target(lane)))
            ).ToList();
    }
}

public class LeshyCombatStep : EffectCompositeDefinition
{
    public override string Name => "LESHY_COMBAT_STEP";
    public override string ReminderText => "Leshy attacks the player.";
    public override IReadOnlyList<IKeywordInstance> Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var lane1 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "1", context));
        var lane2 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "2", context));
        var lane3 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "3", context));
        var lane4 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "4", context));

        var attackLeshyDefinition = context.MetaGameState.Rules.GetOrThrow<AttackPlayer>();

        return new[] { lane1, lane2, lane3, lane4 }.Select(lane => 
            attackLeshyDefinition.CreateInstance(Declare.Operands(), Declare.Targets(Declare.Target(lane)))
        ).ToList();
    }
}

public class StateBasedEffects : EffectCompositeDefinition
{
    public override string Name => "STATE_BASED_EFFECTS";
    public override string ReminderText => "Check for state-based effects.";
    public override IReadOnlyList<IKeywordInstance> Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var exileDeadThings = context.MetaGameState.Rules.GetOrThrow<ExileDeadThings>().CreateInstance(Declare.Operands(), Declare.Targets());
        var checkVictoryDefinition = context.MetaGameState.Rules.GetOrThrow<CheckVictory>().CreateInstance(Declare.Operands(), Declare.Targets());
        
        return Declare.KeywordInstances(
            exileDeadThings,
            checkVictoryDefinition
            );
    }
}