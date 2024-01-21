using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Prototype1.Keywords;

[EffectSyntax("DISCARD_CARD", typeof(OperandDeclaration<ICard>))]
public class DiscardCard : EffectCompositeDefinition
{
    protected override OperandDeclaration<ICard> OperandDeclaration { get; } = new();
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var card = OperandDeclaration.Unpack(effectPayload);
        
        var player = context.GameState.Player;
        var discardPile = player.DiscardPile;
        
        var changeZoneDefinition = context.MetaGameState.Rules.GetOrThrow<ChangeZone>();

        var changeZoneInstance = changeZoneDefinition.CreateInstance(card.ToOperand(), discardPile.ToOperand());

        return new KeywordFrame(changeZoneInstance);
    }
}

public record DiscardCardEvent(IAtom Source, ICard Card) : EventBase(Source);