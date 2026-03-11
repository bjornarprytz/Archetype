using System.Text.Json;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  RpcServer — newline-delimited JSON-RPC loop (D26)
// ---------------------------------------------------------------------------

/// <summary>
/// Reads newline-delimited JSON requests from <see cref="TextReader"/> (stdin),
/// dispatches each to a registered handler, and writes JSON responses to
/// <see cref="TextWriter"/> (stdout).
/// <para>
/// Each line is a complete JSON object.  The loop runs until stdin is closed
/// (EOF) or the <see cref="CancellationToken"/> is cancelled.  Unknown
/// methods and malformed JSON produce error responses; the loop continues.
/// </para>
/// </summary>
public sealed class RpcServer
{
    private readonly Dictionary<string, Func<JsonElement?, Task<object>>>
        _handlers = new(StringComparer.Ordinal);

    private readonly TextReader  _input;
    private readonly TextWriter  _output;

    private static readonly JsonSerializerOptions _readOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerOptions _writeOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>Creates an <see cref="RpcServer"/> bound to the given streams.</summary>
    public RpcServer(TextReader input, TextWriter output)
    {
        _input  = input;
        _output = output;
    }

    // -----------------------------------------------------------------------
    //  Handler registration
    // -----------------------------------------------------------------------

    /// <summary>
    /// Registers an async handler for the given <paramref name="method"/> name.
    /// Handlers receive the raw <see cref="JsonElement"/> params (may be null)
    /// and return a result object that will be serialised as the response.
    /// </summary>
    public void Register(string method,
        Func<JsonElement?, Task<object>> handler) =>
        _handlers[method] = handler;

    /// <summary>Convenience overload for synchronous handlers.</summary>
    public void Register(string method, Func<JsonElement?, object> handler) =>
        Register(method, p => Task.FromResult(handler(p)));

    // -----------------------------------------------------------------------
    //  Main loop
    // -----------------------------------------------------------------------

    /// <summary>
    /// Runs the request/response loop until stdin closes or
    /// <paramref name="ct"/> is cancelled.
    /// </summary>
    public async Task RunAsync(CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            string? line;
            try
            {
                line = await _input.ReadLineAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (line is null) break; // stdin closed

            line = line.Trim();
            if (line.Length == 0) continue;

            var response = await HandleLineAsync(line);
            var json = JsonSerializer.Serialize(response, _writeOpts);
            await _output.WriteLineAsync(json);
            await _output.FlushAsync(ct);
        }
    }

    // -----------------------------------------------------------------------
    //  Request dispatch
    // -----------------------------------------------------------------------

    private async Task<RpcResponse> HandleLineAsync(string line)
    {
        // Attempt to deserialise the request envelope.
        RpcRequest? req;
        try
        {
            req = JsonSerializer.Deserialize<RpcRequest>(line, _readOpts);
        }
        catch (JsonException ex)
        {
            // Malformed JSON — we cannot echo an id, use "null".
            return RpcResponse.Fail("null", $"Malformed JSON: {ex.Message}");
        }

        if (req is null)
            return RpcResponse.Fail("null", "Null request.");

        if (!_handlers.TryGetValue(req.Method, out var handler))
            return RpcResponse.Fail(req.Id, $"Unknown method '{req.Method}'.");

        try
        {
            var result = await handler(req.Params);
            return RpcResponse.Ok(req.Id, result);
        }
        catch (Exception ex)
        {
            return RpcResponse.Fail(req.Id, $"Handler error: {ex.Message}");
        }
    }
}
