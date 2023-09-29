using Archetype.Framework.Definitions;

namespace Archetype.Framework.Proto;

public interface IProtoActionBlock
{
    public IReadOnlyList<CardTargetDescription> TargetSpecs { get; }
    public IReadOnlyList<KeywordInstance> Conditions { get; }
    public IReadOnlyList<KeywordInstance> Costs { get; }
    public IReadOnlyList<KeywordInstance> Effects { get; }
    public IReadOnlyList<KeywordInstance> ComputedValues { get; }
}

public interface IProtoCard
{
    public string Name { get; }
    public IProtoActionBlock ActionBlock { get; }
    public IReadOnlyDictionary<string, IProtoActionBlock> Abilities { get; } // Key is ability name
    
    public IReadOnlyDictionary<string, KeywordInstance> Characteristics { get; } // Key is characteristic keyword
}

public interface IProtoSet
{
    public string Name { get; }
    public string Description { get; }
    public IReadOnlyList<IProtoCard> Cards { get; }
}
