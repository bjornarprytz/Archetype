namespace Archetype.Core;

/// <summary>
/// Thrown when a runtime invariant is violated during game execution (e.g.
/// an atom reference resolves to the wrong kind, or a required atom is absent).
/// This is distinct from <see cref="DefinitionException"/>, which fires at
/// authoring / load time.
/// </summary>
public sealed class EngineException : Exception
{
    /// <inheritdoc/>
    public EngineException(string message) : base(message) { }

    /// <inheritdoc/>
    public EngineException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a game definition is invalid (e.g. an unknown keyword name,
/// a type mismatch in an argument, or a circular keyword dependency).
/// Fires at <c>GameDefinitionBuilder.Build()</c> or <c>GameDefinitionLoader.FromJson()</c>
/// — never during runtime execution.
/// </summary>
public sealed class DefinitionException : Exception
{
    /// <inheritdoc/>
    public DefinitionException(string message) : base(message) { }

    /// <inheritdoc/>
    public DefinitionException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a session cannot be built due to a contract violation between the
/// <see cref="GameDefinition"/> and the session-time configuration supplied by the host
/// (e.g. LocalId collision between <c>InitManifest</c> and <c>HostManifest</c>, or
/// incompatible builder option combinations).
/// Fires at <c>GameSessionBuilder.Build()</c> — never during runtime execution.
/// </summary>
public sealed class SessionException : Exception
{
    /// <inheritdoc/>
    public SessionException(string message) : base(message) { }

    /// <inheritdoc/>
    public SessionException(string message, Exception inner) : base(message, inner) { }
}
