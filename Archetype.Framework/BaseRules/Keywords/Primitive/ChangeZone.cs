using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class ChangeZone : EffectPrimitiveDefinition
{
    public override string Name => "CHANGE_ZONE";
    public override string ReminderText =>  "Change zone from the existing zone to the target zone.";

    protected override TargetDeclaration<ICard, IZone> TargetDeclaration { get; } = new();

    public override IEvent Resolve(IResolutionContext context, EffectPayload payload)
    {
        var (card, to) = TargetDeclaration.UnpackTargets(payload);
        var from = card.CurrentZone;

        from?.Remove(card);
        to.Add(card);
        card.CurrentZone = to;

        return new ChangeZoneEvent(card, from, to);
    }
}
public record ChangeZoneEvent(ICard Card, IZone? From, IZone To) : EventBase;
