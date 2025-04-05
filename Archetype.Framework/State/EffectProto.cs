namespace Archetype.Framework.Core;

/// <summary>
/// Describes which keyword to use and what to pass as parameters.
/// </summary>
public record EffectProto
{
    public required string Keyword { get; init; }
    public required IEnumerable<StateValue> Parameters { get; init; }
}