using Archetype.BasicRules.Primitives;
using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;
using Archetype.Tests.Inscryption.Cards;

namespace Archetype.Tests.Rules.Inscryption;

public class UpkeepPhase : Phase
{
    public UpkeepPhase()
    {
        Steps = new[] { new RefreshStep(this) };
    }
    
    public override string Name => "UPKEEP_PHASE";
    
    
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
    public override string Name => "MAIN_PHASE";
    
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
    public CombatPhase()
    {
        Steps = new IStep[]
        {
            new PlayerCombatStep(this),
            new LeshyCombatStep(this)
        };
    }
    public override string Name => "COMBAT_PHASE";
    
    public override IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } = new Dictionary<string, IKeywordInstance>();
    public override IReadOnlyList<IStep> Steps { get; }
    public override IReadOnlyList<ActionDescription> AllowedActions { get; } = Array.Empty<ActionDescription>();
}

public class PlayerCombatStep : Step
{
    public PlayerCombatStep(IPhase source) : base(source)
    {
        Effects = Declare.KeywordInstances(Declare.KeywordInstance("PLAYER_COMBAT_STEP"), Declare.KeywordInstance("STATE_BASED_EFFECTS"));
    }

    public override string Name => "PLAYER_COMBAT_STEP";
    public override IReadOnlyList<IKeywordInstance> Effects { get; }
}

public class LeshyCombatStep : Step
{
    public LeshyCombatStep(IPhase source) : base(source)
    {
        Effects = Declare.KeywordInstances(Declare.KeywordInstance("LESHY_COMBAT_STEP"), Declare.KeywordInstance("STATE_BASED_EFFECTS"));
    }

    public override string Name => "LESHY_COMBAT_STEP";
    public override IReadOnlyList<IKeywordInstance> Effects { get; }
}


public class DrawCardStepResolver : EffectCompositeDefinition
{
    public override string Name => "DRAW_CARD_STEP";
    public override string ReminderText => "Draw a card from either deck.";
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var promptDefinition = context.MetaGameState.Rules.GetOrThrow<Prompt>();
        var drawCardDefinition = context.MetaGameState.Rules.GetOrThrow<DrawCard>();

        IEnumerable<IAtom> GetDrawPiles(IResolutionContext ctx) => ctx.GameState.Zones.Values.OfType<IDrawPile>();

        var prompt = promptDefinition.CreateInstance(Declare.Operands(Declare.Operand(GetDrawPiles), Declare.Operand(1), Declare.Operand(1), Declare.Operand("Pick a deck to draw from")), Declare.Targets());
        var draw = drawCardDefinition.CreateInstance(Declare.Operands(),
            Declare.Targets(Declare.Target(ctx => ctx.PromptResponses[prompt.Id][0])));

        return new GenericKeywordFrame(Declare.KeywordInstances(prompt, draw));
    }
}

public class PlayerCombatStepResolver : EffectCompositeDefinition
{
    public override string Name => "PLAYER_COMBAT_STEP";
    public override string ReminderText => "The player attacks Leshy.";
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var lane1 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "1", context));
        var lane2 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "2", context));
        var lane3 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "3", context));
        var lane4 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "4", context));

        var attackLeshyDefinition = context.MetaGameState.Rules.GetOrThrow<AttackLeshy>();

        return new GenericKeywordFrame( new[] { lane1, lane2, lane3, lane4 }.Select(lane => 
            attackLeshyDefinition.CreateInstance(Declare.Operands(), Declare.Targets(Declare.Target(lane)))
            ).ToList());
    }
}

public class PlayerSigilStepResolver : EffectCompositeDefinition
{
    public override string Name => "PLAYER_SIGIL_STEP";
    public override string ReminderText => "The player sigils trigger";
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var lane1 = context.GameState.Zones.Values.OfType<ILane>()
            .Single(l => l.HasCharacteristic("LANE", "1", context));
        var lane2 = context.GameState.Zones.Values.OfType<ILane>()
            .Single(l => l.HasCharacteristic("LANE", "2", context));
        var lane3 = context.GameState.Zones.Values.OfType<ILane>()
            .Single(l => l.HasCharacteristic("LANE", "3", context));
        var lane4 = context.GameState.Zones.Values.OfType<ILane>()
            .Single(l => l.HasCharacteristic("LANE", "4", context));
        
        var critter1 = lane1.HomeCritter;
        var critter2 = lane2.HomeCritter;
        var critter3 = lane3.HomeCritter;
        var critter4 = lane4.HomeCritter;

        var moveSidewaysDefinition = context.MetaGameState.Rules.GetOrThrow<MoveSidewaysResolver>();

        var lanes = new[] { lane1, lane2, lane3, lane4 }.ToList();
        var critters = new[] { critter1, critter2, critter3, critter4 }.ToList();

