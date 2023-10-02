using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public abstract class ChangeState<TAtom, T> : EffectPrimitiveDefinition
    where TAtom : IAtom
{
    protected override TargetDeclaration<TAtom> TargetDeclaration { get; } = new();
    
    protected abstract string Property { get; }
    protected abstract T Value { get; }
    

    public override IEvent Resolve(IResolutionContext context, EffectPayload effectPayload)
    {
        var atom = TargetDeclaration.UnpackTargets(effectPayload);

        if (atom.GetState<T>(Property) is { } existingValue && existingValue.Equals(Value))
            return new NonEvent();
         
        atom.State[Property] = Value!;
        
        return new ChangeStateEvent<T>(atom, Property, Value);
    }
}

public record ChangeStateEvent<T>(IAtom Atom, string Property, T Value) : EventBase;