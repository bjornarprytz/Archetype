using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Prototype1.Keywords;

[EffectSyntax("DRAW_CARD")]
public class DrawCard : EffectCompositeDefinition
{
    public override IKeywordFrame Compose(IResolutionContext context, EffectPayload effectPayload)
    {
        var player = context.GameState.Player;
        var drawPile = player.Deck;
        var hand = player.Hand;

        if (drawPile.PeekTop() is not ICard card)
        {
            throw new InvalidOperationException("Cannot draw from an empty deck.");
        }
        
        var changeZoneDefinition = context.MetaGameState.Rules.GetOrThrow<ChangeZone>();

        var changeZoneInstance = changeZoneDefinition.CreateInstance(card.ToOperand(), hand.ToOperand());

        return new KeywordFrame
        (
            new DrawCardEvent(context.Source, card),
            new List<IKeywordInstance>
            {
                changeZoneInstance,
            }
        );
    }
}

public record DrawCardEvent(IAtom Source, ICard Card) : EventBase(Source);