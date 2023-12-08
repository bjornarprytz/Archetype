using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;

namespace Archetype.Framework.Design;

public interface IProtoData
{
    IProtoCard? GetProtoCard(string name);
    IReadOnlyList<IPhase>? TurnSequence { get; }
}

public interface IProtoActionBlock
{
    public IReadOnlyList<CardTargetDescription> TargetSpecs { get; }
    public IReadOnlyList<IKeywordInstance> Conditions { get; }
    public IReadOnlyList<IKeywordInstance> Costs { get; }
    public IReadOnlyList<IKeywordInstance> Effects { get; }
    public IReadOnlyList<IKeywordInstance> AfterEffects { get; }
    public IReadOnlyList<IKeywordInstance> ComputedValues { get; }
}

public interface IProtoCard
{
    public string Name { get; }
    public IProtoActionBlock ActionBlock { get; }
    public IReadOnlyDictionary<string, IProtoActionBlock> Abilities { get; } // Key is ability name
    
    public IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } // Key is characteristic keyword
}

public interface IProtoSet
{
    public string Name { get; }
    public string Description { get; }
    public IReadOnlyList<IProtoCard> Cards { get; }
}
