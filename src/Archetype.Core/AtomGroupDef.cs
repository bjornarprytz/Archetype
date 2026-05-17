using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Archetype.Core;

// ---------------------------------------------------------------------------
//  AtomGroup serialization types
// ---------------------------------------------------------------------------

[JsonDerivedType(typeof(MatcherByName), typeDiscriminator: "by-name")]
[JsonDerivedType(typeof(MatcherByStaticProperty), typeDiscriminator: "has-static-property")]
[JsonDerivedType(typeof(MatcherAll), typeDiscriminator: "all")]
public abstract partial record MatcherDef;

public sealed partial record MatcherByName(string Pattern, bool Regex = false) : MatcherDef;
public sealed partial record MatcherByStaticProperty(string PropertyName) : MatcherDef;
public sealed partial record MatcherAll() : MatcherDef;

[JsonDerivedType(typeof(TransformSetStaticPropertyIfMissing), typeDiscriminator: "set-static-property-if-missing")]
[JsonDerivedType(typeof(TransformAddStaticEffect), typeDiscriminator: "add-static-effect")]
[JsonDerivedType(typeof(TransformSetCostIfMissing), typeDiscriminator: "set-cost-if-missing")]
public abstract partial record TransformationDef;

public sealed partial record TransformSetStaticPropertyIfMissing(string PropertyName, object Value) : TransformationDef;
public sealed partial record TransformAddStaticEffect(StaticEffectDef Effect) : TransformationDef;
public sealed partial record TransformSetCostIfMissing(CostDef Cost) : TransformationDef;

/// <summary>
/// Serializable description of an AtomGroup: selects atoms and applies declarative
/// transformations.  Minimal shape used for JSON authoring and export.
/// </summary>
public sealed record AtomGroupDef(
    string Name,
    IReadOnlyList<AtomKind> Kinds,
    MatcherDef Matcher,
    IReadOnlyList<TransformationDef> Transformations,
    int Priority = 0,
    bool OverrideLocal = false,
    string ApplyPhase = "PreBuild");
