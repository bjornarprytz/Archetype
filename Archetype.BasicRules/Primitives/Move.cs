using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class Move : EffectPrimitiveDefinition
{
    public override string Name => "MOVE";
    public override string ReminderText =>  "Move target card to target zone.";
    
    public override IReadOnlyList<TargetDescription> Targets { get; } = TargetHelpers.Required(
        "type:card",
        "type:zone"
    ).ToList();

    public override IEvent Resolve(IResolutionContext context, Effect payload)
    {
        var (card, to) = payload.Targets.Deconstruct<ICard, IZone>();
        var from = card.CurrentZone;

        from.Cards.Remove(card);
        to.Cards.Add(card);
        card.CurrentZone = to;

        return new MoveEvent(card, from, to);
    }
}
public record MoveEvent(ICard Card, IZone From, IZone To) : EventBase;
