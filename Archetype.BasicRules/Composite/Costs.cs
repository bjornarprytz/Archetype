using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Actions;

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

        var cards = payment.Payment;
        var requiredAmount = OperandDeclaration.UnpackOperands(keywordInstance);
        
        var tapDefinition = context.MetaGameState.Definitions.GetOrThrow<Tap>();

        return Declare.CompositeKeyword(
            Name,
            Declare.Targets(),
            Declare.Operands(Declare.Operand(requiredAmount)),
            cards.Select(c => Declare.KeywordInstance(tapDefinition.Name, Declare.Targets(Declare.Target(c)))).ToList()
        );
    }


    public override bool Check(PaymentPayload paymentPayload, IKeywordInstance keywordInstance)
    {
        if (paymentPayload.Type != Type)
            throw new InvalidOperationException($"Cost type ({Type}) does not match payment type ({paymentPayload.Type})");
        
        var requiredAmount = OperandDeclaration.UnpackOperands(keywordInstance);
        return requiredAmount >= paymentPayload.Payment.Count && paymentPayload.Payment.All(c => c.GetState<bool>("TAPPED") == false);

    }
}