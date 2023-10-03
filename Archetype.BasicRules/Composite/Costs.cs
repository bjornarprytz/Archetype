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

    public override IEnumerable<EffectPayload> Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var definition = context.MetaGameState.Definitions.GetOrThrow<Tap>();
        
        var requiredAmount = OperandDeclaration.UnpackOperands(effectPayload);
        var cards = context.Payments[Type];
        
        return Declare.CompositeKeyword(
            Name,
            effectPayload.Targets,
            effectPayload.Operands,
            
            )
    }

    public override CompositeKeywordInstance Compose(IEnumerable<KeywordOperand> operands, IEnumerable<KeywordTarget> targets, IDefinitions definitions)
    {
        var operandList = operands.ToList();
        var targetList = targets.ToList();
        
        var tapDefinition = definitions.GetOrThrow<Tap>();

        return 
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