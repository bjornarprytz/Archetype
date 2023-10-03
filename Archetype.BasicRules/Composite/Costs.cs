using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Actions;

namespace Archetype.BasicRules.Primitives;

public class WorkCost : CostDefinition
{
    public override CostType Type => CostType.Work;
    public override string Name => "WORK_COST";
    public override string ReminderText => "Pay a work cost by tapping cards.";

    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();

    public override IReadOnlyList<IKeywordInstance> Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var tapDefinition = context.MetaGameState.Definitions.GetOrThrow<Tap>();
        var cards = context.Payments[Type];

        return cards.Payment.Select(c =>
            tapDefinition.CreateInstance(Declare.Operands(), Declare.Targets(Declare.Target(c)))).ToList();
    }

    public override bool Check(PaymentPayload paymentPayload, IKeywordInstance keywordInstance)
    {
        if (paymentPayload.Type != Type)
            throw new InvalidOperationException($"Cost type ({Type}) does not match payment type ({paymentPayload.Type})");
        
        var requiredAmount = OperandDeclaration.UnpackOperands(keywordInstance);
        return requiredAmount >= paymentPayload.Payment.Count && paymentPayload.Payment.All(c => c.GetState<bool>("TAPPED") == false);

    }
}