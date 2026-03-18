using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Archetype.Core;

namespace Archetype.Text;

// ---------------------------------------------------------------------------
//  TextRenderer — D11 text rendering pipeline
// ---------------------------------------------------------------------------

/// <summary>
/// Walks <see cref="KeywordNode"/> trees and <see cref="EffectBlockDef"/>s to
/// produce structured <see cref="RenderNode"/> trees for display by the host.
/// <para>
/// Stateless beyond an internal definition-time cache (see below). No
/// <c>GameState</c>, no <c>ExecutionContext</c>.  One instance is safe to use
/// for all locales and all cards for the lifetime of the game.
/// </para>
/// <para>
/// <b>Two render modes</b> (D11):
/// <list type="bullet">
///   <item><b>Definition-time</b> (<paramref name="bindings"/> == <c>null</c>):
///   <see cref="ParameterRef"/> nodes render as their declared parameter name
///   labels.  Results are cached per <c>(KeywordDefinition, localeStrings)</c>
///   pair.</item>
///   <item><b>Invocation-time</b> (<paramref name="bindings"/> != <c>null</c>):
///   <see cref="ParameterRef"/> nodes are substituted with their bound values.
///   Not cached.</item>
/// </list>
/// </para>
/// <para>
/// <b>Template resolution order</b> for any keyword invocation (D11):
/// <list type="number">
///   <item>Locale file entry keyed by keyword name.</item>
///   <item><see cref="KeywordDefinition.TextTemplate"/> on the definition.</item>
///   <item>Structural fallback: <c>keyword-name(arg, arg, …)</c>.</item>
/// </list>
/// In all cases <see cref="CompositeNode.Body"/> is always the full recursive
/// structural expansion; only <see cref="CompositeNode.Summary"/> is affected
/// by locale/template.
/// </para>
/// <para>
/// <b>Caching.</b> Definition-time body renders are cached per
/// <c>(KeywordDefinition, localeStrings)</c> using
/// <see cref="ConditionalWeakTable{TKey,TValue}"/> keyed by the locale
/// dictionary reference.  The host creates one dictionary object per locale and
/// reuses it; the same object always hits the same cache bucket.  A
/// <c>null</c> locale has its own separate flat cache.  Invocation-time renders
/// are never cached.
/// </para>
/// </summary>
public sealed class TextRenderer
{
    // Regex that splits a template string into alternating plain-text segments
    // and substitution/cross-reference tokens:
    //   {paramName}           — parameter substitution
    //   [display](key)        — long-form cross-reference (D18)
    //   [key]                 — short-form cross-reference (D18)
    // Capture group 1 captures the token text so Regex.Split returns it
    // interleaved with the plain-text segments.
    //
    // RegexOptions.Compiled is intentionally omitted: Reflection.Emit is not
    // available in Godot's WebAssembly export context (D1 WASM constraint).
    // Interpreted mode is sufficient for a template parser that is never on
    // the hot execution path.
    private static readonly Regex TemplateTokenRegex = new(
        @"(\{[\w-]+\}|\[[^\]]+\]\([^)]+\)|\[[^\]]+\])",
        RegexOptions.CultureInvariant);

