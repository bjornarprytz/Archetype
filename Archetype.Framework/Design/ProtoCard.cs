using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.Design;

public interface IProtoCard
{
    public string Name { get; }
    public IProtoActionBlock ActionBlock { get; }
    public IReadOnlyDictionary<string, IProtoActionBlock> Abilities { get; } // Key is ability name
    
    public IReadOnlyDictionary<string, IKeywordInstance> Characteristics { get; } // Key is characteristic keyword
}