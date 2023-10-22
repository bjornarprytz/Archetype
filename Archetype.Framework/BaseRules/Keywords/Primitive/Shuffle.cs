using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class Shuffle : EffectPrimitiveDefinition
{
    public override string Name => "SHUFFLE";
    public override string ReminderText => "Shuffle target draw pile.";

    protected override TargetDeclaration<IOrderedZone> TargetDeclaration { get; } = new();


    public override IEvent Resolve(IResolutionContext context, EffectPayload effectPayload)
    {
        var zone = TargetDeclaration.UnpackTargets(effectPayload);
        zone.Shuffle();
        return new ShuffleEvent(zone);
    }
}

public record ShuffleEvent(IOrderedZone Zone) : EventBase;