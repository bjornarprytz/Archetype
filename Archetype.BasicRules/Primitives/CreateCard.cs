using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class CreateCard : EffectPrimitiveDefinition
{
    public override string Name => "CREATE_CARD";
    public override string ReminderText => "Create a card and place it in a zone.";

    public override IReadOnlyList<OperandDescription> Operands { get; } = OperandHelpers.Required(
        KeywordOperandType.String).ToList();

    public override IReadOnlyList<TargetDescription> Targets { get; } = TargetHelpers.Required(
        "type:zone"
    ).ToList();

    public override IEvent Resolve(IResolutionContext context, Effect effectInstance)
    {
        var protoCard = effectInstance.Operands.Deconstruct<ProtoCard>();
        var zone = effectInstance.Targets.Deconstruct<IZone>();

        var card = new Card(protoCard)
        {
            CurrentZone = zone
        };
        zone.Cards.Add(card);

        return new CreateCardEvent(card, zone);
    }
}

public record CreateCardEvent(ICard Card, IZone Zone) : EventBase;