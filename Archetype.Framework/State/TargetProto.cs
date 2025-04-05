using Jsonata.Net.Native;

namespace Archetype.Framework.Core;

/// <summary>
/// Describes the allowed targets for the effect.
/// </summary>
public record TargetProto
{
    public required IEnumerable<JsonataQuery> Predicates { get; init; }
}