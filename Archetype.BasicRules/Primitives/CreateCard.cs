using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class CreateCard : EffectPrimitiveDefinition
{
    public override string Name => "CREATECARD";
    public override string ReminderText => "Create a card and place it in a zone.";

    public override IReadOnlyList<OperandDescription> Operands { get; } = OperandHelpers.Required(
        KeywordOperandType.String).ToList();

    public override IReadOnlyList<TargetDescription> Targets { get; } = TargetHelpers.Required(
        "type:zone"
    ).ToList();

    public override IEvent Resolve(IResolutionContext context, Effect effectInstance)
    {
        // TODO: Continue here: How do I get proto data in here? Should I expect in in the args?
        
        var name = effectInstance.Operands.Deconstruct<string>();
        var zone = effectInstance.Targets.Deconstruct<IZone>();

        var card = new Card(name, zone);
        zone.Cards.Add(card);

        return new CreateCardEvent(card, zone);
    }
}