        if (critters.All(c => c != null))
            return new GenericKeywordFrame(Declare.KeywordInstances());

        var keywords = new List<IKeywordInstance>();
        
        for(var i=0; i < critters.Count; i++)
        {
            if (critters[i] is not { } critter ||
                !critter.HasCharacteristic("SIGIL_MOVE_SIDEWAYS", "any", context)) continue;
            
            var leftIndex = i > 0 ? i - 1 : i + 1;
            var rightIndex = i < lanes.Count - 1 ? i + 1 : i - 1;

            var primaryTargetLane = lanes[rightIndex];
            var secondaryTargetLane = lanes[leftIndex];
                
            keywords.Add(moveSidewaysDefinition.CreateInstance(Declare.Operands(Declare.Operand("Player")), Declare.Targets(Declare.Target(critter), Declare.Target(primaryTargetLane), Declare.Target(secondaryTargetLane))));
        }
        
        return new GenericKeywordFrame(keywords);
    }
}

public class LeshyCombatStepResolver : EffectCompositeDefinition
{
    public override string Name => "LESHY_COMBAT_STEP";
    public override string ReminderText => "Leshy attacks the player.";
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var lane1 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "1", context));
        var lane2 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "2", context));
        var lane3 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "3", context));
        var lane4 = context.GameState.Zones.Values.OfType<ILane>().Single(l => l.HasCharacteristic("LANE", "4", context));

        var attackLeshyDefinition = context.MetaGameState.Rules.GetOrThrow<AttackPlayer>();

        return new GenericKeywordFrame(new[] { lane1, lane2, lane3, lane4 }.Select(lane => 
            attackLeshyDefinition.CreateInstance(Declare.Operands(), Declare.Targets(Declare.Target(lane)))
        ).ToList());
    }
}

public class LeshySigilStepResolver : EffectCompositeDefinition
{
    public override string Name => "LESHY_SIGIL_STEP";
    public override string ReminderText => "Leshy sigils trigger.";
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var lane1 = context.GameState.Zones.Values.OfType<ILane>()
            .Single(l => l.HasCharacteristic("LANE", "1", context));
        var lane2 = context.GameState.Zones.Values.OfType<ILane>()
            .Single(l => l.HasCharacteristic("LANE", "2", context));
        var lane3 = context.GameState.Zones.Values.OfType<ILane>()
            .Single(l => l.HasCharacteristic("LANE", "3", context));
        var lane4 = context.GameState.Zones.Values.OfType<ILane>()
            .Single(l => l.HasCharacteristic("LANE", "4", context));
        
        var critter1 = lane1.AwayCritter;
        var critter2 = lane2.AwayCritter;
        var critter3 = lane3.AwayCritter;
        var critter4 = lane4.AwayCritter;

        var moveSidewaysDefinition = context.MetaGameState.Rules.GetOrThrow<MoveSidewaysResolver>();

        var lanes = new[] { lane1, lane2, lane3, lane4 }.ToList();
        var critters = new[] { critter1, critter2, critter3, critter4 }.ToList();

        if (critters.All(c => c != null))
            return new GenericKeywordFrame(Declare.KeywordInstances());

        var keywords = new List<IKeywordInstance>();
        
        for(var i=0; i < critters.Count; i++)
        {
            if (critters[i] is not { } critter ||
                !critter.HasCharacteristic("SIGIL_MOVE_SIDEWAYS", "any", context)) continue;
            
            var leftIndex = i > 0 ? i - 1 : i + 1;
            var rightIndex = i < lanes.Count - 1 ? i + 1 : i - 1;

            var primaryTargetLane = lanes[rightIndex];
            var secondaryTargetLane = lanes[leftIndex];
                
            keywords.Add(moveSidewaysDefinition.CreateInstance(Declare.Operands(Declare.Operand("Leshy")), Declare.Targets(Declare.Target(critter), Declare.Target(primaryTargetLane), Declare.Target(secondaryTargetLane))));
        }
        
        return new GenericKeywordFrame(keywords);
    }
}

public class StateBasedEffectsResolver : EffectCompositeDefinition
{
    public override string Name => "STATE_BASED_EFFECTS";
    public override string ReminderText => "Check for state-based effects.";
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var exileDeadThings = context.MetaGameState.Rules.GetOrThrow<ExileDeadThings>().CreateInstance(Declare.Operands(), Declare.Targets());
        var checkVictoryDefinition = context.MetaGameState.Rules.GetOrThrow<CheckVictory>().CreateInstance(Declare.Operands(), Declare.Targets());
        
        return new GenericKeywordFrame(Declare.KeywordInstances(
            exileDeadThings,
            checkVictoryDefinition
            ));
    }
}