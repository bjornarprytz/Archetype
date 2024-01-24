using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.Design;

public interface IProtoCard
{
    public string Name { get; }
    public IProtoActionBlock ActionBlock { get; }
    public IReadOnlyDictionary<string, IProtoActionBlock> Abilities { get; } // Key is ability name
    
    public IReadOnlyDictionary<string, int> Stats { get; } // Key is stat keyword
    public IReadOnlyDictionary<string, string> Tags { get; } // Key is tag keyword
}