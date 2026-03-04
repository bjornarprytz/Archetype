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
