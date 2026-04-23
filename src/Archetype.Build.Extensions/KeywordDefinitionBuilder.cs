using Archetype.Core;

namespace Archetype.Build.Extensions;

/// <summary>
/// Fluent builder for <see cref="KeywordDefinition"/> instances.
/// <example>
/// <code>
/// var kw = new KeywordDefinitionBuilder("take-damage")
///     .WithParam("target", TypeName.Card)
///     .WithParam("amount", TypeName.Number)
///     .WithBody(Kw.ModifyAccumulator(Kw.Param("target"), Kw.Str("health"), Kw.Multiply(Kw.Param("amount"), Kw.Num(-1))))
///     .WithTextTemplate("{target} takes {amount} damage")
///     .Build();
/// </code>
/// </example>
/// </summary>
public sealed class KeywordDefinitionBuilder
{
    private readonly string _name;
    private readonly List<ParameterDecl> _parameters = new();
    private KeywordNode? _body;
    private string? _textTemplate;
    private string? _description;
    private TypeName _returnType = TypeName.Number;

    /// <summary>Initialises a builder for a keyword with the given <paramref name="name"/>.</summary>
    public KeywordDefinitionBuilder(string name) => _name = name;

    /// <summary>Appends a typed parameter to the keyword's parameter list.</summary>
    public KeywordDefinitionBuilder WithParam(string name, TypeName type)
    {
        _parameters.Add(new ParameterDecl(name, type));
        return this;
    }

    /// <summary>Appends a fully specified <see cref="ParameterDecl"/> to the parameter list.</summary>
    public KeywordDefinitionBuilder WithParam(ParameterDecl param)
    {
        _parameters.Add(param);
        return this;
    }

    /// <summary>Sets the expression tree that forms the keyword body.</summary>
    public KeywordDefinitionBuilder WithBody(KeywordNode body)
    {
        _body = body;
        return this;
    }

    /// <summary>
    /// Sets the optional text template for player-facing display.
    /// Supports <c>{paramName}</c> placeholders matching declared parameter names.
    /// </summary>
    public KeywordDefinitionBuilder WithTextTemplate(string template)
    {
        _textTemplate = template;
        return this;
    }

    /// <summary>
    /// Sets the human-readable description of what this keyword does.
    /// Defaults to the keyword name when not set.
    /// </summary>
    public KeywordDefinitionBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the return type of this keyword.
    /// Defaults to <see cref="TypeName.Number"/> when not set.
    /// </summary>
    public KeywordDefinitionBuilder WithReturnType(TypeName returnType)
    {
        _returnType = returnType;
        return this;
    }

    /// <summary>Builds and returns the <see cref="KeywordDefinition"/>.</summary>
    public KeywordDefinition Build() => new(
        Name:              _name,
        Parameters:        _parameters.ToArray(),
        ReturnType:        _returnType,
        Description:       _description ?? _name,
        Body:              _body,
        PrimitiveSentinel: null,
        TextTemplate:      _textTemplate);
}
