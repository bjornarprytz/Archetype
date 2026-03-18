using Archetype.Core;

namespace Archetype.Build;

// ---------------------------------------------------------------------------
//  CostDefBuilder — fluent factory for CostDef instances (D20)
// ---------------------------------------------------------------------------

/// <summary>
/// Fluent builder for <see cref="Core.CostDef"/> instances.
/// <para>
/// Use <see cref="CostDefBuilder.WithBody"/> to start a new cost definition,
/// then chain parameter and text-template declarations before calling
/// <see cref="CostDefBuilder.Build"/>.
/// </para>
/// <example>
/// <code>
/// var energyCost = CostDefBuilder
///     .WithBody(new EffectBlockDef([
///         new EffectBlockStep("assert", [
///             Kw.AtLeast(Kw.GetState(Kw.Param("source"), Kw.Str("energy")), Kw.Num(2)),
///             new Literal((double)(int)OnFail.Panic),
///             new Literal((double)(int)NotifyFlag.Off),
///         ]),
///         new EffectBlockStep("modify-accumulator", [
///             Kw.Param("source"), Kw.Str("energy"), Kw.Num(-2),
///         ]),
///     ]))
///     .WithTemplate("Pay {energy}: 2 energy")
///     .Build();
/// </code>
/// </example>
/// </summary>
public sealed class CostDefBuilder
{
    private readonly EffectBlockDef _body;
    private readonly List<ParameterDecl> _params = new();
    private string? _template;

    private CostDefBuilder(EffectBlockDef body) => _body = body;

    /// <summary>Starts a new <see cref="CostDefBuilder"/> with the given body block.</summary>
    public static CostDefBuilder WithBody(EffectBlockDef body) => new(body);

    /// <summary>Adds a player-provided parameter declaration to this cost.</summary>
    public CostDefBuilder WithParameter(string name, TypeName type)
    {
        _params.Add(new ParameterDecl(name, type));
        return this;
    }

    /// <summary>
    /// Sets the localized text template for this cost.
    /// Supports <c>{paramName}</c> placeholders.
    /// </summary>
    public CostDefBuilder WithTemplate(string template)
    {
        _template = template;
        return this;
    }

    /// <summary>Builds and returns the <see cref="Core.CostDef"/>.</summary>
    public CostDef Build() => new(_body, _params.ToArray(), _template);
}
