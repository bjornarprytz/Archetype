using Archetype.BasicRules.Primitives;
using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.Tests.Rules.Inscryption;

public class DrawCard : EffectCompositeDefinition
{
    public override string Name => "DRAW_CARD";
    public override string ReminderText => "Draw a card.";
    protected override TargetDeclaration<IDrawPile> TargetDeclaration { get; } = new();
    public override IReadOnlyList<IKeywordInstance> Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var deck = TargetDeclaration.UnpackTargets(effectPayload);
        var changeZoneDefinition = context.MetaGameState.Rules.GetOrThrow<ChangeZone>();

        var topCard = deck.GetTopCard();
        
        IAtom GetHand(IResolutionContext ctx) => ctx.GameState.Zones.Values.OfType<IHand>().Single();
        
        var changeZone = changeZoneDefinition.CreateInstance(
            Declare.Operands(), 
            Declare.Targets(Declare.Target(topCard), Declare.Target(GetHand)));
        
        return Declare.KeywordInstances(changeZone);
    }
}


public class Attack : EffectCompositeDefinition
{
    public override string Name => "ATTACK";
    public override string ReminderText => "Attack with a unit.";
    protected override TargetDeclaration<ICard, ICard> TargetDeclaration { get; } = new();
    public override IReadOnlyList<IKeywordInstance> Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var (attacker, defender) = TargetDeclaration.UnpackTargets(effectPayload);
        

        var damageDefinition = context.MetaGameState.Rules.GetOrThrow<Damage>();
        var damage = damageDefinition.CreateInstance(
            Declare.Operands(Declare.Operand(attacker.GetCharacteristicValue("POWER", context))),
            Declare.Targets(Declare.Target(defender)));
        
        
        
        return Declare.KeywordInstances(damage);
    }
}

public class Damage : ChangeState<ICard, int>
{
    public override string Name => "DAMAGE";
    public override string ReminderText => "Deal damage to a unit.";
    protected override string Property => "HEALTH";
    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();
    protected override int ProduceValue(IResolutionContext context, EffectPayload effectPayload)
    {
        return - OperandDeclaration.UnpackOperands(effectPayload);
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

public class BloodCost : CostDefinition
{
    public override CostType Type => CostType.Sacrifice;
    public override string Name => "BLOOD_COST";
    public override string ReminderText => "Pay a blood cost by sacrificing critters.";
    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();
    public override IReadOnlyList<IKeywordInstance> Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var critters = context.Payments[Type].Payment;
        
        var exile = context.GameState.Zones.Values.OfType<IExile>().Single();
        var changeZoneDefinition = context.MetaGameState.Rules.GetOrThrow<ChangeZone>();
        
        var changeZones = critters.Select(c => changeZoneDefinition.CreateInstance(
            Declare.Operands(), 
            Declare.Targets(Declare.Target(c), Declare.Target(exile))));
        
        return changeZones.ToList();
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
    public override string ReminderText => "Pay a bones cost by discarding bones.";
    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();

    public override IReadOnlyList<IKeywordInstance> Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var bones = context.Payments[Type].Payment;
        
        var exile = context.GameState.Zones.Values.OfType<IExile>().Single();
        var changeZoneDefinition = context.MetaGameState.Rules.GetOrThrow<ChangeZone>();
        
        var changeZones = bones.Select(b => changeZoneDefinition.CreateInstance(
            Declare.Operands(), 
            Declare.Targets(Declare.Target(b), Declare.Target(exile))));
        
        return changeZones.ToList();
    }

    public override bool Check(IResolutionContext context, PaymentPayload paymentPayload, IKeywordInstance keywordInstance)
    {
        if (paymentPayload.Type != Type)
            return false;
        
        var requiredAmount = OperandDeclaration.UnpackOperands(keywordInstance);

        return requiredAmount >= paymentPayload.Payment.Count; // Coins are atoms
    }
}