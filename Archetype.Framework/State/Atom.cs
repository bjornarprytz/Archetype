
using Json.Patch;
using Json.Pointer;
using Jsonata.Net.Native;

namespace Archetype.Framework.State;


public record AtomState
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public Dictionary<string, int> Stats { get; init; } = new();
    public Dictionary<string, string[]> Facets { get; init; } = new();
    public Dictionary<string, string> Labels { get; init; } = new();
    public Dictionary<string, Guid> Atoms { get; init; } = new();
    public Dictionary<string, List<Guid>> AtomGroups { get; init; } = new();
    public HashSet<string> Tags { get; init; } = new();
}

public record Atom
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public AtomState State { get; init; } = new();
    
    public Dictionary<string, List<Modifier>> Modifiers { get; } = new();
}

public record Modifier
{
    public required string Path { get; init; } = string.Empty;
    
}