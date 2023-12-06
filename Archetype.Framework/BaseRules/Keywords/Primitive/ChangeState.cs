using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

public abstract class ChangeState<TAtom, T> : EffectPrimitiveDefinition
    where TAtom : IAtom
{
    protected override TargetDeclaration<TAtom> TargetDeclaration { get; } = new();
    
    protected abstract string Property { get; }
    protected abstract T ProduceValue(IResolutionContext context, EffectPayload effectPayload);
    

    public override IEvent Resolve(IResolutionContext context, EffectPayload effectPayload)
    {
        var atom = TargetDeclaration.UnpackTargets(effectPayload);

        var value = ProduceValue(context, effectPayload);

        if (atom.GetState<T>(Property) is { } existingValue && existingValue.Equals(value))
            return new NonEvent(effectPayload.Source);
         
        atom.State[Property] = value!;
        
        return new ChangeStateEvent<T>(effectPayload.Source, atom, Property, value);
    }
}

public record ChangeStateEvent<T>(IAtom Source, IAtom Atom, string Property, T Value) : EventBase(Source);