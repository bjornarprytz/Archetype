using System.Text.Json;
using System.Text.Json.Serialization;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  JSON-RPC wire types (D26)
// ---------------------------------------------------------------------------

/// <summary>
/// A single JSON-RPC request received from the Electron main process over stdin.
/// Each line from stdin is one complete JSON object deserialised to this type.
/// </summary>
public sealed record RpcRequest(
    /// <summary>Caller-assigned correlation ID; echoed in the response.</summary>
    [property: JsonPropertyName("id")] string Id,
    /// <summary>Method name to dispatch.</summary>
    [property: JsonPropertyName("method")] string Method,
    /// <summary>Method-specific parameters; may be null for no-parameter methods.</summary>
    [property: JsonPropertyName("params")] JsonElement? Params = null);
