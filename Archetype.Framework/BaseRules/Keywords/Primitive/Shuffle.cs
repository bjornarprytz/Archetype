using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

[EffectKeyword("SHUFFLE", typeof(OperandDeclaration<IOrderedZone>))]
public class Shuffle : EffectPrimitiveDefinition
{
    public override string ReminderText => "Shuffle target draw pile.";

    protected override OperandDeclaration<IOrderedZone> OperandDeclaration { get; } = new();


    public override IEvent Resolve(IResolutionContext context, EffectPayload effectPayload)
    {
        var zone = OperandDeclaration.Unpack(effectPayload);
        zone.Shuffle();
        return new ShuffleEvent(effectPayload.Source, zone);
    }
}

public record ShuffleEvent(IAtom Source, IOrderedZone Zone) : EventBase(Source);