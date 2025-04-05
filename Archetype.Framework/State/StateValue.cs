using Jsonata.Net.Native;

namespace Archetype.Framework.Core;

public record StateValue
{
    public required string Whence { get; init; } = string.Empty; // This is a string for now, but could be an enum once I know its bounds. Should indicate what to base the query on.
    public required JsonataQuery GetterQuery { get; init; }
}