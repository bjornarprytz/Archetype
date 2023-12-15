using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

[EffectSyntax("CREATE_CARD", typeof(OperandDeclaration<string, IZone>))]
public class CreateCard : EffectPrimitiveDefinition
{
    protected override OperandDeclaration<string, IZone> OperandDeclaration { get; } = new();

    public override IEvent Resolve(IResolutionContext context, EffectPayload effectPayload)
    {
        var (cardName, zone) = OperandDeclaration.Unpack(effectPayload);

        if (context.MetaGameState.ProtoData.GetProtoCard(cardName) is not { } protoCard)
            throw new InvalidOperationException($"No card with name {cardName} exists.");
        
        var card = new Card(protoCard)
        {
            CurrentZone = zone
        };
        zone.Add(card);
        context.GameState.AddAtom(card);

        return new CreateCardEvent(effectPayload.Source, card, zone);
    }
}

public record CreateCardEvent(IAtom Source, ICard Card, IZone Zone) : EventBase(Source);