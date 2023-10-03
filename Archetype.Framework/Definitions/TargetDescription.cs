using System.Collections;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Definitions;

public record CardTargetDescription(Filter Filter, bool IsOptional);
public abstract record KeywordTargetDescription(Type Type, bool IsOptional);
public record KeywordTargetDescription<T>(bool IsOptional) : KeywordTargetDescription(typeof(T), IsOptional) 
    where T : IAtom;


public class TargetDeclaration : IReadOnlyList<KeywordTargetDescription>
{
    protected IReadOnlyList<KeywordTargetDescription> Targets { get; init; } = new List<KeywordTargetDescription>();
    
    public bool Validate(IReadOnlyList<KeywordTarget> targets) => targets.Count == Targets.Count(t => !t.IsOptional);
    
    public IEnumerator<KeywordTargetDescription> GetEnumerator()
    {
        return Targets.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Targets).GetEnumerator();
    }

    public int Count => Targets.Count;

    public KeywordTargetDescription this[int index] => Targets[index];
}

public class TargetDeclaration<T0> : TargetDeclaration
    where T0: IAtom
{
    public TargetDeclaration(bool isOptional = false)
    {
        Targets = new List<KeywordTargetDescription>
        {
            new KeywordTargetDescription<T0>(isOptional)
        };
    }

    public T0 UnpackTargets(EffectPayload effectPayload)
    {
        return effectPayload.Targets.Deconstruct<T0>();
    }
}

public class TargetDeclaration<T0, T1> : TargetDeclaration
    where T0: IAtom
    where T1: IAtom
{
    public TargetDeclaration(int nOptional = 0)
    {
        Targets = new List<KeywordTargetDescription>
        {
            new KeywordTargetDescription<T0>(nOptional >= 2),
            new KeywordTargetDescription<T1>(nOptional >= 1),
        };
    }
    
    public (T0, T1) UnpackTargets(EffectPayload effectPayload)
    {
        return effectPayload.Targets.Deconstruct<T0, T1>();
    }
}

public class TargetDeclaration<T0, T1, T2> : TargetDeclaration
    where T0: IAtom
    where T1: IAtom
    where T2: IAtom
{
    public TargetDeclaration(int nOptional = 0)
    {
        Targets = new List<KeywordTargetDescription>
        {
            new KeywordTargetDescription<T0>(nOptional >= 3),
            new KeywordTargetDescription<T1>(nOptional >= 2),
            new KeywordTargetDescription<T2>(nOptional >= 1),
        };
    }
    
    public (T0, T1, T2) UnpackTargets(EffectPayload effectPayload)
    {
        return effectPayload.Targets.Deconstruct<T0, T1, T2>();
    }
}

public class TargetDeclaration<T0, T1, T2, T3> : TargetDeclaration
    where T0: IAtom
    where T1: IAtom
    where T2: IAtom
    where T3: IAtom
{
    public TargetDeclaration(int nOptional = 0)
    {
        Targets = new List<KeywordTargetDescription>
        {
            new KeywordTargetDescription<T0>(nOptional >= 4),
            new KeywordTargetDescription<T1>(nOptional >= 3),
            new KeywordTargetDescription<T2>(nOptional >= 2),
            new KeywordTargetDescription<T3>(nOptional >= 1),
        };
    }
    
    public (T0, T1, T2, T3) UnpackTargets(EffectPayload effectPayload)
    {
        return effectPayload.Targets.Deconstruct<T0, T1, T2, T3>();
    }
}

