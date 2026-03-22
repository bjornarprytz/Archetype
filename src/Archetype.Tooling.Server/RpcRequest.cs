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
/// <param name="Id">Caller-assigned correlation ID; echoed in the response.</param>
/// <param name="Method">Method name to dispatch.</param>
/// <param name="Params">Method-specific parameters; may be null for no-parameter methods.</param>
public sealed record RpcRequest(
    [property: JsonPropertyName("id")]     string Id,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("params")] JsonElement? Params = null);
