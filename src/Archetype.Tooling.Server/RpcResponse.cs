using System.Text.Json;
using System.Text.Json.Serialization;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  JSON-RPC response types (D26)
// ---------------------------------------------------------------------------

/// <summary>
/// A JSON-RPC response written to stdout.  Exactly one of
/// <see cref="Result"/> or <see cref="Error"/> is non-null.
/// </summary>
/// <param name="Id">Correlation ID from the originating request.</param>
/// <param name="Result">Successful result payload; null when <see cref="Error"/> is set.</param>
/// <param name="Error">Error descriptor; null when <see cref="Result"/> is set.</param>
public sealed record RpcResponse(
    [property: JsonPropertyName("id")]     string Id,
    [property: JsonPropertyName("result")] object? Result = null,
    [property: JsonPropertyName("error")]  RpcError? Error = null)
{
    /// <summary>Constructs a success response.</summary>
    public static RpcResponse Ok(string id, object result) => new(id, Result: result);

    /// <summary>Constructs an error response.</summary>
    public static RpcResponse Fail(string id, string message, int code = -1) =>
        new(id, Error: new RpcError(code, message));
}

/// <summary>Error descriptor included in a failed <see cref="RpcResponse"/>.</summary>
public sealed record RpcError(
    [property: JsonPropertyName("code")]    int    Code,
    [property: JsonPropertyName("message")] string Message);