    // Reserved engine locale keys and their built-in English defaults.
    private static readonly IReadOnlyDictionary<string, string> _engineDefaults =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["engine.lifetime.turn_timer"]     = "for {n} turn(s)",
            ["engine.lifetime.trigger_count"]  = "(up to {n} time(s))",
            ["engine.lifetime.while_condition"] = "while {expr}",
            ["engine.lifetime.or_separator"]   = " or ",
        };

    // -----------------------------------------------------------------------
    //  Definition-time body cache
    //
    //  Only composite keywords have a Body tree worth caching; primitives
    //  produce a structural TextSpan which is cheap to recompute.
    //
    //  Outer key: locale dictionary reference (ConditionalWeakTable provides
    //    identity-based lookup and allows the locale dict to be GC'd).
    //  Inner key: KeywordDefinition instance (by reference via
    //    ReferenceEqualityComparer).
    // -----------------------------------------------------------------------
    private readonly ConditionalWeakTable<
        IReadOnlyDictionary<string, string>,
        Dictionary<KeywordDefinition, RenderNode>> _localeBodyCaches = new();

    // Null-locale body cache: no locale, so no ConditionalWeakTable needed.
    private readonly Dictionary<KeywordDefinition, RenderNode> _nullLocaleBodyCache =
        new(ReferenceEqualityComparer.Instance);

    private readonly IReadOnlyDictionary<string, KeywordDefinition> _registry;

    /// <summary>
    /// Constructs a <see cref="TextRenderer"/> backed by the given keyword
    /// registry.  Pass <c>GameDefinition.Keywords</c> or
    /// <c>BuiltInKeywords.All.ToDictionary(k =&gt; k.Name)</c>.
    /// </summary>
    /// <param name="registry">
    /// Map of keyword name → <see cref="KeywordDefinition"/>.  Used to look up
    /// parameter declarations and <see cref="KeywordDefinition.TextTemplate"/>
    /// values for each <see cref="Invocation"/> encountered during traversal.
    /// </param>
    public TextRenderer(IReadOnlyDictionary<string, KeywordDefinition> registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    // -----------------------------------------------------------------------
    //  Public API (D11 + D18)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Renders a single <see cref="KeywordNode"/> to a <see cref="RenderNode"/>
    /// tree.
    /// <list type="bullet">
    ///   <item><see cref="ParameterRef"/> — definition-time: parameter label;
    ///   invocation-time: bound value.</item>
    ///   <item><see cref="Literal"/> — always the formatted value.</item>
    ///   <item><see cref="Invocation"/> — <see cref="CompositeNode"/> with
    ///   template-resolved summary and recursive body.</item>
    /// </list>
    /// </summary>
    /// <param name="node">The keyword expression node to render.</param>
    /// <param name="localeStrings">
    /// Optional locale string map.  <c>null</c> falls through to
    /// <see cref="KeywordDefinition.TextTemplate"/> and structural rendering.
    /// </param>
    /// <param name="bindings">
    /// Optional runtime bindings.  <c>null</c> = definition-time mode.
    /// </param>
    public RenderNode Render(
        KeywordNode node,
        IReadOnlyDictionary<string, string>? localeStrings,
        IReadOnlyDictionary<string, object>? bindings) =>
        node switch
        {
            ParameterRef p  => RenderParameterRef(p, bindings),
            Literal l       => new TextSpan(FormatValue(l.Value)),
            Invocation inv  => RenderInvocation(inv, localeStrings, bindings),
            // Defensive fallback — no other KeywordNode subtypes exist today.
            _               => new TextSpan(node.ToString() ?? string.Empty),
        };

    /// <summary>
    /// Renders an <see cref="EffectBlockDef"/> as a <see cref="SequenceNode"/>
    /// with one child per step (D11).  Always returns a
    /// <see cref="SequenceNode"/> — including for single-step blocks — so that
    /// callers can pattern-match or iterate uniformly without a special case.
    /// </summary>
    /// <param name="block">The effect block to render.</param>
    /// <param name="localeStrings">Optional locale string map.</param>
    /// <param name="bindings">Optional runtime bindings.</param>
    public RenderNode RenderBlock(
        EffectBlockDef block,
        IReadOnlyDictionary<string, string>? localeStrings,
        IReadOnlyDictionary<string, object>? bindings)
    {
        var items = block.Steps
            .Select(step => Render(
                // Treat each step as a synthetic Invocation so the standard
                // rendering pipeline handles template lookup and body building.
                new Invocation(step.KeywordName, step.ArgNodes),
                localeStrings,
                bindings))
            .ToList();

        // Always wrap in SequenceNode — even for a single step — so that hosts
        // can iterate steps uniformly without a special case (D11 API contract).
        return new SequenceNode(items);
    }

    /// <summary>
    /// Renders a <see cref="StaticEffectDef"/> as a <see cref="SequenceNode"/>
    /// containing: the rendered state-contribution block (if present), the
    /// rendered trigger (if present), and the rendered lifetime spec.
    /// For declarative static effects, this is the card's ability text.
    /// </summary>
    /// <param name="effect">The static effect definition to render.</param>
    /// <param name="localeStrings">Optional locale string map.</param>
    /// <param name="bindings">Optional runtime bindings.</param>
    public RenderNode RenderStaticEffect(
        StaticEffectDef effect,
        IReadOnlyDictionary<string, string>? localeStrings,
        IReadOnlyDictionary<string, object>? bindings)
    {
        var items = new List<RenderNode>();

        if (effect.StateContributionBlock is not null)
            items.Add(RenderBlock(effect.StateContributionBlock, localeStrings, bindings));

        if (effect.Trigger is not null)
        {
            // Render the trigger as: "when <event-keyword>: <fired block>"
            var triggerParts = new List<RenderNode>
            {
                new TextSpan($"when {effect.Trigger.EventKeyword}: "),
                RenderBlock(effect.Trigger.FiredBlock, localeStrings, bindings),
            };
            items.Add(new SequenceNode(triggerParts));
        }

        var lifetimeNode = RenderLifetimeSpec(effect.Lifetime, localeStrings);
        // Only include the lifetime node if it's non-empty (i.e. not permanent).
        if (lifetimeNode is not TextSpan { Text: "" })
            items.Add(lifetimeNode);

        return new SequenceNode(items);
    }

    /// <summary>
    /// Renders a <see cref="LifetimeSpec"/> using the engine's reserved locale
    /// keys (D11 lifetime table).  Multiple conditions are joined by the
    /// <c>engine.lifetime.or_separator</c> string.  A permanent lifetime
    /// (no conditions) renders as an empty <see cref="TextSpan"/>.
    /// </summary>
    /// <param name="spec">The lifetime spec to render.</param>
    /// <param name="localeStrings">Optional locale string map.</param>
    public RenderNode RenderLifetimeSpec(
        LifetimeSpec spec,
        IReadOnlyDictionary<string, string>? localeStrings)
    {
        if (spec.IsPermanent)
            return new TextSpan(string.Empty);

        string separator = ResolveEngineLocale(
            localeStrings, "engine.lifetime.or_separator");

        var parts = spec.Conditions.Select(cond => cond switch
        {
            TurnTimer t => ExpandSimpleTemplate(
                ResolveEngineLocale(localeStrings, "engine.lifetime.turn_timer"),
                ("n", t.Turns.ToString())),

            TriggerCount c => ExpandSimpleTemplate(
                ResolveEngineLocale(localeStrings, "engine.lifetime.trigger_count"),
                ("n", c.Count.ToString())),

            // Render the while-condition expression recursively, then flatten
            // its text into the template substitution.
            WhileCondition w => ExpandSimpleTemplate(
                ResolveEngineLocale(localeStrings, "engine.lifetime.while_condition"),
                ("expr", FlattenToText(Render(w.Expression, localeStrings, null)))),

            _ => new TextSpan(cond.ToString() ?? string.Empty),
        }).ToList();

        // Join multiple conditions with the separator.
        string joined = string.Join(separator, parts.Select(FlattenToText));
        return new TextSpan(joined);
    }

    /// <summary>
    /// Resolves a keyword definition to a <see cref="RenderNode"/> for
    /// rules-reference display (D18).
    /// <list type="bullet">
    ///   <item>Returns <c>null</c> if the keyword name is not in the
    ///   registry.</item>
    ///   <item>For primitives: renders with parameter-ref labels as arguments
    ///   (definition-time), using the registered <c>TextTemplate</c> or
    ///   structural fallback.</item>
    ///   <item>For composites: calls <see cref="Render"/> on the definition
    ///   body.</item>
    /// </list>
    /// </summary>
    /// <param name="keywordName">The keyword name to look up and render.</param>
    /// <param name="localeStrings">Optional locale string map.</param>
    /// <param name="bindings">Optional runtime bindings.</param>
    public RenderNode? Resolve(
        string keywordName,
        IReadOnlyDictionary<string, string>? localeStrings,
        IReadOnlyDictionary<string, object>? bindings = null)
    {
        if (!_registry.TryGetValue(keywordName, out var def))
            return null;

        if (def.IsPrimitive)
        {
            // Build a synthetic Invocation with each parameter rendered as
            // its own ParameterRef label, giving definition-time card text
            // for the primitive's signature.
            var paramArgs = def.Parameters
                .Select(p => (KeywordNode)new ParameterRef(p.Name))
                .ToArray();
            return Render(new Invocation(keywordName, paramArgs), localeStrings, bindings);
        }
        else
        {
            // Composite: render the body tree directly.
            return Render(def.Body!, localeStrings, bindings);
        }
    }

    // -----------------------------------------------------------------------
    //  Core rendering helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Renders an <see cref="Invocation"/> node as a <see cref="CompositeNode"/>
    /// with a template-resolved summary and a structural body.
    /// </summary>
    private RenderNode RenderInvocation(
        Invocation inv,
        IReadOnlyDictionary<string, string>? locale,
        IReadOnlyDictionary<string, object>? bindings)
    {
        _registry.TryGetValue(inv.KeywordName, out var def);

        // ── Body: recursive structural expansion ────────────────────────────
        RenderNode body;
        if (def?.Body is not null)
        {
            // Composite keyword: cache the body render in definition-time mode.
            body = bindings is null
                ? GetOrComputeBodyCache(def, locale)
                : Render(def.Body, locale, bindings);
        }
        else
        {
            // Primitive (or unknown keyword): body is the structural string.
            // Not cached — depends on the specific args in this Invocation.
            body = BuildStructural(inv.KeywordName, inv.Args, locale, bindings);
        }

        // ── Summary: locale → TextTemplate → structural ──────────────────────
        string? templateStr = null;
        locale?.TryGetValue(inv.KeywordName, out templateStr);
        templateStr ??= def?.TextTemplate;

        RenderNode summary = templateStr is not null
            ? ExpandTemplate(templateStr, def, inv.Args, locale, bindings)
            : BuildStructural(inv.KeywordName, inv.Args, locale, bindings);

        return new CompositeNode(summary, body);
    }

    /// <summary>
    /// Resolves a <see cref="ParameterRef"/> node.
    /// <list type="bullet">
    ///   <item>Definition-time (<c>bindings == null</c>): returns the parameter
    ///   name as a <see cref="TextSpan"/> label.</item>
    ///   <item>Invocation-time: looks up the value in <c>bindings</c> and
    ///   formats it, or falls back to the parameter name if not found.</item>
    /// </list>
    /// </summary>
    private static RenderNode RenderParameterRef(
        ParameterRef p,
        IReadOnlyDictionary<string, object>? bindings)
    {
        if (bindings is null)
            return new TextSpan(p.Name); // definition-time label

        return bindings.TryGetValue(p.Name, out var value)
            ? new TextSpan(FormatValue(value))
            : new TextSpan(p.Name); // not bound — fall back to label
    }

    /// <summary>
    /// Builds the structural fallback rendering for an invocation:
    /// <c>"keyword-name(arg1, arg2, …)"</c> as a <see cref="TextSpan"/>.
    /// </summary>
    private RenderNode BuildStructural(
        string keywordName,
        KeywordNode[] args,
        IReadOnlyDictionary<string, string>? locale,
        IReadOnlyDictionary<string, object>? bindings)
    {
        if (args.Length == 0)
            return new TextSpan(keywordName);

        var argTexts = args.Select(a => FlattenToText(Render(a, locale, bindings)));
        return new TextSpan($"{keywordName}({string.Join(", ", argTexts)})");
    }

    // -----------------------------------------------------------------------
    //  Template expansion
    // -----------------------------------------------------------------------

    /// <summary>
    /// Expands a <c>TextTemplate</c> string into a <see cref="RenderNode"/>,
    /// substituting <c>{paramName}</c> tokens with rendered argument nodes and
    /// producing <see cref="RulesRef"/> leaf nodes for <c>[display](key)</c>
    /// and <c>[key]</c> cross-reference tags (D18).
    /// <para>
    /// The result is a <see cref="TextSpan"/> when all tokens resolved to plain
    /// text; otherwise a <see cref="SequenceNode"/> of interleaved
    /// <see cref="TextSpan"/> and <see cref="RulesRef"/> nodes.
    /// </para>
    /// </summary>
    private RenderNode ExpandTemplate(
        string template,
        KeywordDefinition? def,
        KeywordNode[] args,
        IReadOnlyDictionary<string, string>? locale,
        IReadOnlyDictionary<string, object>? bindings)
    {
        // Split on tokens; Regex.Split with a capture group returns segments
        // and captured tokens interleaved: [plain, token, plain, token, plain].
        string[] parts = TemplateTokenRegex.Split(template);

        var nodes = new List<RenderNode>(parts.Length);

        foreach (string part in parts)
        {
            if (part.Length == 0) continue;

            if (part.StartsWith('['))
            {
                // Cross-reference tag — D18.
                nodes.Add(ParseCrossRefTag(part));
            }
            else if (part.StartsWith('{') && part.EndsWith('}'))
            {
                // Parameter substitution: {paramName}
                string paramName = part[1..^1];
                nodes.Add(ResolveParamSubstitution(paramName, def, args, locale, bindings));
            }
            else
            {
                // Plain text segment.
                nodes.Add(new TextSpan(part));
            }
        }

        // Collapse adjacent TextSpan nodes and flatten single-item sequences.
        return CollapseTextSpans(nodes);
    }

    /// <summary>
    /// Looks up the argument corresponding to <paramref name="paramName"/> in
    /// the definition's parameter list and renders it.  If the parameter name
    /// is not found (e.g. a game-creator typo), falls back to the label.
    /// </summary>
    private RenderNode ResolveParamSubstitution(
        string paramName,
        KeywordDefinition? def,
        KeywordNode[] args,
        IReadOnlyDictionary<string, string>? locale,
        IReadOnlyDictionary<string, object>? bindings)
    {
        if (def is not null)
        {
            int idx = Array.FindIndex(def.Parameters, p => p.Name == paramName);
            if (idx >= 0 && idx < args.Length)
            {
                var rendered = Render(args[idx], locale, bindings);
                // For inline template substitution we want a flat text form;
                // wrap non-TextSpan results in TextSpan via flatten so the
                // surrounding text can be merged cleanly.
                return rendered is TextSpan ? rendered : new TextSpan(FlattenToText(rendered));
            }
        }

        // Fallback: no definition or parameter not found — render as label.
        return new TextSpan(paramName);
    }

    /// <summary>
    /// Parses a cross-reference tag string from the template.
    /// Supports <c>[display](key)</c> (long form) and <c>[key]</c> (short form).
    /// </summary>
    private static RulesRef ParseCrossRefTag(string token)
    {
        // Long form: [display](key)
        int bracketClose = token.IndexOf(']');
        if (bracketClose > 0 && bracketClose + 1 < token.Length && token[bracketClose + 1] == '(')
        {
            string display = token[1..bracketClose];
            string key     = token[(bracketClose + 2)..^1]; // strip leading '(' and trailing ')'
            return new RulesRef(key, display);
        }

        // Short form: [key]  →  display == key
        string shortKey = token[1..^1];
        return new RulesRef(shortKey, shortKey);
    }

    // -----------------------------------------------------------------------
    //  Body cache helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns the cached body render for a composite keyword definition, or
    /// computes, caches, and returns it on first access.
    /// </summary>
    private RenderNode GetOrComputeBodyCache(
        KeywordDefinition def,
        IReadOnlyDictionary<string, string>? locale)
    {
        var cache = locale is null
            ? _nullLocaleBodyCache
            : _localeBodyCaches.GetValue(locale,
                _ => new Dictionary<KeywordDefinition, RenderNode>(
                    ReferenceEqualityComparer.Instance));

        if (!cache.TryGetValue(def, out var cached))
        {
            // Recurse into the body tree with no bindings (definition-time).
            cached = Render(def.Body!, locale, null);
            cache[def] = cached;
        }

        return cached;
    }

    // -----------------------------------------------------------------------
    //  Locale helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Resolves an engine reserved locale key: locale file first, then
    /// built-in English default.
    /// </summary>
    private static string ResolveEngineLocale(
        IReadOnlyDictionary<string, string>? localeStrings,
        string key)
    {
        if (localeStrings is not null && localeStrings.TryGetValue(key, out var val))
            return val;

        return _engineDefaults[key];
    }

    /// <summary>
    /// Expands a simple template string (engine lifetime keys only) that may
    /// contain named <c>{key}</c> placeholders from the given replacements.
    /// </summary>
    private static TextSpan ExpandSimpleTemplate(
        string template,
        params (string key, string value)[] replacements)
    {
        string result = template;
        foreach (var (key, value) in replacements)
            result = result.Replace("{" + key + "}", value);
        return new TextSpan(result);
    }

    // -----------------------------------------------------------------------
    //  Utility helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Flattens a <see cref="RenderNode"/> tree to a plain string by collecting
    /// all <see cref="TextSpan.Text"/> values and <see cref="RulesRef.DisplayText"/>
    /// values recursively.  Used for inline template substitution and structural
    /// fallback text building.
    /// </summary>
    public static string FlattenToText(RenderNode node) => node switch
    {
        TextSpan ts    => ts.Text,
        CompositeNode c => FlattenToText(c.Summary),
        SequenceNode s  => string.Concat(s.Items.Select(FlattenToText)),
        RulesRef rr    => rr.DisplayText,
        _              => string.Empty,
    };

    /// <summary>
    /// Formats a runtime or literal value as a display string.
    /// Doubles are formatted with <c>"G"</c> to suppress trailing zeros
    /// (e.g. <c>3.0</c> → <c>"3"</c>).
    /// </summary>
    private static string FormatValue(object value) => value switch
    {
        double d  => d.ToString("G"),
        float f   => f.ToString("G"),
        bool b    => b.ToString(),
        string s  => s,
        _         => value.ToString() ?? string.Empty,
    };

    /// <summary>
    /// Collapses a flat list of <see cref="RenderNode"/>s by merging adjacent
    /// <see cref="TextSpan"/> nodes and simplifying single-item sequences to
    /// their single child.
    /// </summary>
    private static RenderNode CollapseTextSpans(List<RenderNode> nodes)
    {
        if (nodes.Count == 0)
            return new TextSpan(string.Empty);

        // Merge adjacent TextSpans into a single TextSpan.
        var merged = new List<RenderNode>(nodes.Count);
        foreach (var node in nodes)
        {
            if (node is TextSpan ts && merged.Count > 0 && merged[^1] is TextSpan prev)
            {
                // Merge into the previous TextSpan.
                merged[^1] = new TextSpan(prev.Text + ts.Text);
            }
            else
            {
                merged.Add(node);
            }
        }

        return merged.Count == 1 ? merged[0] : new SequenceNode(merged);
    }
}
