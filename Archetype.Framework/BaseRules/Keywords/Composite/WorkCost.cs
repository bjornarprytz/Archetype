﻿using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.Actions;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class WorkCost : CostDefinition
{
    public override CostType Type => CostType.Work;

    public override string Name => "WORK_COST";
    public override string ReminderText => "Pay a work cost by tapping cards.";

    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();

    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var tapDefinition = context.MetaGameState.Rules.GetOrThrow<Tap>();
        var cards = context.Payments[Type];

        return new KeywordFrame(
            new WorkCostEvent(effectPayload.Source, cards.Payment.ToList()),
            cards.Payment.Select(c =>
                tapDefinition.CreateInstance(Declare.Operands(), Declare.Targets(Declare.Target(c)))).ToList()
        );
    }

    public override bool Check(IResolutionContext context, PaymentPayload paymentPayload, IKeywordInstance keywordInstance)
    {
        if (paymentPayload.Type != Type)
            throw new InvalidOperationException($"Cost type ({Type}) does not match payment type ({paymentPayload.Type})");
        
        var requiredAmount = OperandDeclaration.UnpackOperands(keywordInstance);
        return requiredAmount >= paymentPayload.Payment.Count && paymentPayload.Payment.All(c => c.GetState<bool>("TAPPED") == false);
    }
}

public record WorkCostEvent(IAtom Source, IReadOnlyList<IAtom> Payments) : EventBase(Source);