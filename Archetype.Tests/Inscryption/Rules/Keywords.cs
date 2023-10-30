using Archetype.BasicRules.Primitives;
using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;
using Archetype.Tests.Inscryption.Cards;

namespace Archetype.Tests.Rules.Inscryption;

public record GenericEvent(IAtom Souce) : EventBase(Souce);
public record GenericKeywordFrame(IAtom Source, IReadOnlyList<IKeywordInstance> Effects) : KeywordFrame(new GenericEvent(Source), Effects);

public class DrawCard : EffectCompositeDefinition
{
    public override string Name => "DRAW_CARD";
    public override string ReminderText => "Draw a card.";
    protected override TargetDeclaration<IDrawPile> TargetDeclaration { get; } = new();
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var deck = TargetDeclaration.UnpackTargets(effectPayload);
        var changeZoneDefinition = context.MetaGameState.Rules.GetOrThrow<ChangeZone>();

        var topCard = deck.PeekTop();
        
        IAtom GetHand(IResolutionContext ctx) => ctx.GameState.Zones.Values.OfType<IHand>().Single();
        
        var changeZone = changeZoneDefinition.CreateInstance(
            Declare.Operands(), 
            Declare.Targets(Declare.Target(topCard), Declare.Target(GetHand)));

        return
            new GenericKeywordFrame(
                    effectPayload.Source,
                Declare.KeywordInstances(changeZone)
            );

    }
}


public class AttackLeshy : EffectCompositeDefinition
{
    public override string Name => "ATTACK_LESHY";
    public override string ReminderText => "Attack with a unit.";
    protected override TargetDeclaration<ILane> TargetDeclaration { get; } = new();
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var lane = TargetDeclaration.UnpackTargets(effectPayload);
        
        var attacker = lane.HomeCritter;
        var defender = lane.AwayCritter;
        var stagedCreature = lane.StagingCritter;

        if (attacker is null) return new GenericKeywordFrame(
            effectPayload.Source,Declare.KeywordInstances());
        
        var power = attacker.GetCharacteristicValue("POWER", context);

        if (defender is null)
        {
             var tipScalesDefinition = context.MetaGameState.Rules.GetOrThrow<TipScales>();
                var tipScales = tipScalesDefinition.CreateInstance(
                    Declare.Operands(Declare.Operand(power)),
                    Declare.Targets());

                return new GenericKeywordFrame(
                    effectPayload.Source,Declare.KeywordInstances(tipScales));
        }
        

        var damageDefinition = context.MetaGameState.Rules.GetOrThrow<Damage>();
        var damage = damageDefinition.CreateInstance(
            Declare.Operands(Declare.Operand(power)),
            Declare.Targets(Declare.Target(defender)));
        
        var trampleAmount = power - defender.GetCharacteristicValue("HEALTH", context);

        if (stagedCreature is null || trampleAmount <= 0) return new GenericKeywordFrame(
            effectPayload.Source,Declare.KeywordInstances(damage));
        
        var trampleDamage = damageDefinition.CreateInstance(
            Declare.Operands(Declare.Operand(trampleAmount)), 
            Declare.Targets(Declare.Target(stagedCreature)));
            
        return new GenericKeywordFrame(
            effectPayload.Source,Declare.KeywordInstances(damage, trampleDamage));
    }
}

public class AttackPlayer : EffectCompositeDefinition
{
    public override string Name => "ATTACK_PLAYER";
    public override string ReminderText => "Attack with a unit.";
    protected override TargetDeclaration<ILane> TargetDeclaration { get; } = new();
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var lane = TargetDeclaration.UnpackTargets(effectPayload);
        
        var attacker = lane.HomeCritter;
        var defender = lane.AwayCritter;

        if (attacker is null) return new GenericKeywordFrame(
            effectPayload.Source,Declare.KeywordInstances());

        var power = attacker.GetCharacteristicValue("POWER", context);

        if (defender is null)
        {
             var tipScalesDefinition = context.MetaGameState.Rules.GetOrThrow<TipScales>();
                var tipScales = tipScalesDefinition.CreateInstance(
                    Declare.Operands(Declare.Operand( -power )), // Minus because
                    Declare.Targets());
            
            return new GenericKeywordFrame(
                effectPayload.Source,Declare.KeywordInstances(tipScales));
        }
        

