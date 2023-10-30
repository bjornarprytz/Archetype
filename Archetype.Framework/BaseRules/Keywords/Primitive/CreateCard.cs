using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class CreateCard : EffectPrimitiveDefinition
{
    public override string Name => "CREATE_CARD";
    public override string ReminderText => "Create a card and place it in a zone.";

    protected override OperandDeclaration<string> OperandDeclaration { get; } = new();
    protected override TargetDeclaration<IZone> TargetDeclaration { get; } = new();

    public override IEvent Resolve(IResolutionContext context, EffectPayload effectPayload)
    {
        var cardName = OperandDeclaration.UnpackOperands(effectPayload);
        
        var zone = TargetDeclaration.UnpackTargets(effectPayload);

        if (context.MetaGameState.ProtoCards.GetProtoCard(cardName) is not { } protoCard)
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