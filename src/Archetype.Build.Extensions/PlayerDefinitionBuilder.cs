using Archetype.Core;

namespace Archetype.Build.Extensions;

/// <summary>
/// Fluent builder for <see cref="PlayerDefinition"/> instances.
/// <example>
/// <code>
/// var player = new PlayerDefinitionBuilder()
///     .WithStaticProperty("name", "Player 1")
///     .WithStateField("health", StateFieldType.Number)
///     .WithStateField("mana", StateFieldType.Number)
///     .Build();
/// </code>
/// </example>
/// </summary>
public sealed class PlayerDefinitionBuilder
{
    private readonly Dictionary<string, object> _staticProperties = new();
    private List<StateFieldDecl>? _stateMap;

    /// <summary>Adds or overwrites a static property on this player definition.</summary>
    public PlayerDefinitionBuilder WithStaticProperty(string key, object value)
    {
        _staticProperties[key] = value;
        return this;
    }

    /// <summary>Replaces the entire state map declaration list.</summary>
    public PlayerDefinitionBuilder WithStateMap(IReadOnlyList<StateFieldDecl> declarations)
    {
        _stateMap = declarations.ToList();
        return this;
    }

    /// <summary>Appends a single named state field to this player's state map.</summary>
    public PlayerDefinitionBuilder WithStateField(string name, StateFieldType type, string? textTemplate = null)
    {
        _stateMap ??= new List<StateFieldDecl>();
        _stateMap.Add(new StateFieldDecl(name, type, textTemplate));
        return this;
    }

    /// <summary>Builds and returns the <see cref="PlayerDefinition"/>.</summary>
    public PlayerDefinition Build() => new(
        StaticProperties:     _staticProperties,
        StateMapDeclarations: _stateMap);
}
