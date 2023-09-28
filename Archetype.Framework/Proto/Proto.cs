using Archetype.Framework.Definitions;

namespace Archetype.Framework.Proto;

public class ProtoAbility
{
    public string Name { get; set; }
    public IReadOnlyList<TargetDescription> Targets { get; set; }
    public IReadOnlyList<KeywordInstance> Conditions { get; set; }
    public IReadOnlyList<KeywordInstance> Costs { get; set; }
    public IReadOnlyList<KeywordInstance> Effects { get; set; }
    public IReadOnlyList<KeywordInstance> ComputedValues { get; set; }
}

public class ProtoCard
{
    public string Name { get; set; }
    public IReadOnlyList<TargetDescription> Targets { get; set; }
    public IReadOnlyList<KeywordInstance> Conditions { get; set; }
    public IReadOnlyList<KeywordInstance> Effects { get; set; }
    public IReadOnlyList<KeywordInstance> Costs { get; set; }
    public IReadOnlyList<KeywordInstance> ComputedValues { get; set; }
    public IReadOnlyDictionary<string, ProtoAbility> Abilities { get; set; } // Key is ability name
    
    public IReadOnlyDictionary<string, KeywordInstance> Characteristics { get; set; } // Key is characteristic keyword
}

public class ProtoSet
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IReadOnlyList<ProtoCard> Cards { get; set; }
}
