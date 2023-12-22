using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;

namespace Archetype.Prototype1.Keywords;


[ConditionSyntax("COND_MAX_HAND_SIZE", typeof(OperandDeclaration<int>))]
public class ConditionHandSize : ConditionDefinition
{
    protected override OperandDeclaration<int> OperandDeclaration { get; } = new();
    
    public override bool Check(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        var cardsInHand = context.GameState.Player.Hand.Atoms.Count;
        var maxHandSize = OperandDeclaration.Unpack(keywordInstance);
        
        return cardsInHand <= maxHandSize;
    }
}