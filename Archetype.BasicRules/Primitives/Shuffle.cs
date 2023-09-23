using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class Shuffle : EffectPrimitiveDefinition
{
    public override string Name => "SHUFFLE";
    public override string ReminderText => "Shuffle target draw pile.";
    
    public override IReadOnlyList<TargetDescription> Targets { get; } = TargetHelpers.Required(
        "type:zone,subtype:drawpile"
    ).ToList();
    public override IEvent Resolve(IResolutionContext context, Effect effectInstance)
    {
        var zone = effectInstance.Targets.Deconstruct<IZone>();
        zone.Cards.Shuffle();
        return new ShuffleEvent(zone);
    }
}

public record ShuffleEvent(IZone Zone) : EventBase;