        var damageDefinition = context.MetaGameState.Rules.GetOrThrow<Damage>();
        var damage = damageDefinition.CreateInstance(
            Declare.Operands(Declare.Operand(power)),
            Declare.Targets(Declare.Target(defender)));

        return new GenericKeywordFrame(
            effectPayload.Source,Declare.KeywordInstances(damage));
    }
}

public class TipScales : EffectPrimitiveDefinition
{
    public override string Name => "TIP_SCALES";
    public override string ReminderText => "Tip the scales.";
    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();

    public override IEvent Resolve(IResolutionContext context, EffectPayload effectPayload)
    {
        var amount = OperandDeclaration.UnpackOperands(effectPayload);
        
        var player = context.GameState.Player as IInscryptionPlayer;
        
        if (amount > 0)
        {
            player!.MyTeeth += amount;
        }
        else
        {
            player!.TheirTeeth -= amount;
        }

        return new TipScalesEvent(effectPayload.Source, amount);
    }
}
public record TipScalesEvent(IAtom Source, int Amount) : EventBase(Source);

public class Damage : ChangeState<ICard, int>
{
    public override string Name => "DAMAGE";
    public override string ReminderText => "Deal damage to a unit.";
    protected override string Property => "DAMAGE_TAKEN";
    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();
    protected override int ProduceValue(IResolutionContext context, EffectPayload effectPayload)
    {
        return OperandDeclaration.UnpackOperands(effectPayload);
    }
}

public class Power : CharacteristicDefinition
{
    public override string Name => "POWER";
    public override string ReminderText => "Damage in combat.";
}

public class Health : CharacteristicDefinition
{
    public override string Name => "HEALTH";
    public override string ReminderText => "Toughness in combat.";
}

public class Blood : CharacteristicDefinition
{
    public override string Name => "BLOOD";
    public override string ReminderText => "Sacrifice cost.";
}

public class Tribe : CharacteristicDefinition
{
    public override string Name => "TRIBE";
    public override string ReminderText => "Tribe.";
}

public class Flying : CharacteristicDefinition
{
    public override string Name => "SIGIL_FLYING";
    public override string ReminderText => "Flying.";
}

public class Reach : CharacteristicDefinition
{
    public override string Name => "SIGIL_REACH";
    public override string ReminderText => "Reach.";
}

public class Spikes : CharacteristicDefinition
{
    public override string Name => "SIGIL_SPIKES";
    public override string ReminderText => "Spikes.";
}

public class MoveSideways : CharacteristicDefinition
{
    public override string Name => "SIGIL_MOVE_SIDEWAYS";
    public override string ReminderText => "Move sideways.";
}

public class Lane : CharacteristicDefinition
{
    public override string Name => "LANE";
    public override string ReminderText => "Lane.";
}

public class MoveSidewaysResolver : EffectCompositeDefinition
{
    public override string Name => "MOVE_SIDEWAYS";
    public override string ReminderText => "Move sideways.";
    protected override OperandDeclaration<string> OperandDeclaration { get; } = new();
    protected override TargetDeclaration<ICard, ILane, ILane> TargetDeclaration { get; } = new();
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var alignment = OperandDeclaration.UnpackOperands(effectPayload);
        var (critter, primary, secondary) = TargetDeclaration.UnpackTargets(effectPayload);

        var enemy = alignment.Equals("Leshy", StringComparison.InvariantCultureIgnoreCase);
        
        var changeZoneDefinition = context.MetaGameState.Rules.GetOrThrow<ChangeZone>();
        
        Func<ILane, bool> zoneChecker = enemy ? z => z.AwayCritter == null : z => z.HomeCritter == null;
        
        var keywords = zoneChecker(primary) 
            ? Declare.KeywordInstances(changeZoneDefinition.CreateInstance(Declare.Operands(), Declare.Targets(Declare.Target(critter), Declare.Target(primary))))
            : zoneChecker(secondary)
            ? Declare.KeywordInstances(changeZoneDefinition.CreateInstance(Declare.Operands(), Declare.Targets(Declare.Target(critter), Declare.Target(secondary))))
            : Declare.KeywordInstances();
        
        return new GenericKeywordFrame(
            effectPayload.Source, keywords);
    }
}

