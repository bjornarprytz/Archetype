using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Interface.Actions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Prototype1.Keywords;

[CostSyntax("COST_RESOURCES", typeof(OperandDeclaration<int>))]
public class CostResources : CostDefinition
{
    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();

    public override CostType Type => CostType.Resource;
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var cards = context.Payments[Type].Payment.Cast<ICard>().ToList();
        
        var discardDefinition = context.MetaGameState.Rules.GetOrThrow<DiscardCard>();
            
        var effects = cards.Select(a => discardDefinition.CreateInstance(a.ToOperand()));

        return new KeywordFrame(
            new PayResourcesEvent(context.Source, cards),
            new List<IKeywordInstance>(effects)

        );
    }

    public override bool Check(IResolutionContext context, PaymentPayload paymentPayload, IKeywordInstance keywordInstance)
    {
        var cards = context.Payments[Type].Payment.Cast<ICard>().ToList();
            
        var cost = OperandDeclaration.Unpack(keywordInstance);
        
        var value = cards.Sum(c => c.GetCharacteristicValue("VALUE", context));
        
        return value >= cost;
    }
}

public record PayResourcesEvent(IAtom Source, IReadOnlyList<ICard> Payments) : EventBase(Source);