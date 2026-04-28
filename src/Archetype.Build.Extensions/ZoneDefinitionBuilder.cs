using Archetype.Core;

namespace Archetype.Build.Extensions;

/// <summary>
/// Fluent builder for <see cref="ZoneDefinition"/> instances.
/// <example>
/// <code>
/// var zone = new ZoneDefinitionBuilder("hand")
///     .WithStaticProperty("visible", true)
///     .WithStateField("card-limit", StateFieldType.Number)
///     .Build();
/// </code>
/// </example>
/// </summary>
public sealed class ZoneDefinitionBuilder
{
    private readonly string _name;
    private readonly Dictionary<string, object> _staticProperties = new();
    private List<StateFieldDecl>? _stateMap;

    /// <summary>Initialises a builder for a zone with the given <paramref name="name"/>.</summary>
    public ZoneDefinitionBuilder(string name) => _name = name;

    /// <summary>Adds or overwrites a static property on this zone definition.</summary>
    public ZoneDefinitionBuilder WithStaticProperty(string key, object value)
    {
        _staticProperties[key] = value;
        return this;
    }

    /// <summary>Replaces the entire state map declaration list.</summary>
    public ZoneDefinitionBuilder WithStateMap(IReadOnlyList<StateFieldDecl> declarations)
    {
        _stateMap = declarations.ToList();
        return this;
    }

    /// <summary>Appends a single named state field to this zone's state map.</summary>
    public ZoneDefinitionBuilder WithStateField(string name, StateFieldType type, string? textTemplate = null)
    {
        _stateMap ??= new List<StateFieldDecl>();
        _stateMap.Add(new StateFieldDecl(name, type, textTemplate));
        return this;
    }

    /// <summary>Builds and returns the <see cref="ZoneDefinition"/>.</summary>
    public ZoneDefinition Build() => new(
        Name:                 _name,
        StaticProperties:     _staticProperties,
        StateMapDeclarations: _stateMap);
}
