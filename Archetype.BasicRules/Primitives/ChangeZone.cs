using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class ChangeZone : EffectPrimitiveDefinition
{
    public override string Name => "CHANGE_ZONE";
    public override string ReminderText =>  "Change zone from the existing zone to the target zone.";
    
    public override IReadOnlyList<TargetDescription> Targets { get; } = TargetHelpers.Required(
        "type:card",
        "type:zone"
    ).ToList();

    public override IEvent Resolve(IResolutionContext context, Effect payload)
    {
        var (card, to) = payload.Targets.Deconstruct<ICard, IZone>();
        var from = card.CurrentZone;

        from?.Cards.Remove(card);
        to.Cards.Add(card);
        card.CurrentZone = to;

        return new ChangeZoneEvent(card, from, to);
    }
}
public record ChangeZoneEvent(ICard Card, IZone? From, IZone To) : EventBase;
