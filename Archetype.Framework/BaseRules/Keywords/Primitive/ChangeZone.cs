using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

public class ChangeZone : EffectPrimitiveDefinition
{
    public override string Name => "CHANGE_ZONE";
    public override string ReminderText =>  "Change zone from the existing zone to the target zone.";

    protected override TargetDeclaration<IAtom, IZone> TargetDeclaration { get; } = new();

    public override IEvent Resolve(IResolutionContext context, EffectPayload payload)
    {
        var (atom, to) = TargetDeclaration.UnpackTargets(payload);
        var from = atom.CurrentZone;

        from?.Remove(atom);
        to.Add(atom);
        atom.CurrentZone = to;

        return new ChangeZoneEvent(payload.Source, atom, from, to);
    }
}
public record ChangeZoneEvent(IAtom Source, IAtom Atom, IZone? From, IZone To) : EventBase(Source);
