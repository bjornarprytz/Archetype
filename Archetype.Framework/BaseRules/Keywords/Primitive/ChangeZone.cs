using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

public class ChangeZone : EffectPrimitiveDefinition
{
    protected override OperandDeclaration<IAtom, IZone> OperandDeclaration { get; } = new();

    public override IEvent Resolve(IResolutionContext context, EffectPayload payload)
    {
        var (atom, to) = OperandDeclaration.Unpack(payload);
        var from = atom.CurrentZone;

        from?.Remove(atom);
        to.Add(atom);
        atom.CurrentZone = to;

        return new ChangeZoneEvent(payload.Source, atom, from, to);
    }
}
public record ChangeZoneEvent(IAtom Source, IAtom Atom, IZone? From, IZone To) : EventBase(Source);
