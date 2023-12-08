using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.Design;

public interface IProtoActionBlock
{
    public IReadOnlyList<CardTargetDescription> TargetSpecs { get; }
    public IReadOnlyList<IKeywordInstance> Conditions { get; }
    public IReadOnlyList<IKeywordInstance> Costs { get; }
    public IReadOnlyList<IKeywordInstance> Effects { get; }
    public IReadOnlyList<IKeywordInstance> AfterEffects { get; }
    public IReadOnlyList<IKeywordInstance> ComputedValues { get; }
}