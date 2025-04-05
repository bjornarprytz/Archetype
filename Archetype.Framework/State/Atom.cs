using Archetype.Framework.Core;
using Json.Patch;
using Json.Pointer;
using Jsonata.Net.Native;

namespace Archetype.Framework.State;

public record TriggeredEffect
{
    public required Trigger Trigger { get; init; }
    public required IReadOnlyList<JsonataQuery> TargetPickers { get; init; } // Get the target(s) for the action block
    public required ActionBlock ActionBlock { get; init; }
}

public record ActionBlock
{
    public List<TargetProto> Targets { get; init; } = new();
    public List<EffectProto> Effects { get; init; } = new();
}

public record Trigger
{
    public Trigger(JsonPointer JsonPointer, string? RequiredValue)
    {
        this.JsonPointer = JsonPointer;
        this.RequiredValue = RequiredValue;
        Patch = new(PatchOperation.Test(JsonPointer, RequiredValue ?? string.Empty));
    }

    private JsonPatch Patch { get; init; }
    public JsonPointer JsonPointer { get; init; }
    public string? RequiredValue { get; init; }

    public void Deconstruct(out JsonPointer JsonPointer, out string? RequiredValue)
    {
        JsonPointer = this.JsonPointer;
        RequiredValue = this.RequiredValue;
    }
}

public record Modifier
{
    public required Guid SourceId { get; init; }
    public required JsonPatch Patch { get; init; }
    public required Trigger? EndTrigger { get; init; }
}

public record AtomState
{
    public Dictionary<string, int> Stats { get; init; } = new();
    public Dictionary<string, string[]> Facets { get; init; } = new();
    public Dictionary<string, string> Labels { get; init; } = new();
    public Dictionary<string, Guid> Atoms { get; init; } = new();
    public Dictionary<string, HashSet<Guid>> AtomGroups { get; init; } = new();
    public HashSet<string> Tags { get; init; } = new();
}

public record Atom
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Type { get; init; }
    public AtomState BaseState { get; } = new();
    public AtomState ResolvedState { get; } = new();
    public List<TriggeredEffect> TriggeredEffects { get; } = new();
    public Dictionary<JsonPointer, Modifier> Modifiers { get; } = new();
}