public class ChangeBones : ChangeState<IInscryptionPlayer, int>
{
    public override string Name => "CHANGE_BONES";
    public override string ReminderText => "Change bones.";
    protected override string Property => "BONES";
    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();
    protected override int ProduceValue(IResolutionContext context, EffectPayload effectPayload)
    {
        return OperandDeclaration.UnpackOperands(effectPayload);
    }
}

public class BloodCost : CostDefinition
{
    public override CostType Type => CostType.Sacrifice;
    public override string Name => "BLOOD_COST";
    public override string ReminderText => "Pay a blood cost by sacrificing critters.";
    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var critters = context.Payments[Type].Payment;
        
        var exile = context.GameState.Zones.Values.OfType<IExile>().Single();
        var changeZoneDefinition = context.MetaGameState.Rules.GetOrThrow<ChangeZone>();
        
        var changeZones = critters.Select(c => changeZoneDefinition.CreateInstance(
            Declare.Operands(), 
            Declare.Targets(Declare.Target(c), Declare.Target(exile))));
        
        return new GenericKeywordFrame(
            effectPayload.Source,changeZones.ToList());
    }

    public override bool Check(IResolutionContext context, PaymentPayload paymentPayload, IKeywordInstance keywordInstance)
    {
        if (paymentPayload.Type != Type)
            return false;
        
        var requiredAmount = OperandDeclaration.UnpackOperands(keywordInstance);

        return requiredAmount >= paymentPayload.Payment.Select(a => a.GetCharacteristicValue("BLOOD", context)).Sum(); // Coins are atoms
    }
}

public class BonesCost : CostDefinition
{
    public override CostType Type => CostType.Coins;
    public override string Name => "BONES_COST";
    public override string ReminderText => "Pay a bones cost.";
    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();

    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var bonesAmount = context.Payments[Type].Amount;
        
        var changeBonesDefinition = context.MetaGameState.Rules.GetOrThrow<ChangeBones>();
        
        var changeBones = changeBonesDefinition.CreateInstance(
            Declare.Operands(Declare.Operand(-bonesAmount)), 
            Declare.Targets());
        
        return new GenericKeywordFrame(
            effectPayload.Source,Declare.KeywordInstances(changeBones));
    }

    public override bool Check(IResolutionContext context, PaymentPayload paymentPayload, IKeywordInstance keywordInstance)
    {
        if (paymentPayload.Type != Type)
            return false;
        
        var requiredAmount = OperandDeclaration.UnpackOperands(keywordInstance);
        
        var player = context.GameState.Player;
        
        var playerBones = player.GetState<int>("BONES");
        
        return requiredAmount >= playerBones;
    }
}

public class CheckVictory : EffectPrimitiveDefinition
{
    public override string Name => "CHECK_VICTORY";
    public override string ReminderText => "Check for victory.";
    public override IEvent Resolve(IResolutionContext context, EffectPayload effectPayload)
    {
        var player = context.GameState.Player as IInscryptionPlayer;

        var difference = int.Abs(player!.TheirTeeth - player.MyTeeth); 
        
        if (difference >= 5)
        {
            return new GameOverEvent(effectPayload.Source, player.MyTeeth > player.TheirTeeth);
        }
        
        return new NonEvent(effectPayload.Source);
    }
}
public record GameOverEvent(IAtom Source, bool Victory) : EventBase(Source);

public class ExileDeadThings : EffectCompositeDefinition
{
    public override string Name => "EXILE_DEAD_THINGS";
    public override string ReminderText => "Exile dead things.";
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var deadCritters = context.GameState.Atoms.Values.OfType<ICard>().Where(c => c.GetState<int>("DAMAGE_TAKEN") >= c.GetCharacteristicValue("HEALTH", context)).ToList();

        var exile = context.GameState.Zones.Values.OfType<IExile>().Single();
        
        var changeZoneDefinition = context.MetaGameState.Rules.GetOrThrow<ChangeZone>();
        
        var changeZones = deadCritters.Select(c => changeZoneDefinition.CreateInstance(
            Declare.Operands(), 
            Declare.Targets(Declare.Target(c), Declare.Target(exile))));
        
        return new GenericKeywordFrame(
            effectPayload.Source,changeZones.ToList());
    }
}