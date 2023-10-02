using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class WorkCost : CostDefinition
{
    public override string Name => "WORK_COST";
    public override string ReminderText => "Pay a work cost by tapping cards.";

    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();
    public override ICompositeKeywordInstance CreateEffectSequence(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        if (context.Payments[Type] is not { } payment)
            throw new InvalidOperationException($"No payment found for cost type {Type}");
        
        var requiredAmount = OperandDeclaration.UnpackOperands(keywordInstance);
        var paymentValue = payment.Payment.Count;

        if (requiredAmount > paymentValue)
            throw new InvalidOperationException("Not enough payment to satisfy cost.");

        var tapDefinition = context.MetaGameState.Definitions.GetOrThrow<Tap>();

        return Declare.CompositeKeyword(
            tapDefinition.Name,
            // TODO: Finish this
            );
    }


    public override bool Check(PaymentPayload paymentPayload, KeywordInstance keywordInstance)
    {
        if (paymentPayload.Type != CostType.Resource)
            throw new InvalidOperationException($"Cost type ({paymentPayload.Type}) does not match payment type ({CostType.Resource})");
        
        var requiredAmount = OperandDeclaration.UnpackOperands(keywordInstance);
        
        return requiredAmount >= paymentPayload.Payment.Count && paymentPayload.Payment.All(c => c.GetState<bool>("TAPPED") == false);

    }
}