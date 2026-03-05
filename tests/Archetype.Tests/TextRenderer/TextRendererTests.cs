using Archetype.Build;
using Archetype.Core;
using Archetype.Text;

namespace Archetype.Tests.TextRenderer;

/// <summary>
/// Tests for <see cref="Archetype.Text.TextRenderer"/> (D11, D18).
/// <para>
/// Layer 1 (T1–T8): unit tests — isolated renderer, no engine, no game state.
/// Cover the core invariants: node types, template resolution order,
/// cross-reference tags, block rendering, static-effect rendering,
/// lifetime-spec rendering, and caching.
/// </para>
/// <para>
/// Layer 2 (T9–T10): dual-use invariant tests — the same
/// <see cref="KeywordDefinition"/> tree drives both execution and rendering.
/// These are the architecturally significant tests (D2 dual-use, D11).
/// </para>
/// </summary>
public sealed class TextRendererTests
{
    // -----------------------------------------------------------------------
    //  Helpers shared across tests
    // -----------------------------------------------------------------------

    /// <summary>
    /// Flattens a <see cref="RenderNode"/> tree to a plain string by collecting
    /// all text content (TextSpan.Text, RulesRef.DisplayText).
    /// Uses <see cref="Archetype.Text.TextRenderer.FlattenToText"/> directly.
    /// </summary>
    private static string Flat(RenderNode node) => Archetype.Text.TextRenderer.FlattenToText(node);

    /// <summary>
    /// Extracts the <see cref="CompositeNode.Summary"/> of the outermost
    /// <see cref="CompositeNode"/> in the tree.  Fails fast if the root is not
    /// a <see cref="CompositeNode"/>.
    /// </summary>
    private static RenderNode Summary(RenderNode node)
    {
        Assert.IsType<CompositeNode>(node);
        return ((CompositeNode)node).Summary;
    }

    /// <summary>
    /// Extracts the <see cref="CompositeNode.Body"/> of the outermost
    /// <see cref="CompositeNode"/>.
    /// </summary>
    private static RenderNode Body(RenderNode node)
    {
        Assert.IsType<CompositeNode>(node);
        return ((CompositeNode)node).Body;
    }

    /// <summary>
    /// Builds a minimal <see cref="TextRenderer"/> backed by the full
    /// built-in keyword registry.
    /// </summary>
    private static Archetype.Text.TextRenderer Renderer() =>
        new(BuiltInKeywords.All.ToDictionary(k => k.Name));

    /// <summary>
    /// Builds a <see cref="TextRenderer"/> that also includes the given
    /// game-creator-defined keyword.
    /// </summary>
    private static Archetype.Text.TextRenderer RendererWith(KeywordDefinition extra)
    {
        var dict = BuiltInKeywords.All.ToDictionary(k => k.Name);
        dict[extra.Name] = extra;
        return new Archetype.Text.TextRenderer(dict);
    }

    // -----------------------------------------------------------------------
    //  T1 — ParameterRef renders as label in definition-time mode
    // -----------------------------------------------------------------------

    /// <summary>
    /// T1a  ParameterRef with no bindings (definition-time) renders as the
    ///      parameter name label.
    /// </summary>
    [Fact]
    public void ParameterRef_DefinitionTime_RendersAsLabel()
    {
        var renderer = Renderer();
        var node = new ParameterRef("amount");

        var result = renderer.Render(node, localeStrings: null, bindings: null);

        var span = Assert.IsType<TextSpan>(result);
        Assert.Equal("amount", span.Text);
    }

    /// <summary>
    /// T1b  ParameterRef with a binding (invocation-time) renders the bound
    ///      value, not the label.
    /// </summary>
    [Fact]
    public void ParameterRef_InvocationTime_RendersValue()
    {
        var renderer = Renderer();
        var node     = new ParameterRef("amount");
        var bindings = new Dictionary<string, object> { ["amount"] = 5.0 };

        var result = renderer.Render(node, localeStrings: null, bindings);

        var span = Assert.IsType<TextSpan>(result);
        Assert.Equal("5", span.Text); // double 5.0 → "5" via "G" format
    }

    // -----------------------------------------------------------------------
    //  T2 — Literal always renders as its formatted value
    // -----------------------------------------------------------------------

    /// <summary>
    /// T2  Literal values render as formatted text regardless of mode.
    /// </summary>
    [Fact]
    public void Literal_RendersAsFormattedValue()
    {
        var renderer = Renderer();

        var spanDouble = renderer.Render(new Literal(3.0),   null, null);
        var spanString = renderer.Render(new Literal("foo"), null, null);
        var spanBool   = renderer.Render(new Literal(true),  null, null);

        Assert.Equal("3",    Assert.IsType<TextSpan>(spanDouble).Text);
        Assert.Equal("foo",  Assert.IsType<TextSpan>(spanString).Text);
        Assert.Equal("True", Assert.IsType<TextSpan>(spanBool).Text);
    }

    // -----------------------------------------------------------------------
    //  T3 — Invocation produces CompositeNode with TextTemplate summary
    // -----------------------------------------------------------------------

    /// <summary>
    /// T3  Rendering a built-in Invocation with its registered TextTemplate
    ///     produces a CompositeNode whose summary expands the template.
    ///     <c>add(Literal(2.0), Literal(3.0))</c> → summary contains "2 + 3".
    /// </summary>
    [Fact]
    public void Invocation_WithTextTemplate_ProducesSummaryFromTemplate()
    {
        var renderer = Renderer();
        // add has TextTemplate "{a} + {b}"
        var node = new Invocation("add", new Literal(2.0), new Literal(3.0));

        var result = renderer.Render(node, localeStrings: null, bindings: null);

        Assert.IsType<CompositeNode>(result);
        Assert.Equal("2 + 3", Flat(Summary(result)));
    }

    /// <summary>
    /// T3b  The body of the CompositeNode for a primitive is the structural
    ///      fallback <c>"add(2, 3)"</c>, not the template text.
    /// </summary>
    [Fact]
    public void Invocation_Primitive_BodyIsStructuralFallback()
    {
        var renderer = Renderer();
        var node = new Invocation("add", new Literal(2.0), new Literal(3.0));

        var result = renderer.Render(node, localeStrings: null, bindings: null);

        // Primitive has no Body tree → body is structural string.
        Assert.Equal("add(2, 3)", Flat(Body(result)));
    }

    // -----------------------------------------------------------------------
    //  T4 — Structural fallback when no template exists
    // -----------------------------------------------------------------------

    /// <summary>
    /// T4  A game-creator keyword with no TextTemplate falls back to
    ///     structural rendering: <c>"my-keyword(arg1, arg2)"</c>.
    /// </summary>
    [Fact]
    public void Invocation_NoTemplate_FallsBackToStructural()
    {
        // Game-creator composite keyword: body = add(a, b), no TextTemplate.
        var customDef = new KeywordDefinition(
            Name: "my-boost",
            Parameters: [
                new("target", TypeName.Atom),
                new("amount", TypeName.Number),
            ],
            ReturnType:  TypeName.Boolean,
            Description: "custom",
            Body: new Invocation("add",
                new ParameterRef("amount"),
                new Literal(1.0)));

        var renderer = RendererWith(customDef);
        var node = new Invocation("my-boost",
            new ParameterRef("target"),
            new Literal(5.0));

        var result = renderer.Render(node, localeStrings: null, bindings: null);

        // No TextTemplate → structural summary = "my-boost(target, 5)"
        Assert.Contains("my-boost", Flat(Summary(result)));
        Assert.Contains("target",   Flat(Summary(result)));
        Assert.Contains("5",        Flat(Summary(result)));
    }

    // -----------------------------------------------------------------------
    //  T5 — Locale string takes precedence over TextTemplate
    // -----------------------------------------------------------------------

    /// <summary>
    /// T5  A locale entry for a keyword overrides the registered TextTemplate.
    ///     Verifies locale > TextTemplate resolution order (D11).
    /// </summary>
    [Fact]
    public void Invocation_LocaleOverridesTextTemplate()
    {
        var renderer = Renderer();
        // "add" has TextTemplate "{a} + {b}" — locale overrides it.
        var locale = new Dictionary<string, string>
        {
            ["add"] = "{a} plus {b}",
        };
        var node = new Invocation("add", new Literal(2.0), new Literal(3.0));

        var result = renderer.Render(node, locale, bindings: null);

        Assert.Equal("2 plus 3", Flat(Summary(result)));
    }

    // -----------------------------------------------------------------------
    //  T6 — Cross-reference tags produce RulesRef nodes (D18)
    // -----------------------------------------------------------------------

    /// <summary>
    /// T6a  Long-form tag <c>[damage](take-damage)</c> in a template produces
    ///      a <see cref="RulesRef"/> with the correct key and display text.
    /// </summary>
    [Fact]
    public void Template_LongFormCrossRefTag_ProducesRulesRef()
    {
        // Game-creator keyword with cross-reference in TextTemplate.
        var attackDef = new KeywordDefinition(
            Name: "attack",
            Parameters: [
                new("attacker", TypeName.Atom),
                new("amount",   TypeName.Number),
            ],
            ReturnType:  TypeName.Boolean,
            Description: "custom",
            Body: new Invocation("modify-accumulator",
                new ParameterRef("attacker"),
                new Literal("health"),
                new ParameterRef("amount")),
            TextTemplate: "deal {amount} [damage](modify-accumulator)");

        var renderer = RendererWith(attackDef);
        var node = new Invocation("attack",
            new ParameterRef("attacker"),
            new Literal(3.0));

        var result = renderer.Render(node, localeStrings: null, bindings: null);
        var summary = Summary(result);

        // Summary should be a SequenceNode containing a RulesRef.
        var seq = Assert.IsType<SequenceNode>(summary);
        var rulesRef = seq.Items.OfType<RulesRef>().FirstOrDefault();
        Assert.NotNull(rulesRef);
        Assert.Equal("modify-accumulator", rulesRef!.Key);
        Assert.Equal("damage",             rulesRef.DisplayText);
    }

    /// <summary>
    /// T6b  Short-form tag <c>[take-damage]</c> produces a
    ///      <see cref="RulesRef"/> where key == displayText.
    /// </summary>
    [Fact]
    public void Template_ShortFormCrossRefTag_ProducesRulesRef_WithMatchingKeyAndDisplay()
    {
        var customDef = new KeywordDefinition(
            Name: "strike",
            Parameters: [ new("target", TypeName.Atom) ],
            ReturnType:  TypeName.Boolean,
            Description: "custom",
            Body: new Invocation("modify-accumulator",
                new ParameterRef("target"),
                new Literal("health"),
                new Literal(-1.0)),
            TextTemplate: "[modify-accumulator] target");

        var renderer = RendererWith(customDef);
        var node = new Invocation("strike", new ParameterRef("target"));

        var result = renderer.Render(node, localeStrings: null, bindings: null);
        var summary = Summary(result);

        var seq     = Assert.IsType<SequenceNode>(summary);
        var rr      = seq.Items.OfType<RulesRef>().FirstOrDefault();
        Assert.NotNull(rr);
        Assert.Equal("modify-accumulator", rr!.Key);
        Assert.Equal("modify-accumulator", rr.DisplayText); // short form: key == display
    }

    // -----------------------------------------------------------------------
    //  T7 — RenderBlock produces SequenceNode over block steps
    // -----------------------------------------------------------------------

    /// <summary>
    /// T7  RenderBlock with two steps produces a SequenceNode with two children.
    ///     A single-step block returns the single CompositeNode directly
    ///     (no unnecessary wrapper).
    /// </summary>
    [Fact]
    public void RenderBlock_MultiStep_ProducesSequenceNode()
    {
        var renderer = Renderer();
        var twoStepBlock = new EffectBlockDef([
            new EffectBlockStep("add",      [new Literal(1.0), new Literal(2.0)]),
            new EffectBlockStep("subtract", [new Literal(5.0), new Literal(3.0)]),
        ]);

        var result = renderer.RenderBlock(twoStepBlock, localeStrings: null, bindings: null);

        var seq = Assert.IsType<SequenceNode>(result);
        Assert.Equal(2, seq.Items.Count);
    }

    [Fact]
    public void RenderBlock_SingleStep_ReturnsSingleNodeDirectly()
    {
        var renderer = Renderer();
        var oneStepBlock = new EffectBlockDef([
            new EffectBlockStep("add", [new Literal(1.0), new Literal(2.0)]),
        ]);

        var result = renderer.RenderBlock(oneStepBlock, localeStrings: null, bindings: null);

        // No SequenceNode wrapper for a single step.
        Assert.IsType<CompositeNode>(result);
    }

    // -----------------------------------------------------------------------
    //  T8a — RenderLifetimeSpec: TurnTimer, TriggerCount, WhileCondition
    // -----------------------------------------------------------------------

    /// <summary>
    /// T8a  TurnTimer renders using the engine default
    ///      <c>"for {n} turn(s)"</c>.
    /// </summary>
    [Fact]
    public void RenderLifetimeSpec_TurnTimer_UsesEngineDefault()
    {
        var renderer = Renderer();
        var spec     = new LifetimeSpec([new TurnTimer(2)]);

        var result = renderer.RenderLifetimeSpec(spec, localeStrings: null);

        var span = Assert.IsType<TextSpan>(result);
        Assert.Equal("for 2 turn(s)", span.Text);
    }

    /// <summary>
    /// T8b  TriggerCount renders using the engine default
    ///      <c>"(up to {n} time(s))"</c>.
    /// </summary>
    [Fact]
    public void RenderLifetimeSpec_TriggerCount_UsesEngineDefault()
    {
        var renderer = Renderer();
        var spec     = new LifetimeSpec([new TriggerCount(3)]);

        var result = renderer.RenderLifetimeSpec(spec, localeStrings: null);

        var span = Assert.IsType<TextSpan>(result);
        Assert.Equal("(up to 3 time(s))", span.Text);
    }

    /// <summary>
    /// T8c  WhileCondition renders the expression inline using the engine
    ///      default <c>"while {expr}"</c>.
    /// </summary>
    [Fact]
    public void RenderLifetimeSpec_WhileCondition_RendersExpressionInline()
    {
        var renderer = Renderer();
        // Condition: "a >= b" (structural rendering since there's no entity)
        var condition = new Invocation("at-least", new Literal(1.0), new Literal(0.0));
        var spec      = new LifetimeSpec([new WhileCondition(condition)]);

        var result = renderer.RenderLifetimeSpec(spec, localeStrings: null);

        var span = Assert.IsType<TextSpan>(result);
        // "at-least" has TextTemplate "{a} >= {b}" → "1 >= 0"
        Assert.Contains("while", span.Text);
        Assert.Contains("1 >= 0", span.Text);
    }

    /// <summary>
    /// T8d  Multiple lifetime conditions are joined with the or-separator.
    /// </summary>
    [Fact]
    public void RenderLifetimeSpec_MultipleConditions_JoinedWithOrSeparator()
    {
        var renderer = Renderer();
        var spec     = new LifetimeSpec([new TurnTimer(1), new TriggerCount(2)]);

        var result = renderer.RenderLifetimeSpec(spec, localeStrings: null);

        var span = Assert.IsType<TextSpan>(result);
        Assert.Contains(" or ", span.Text);
    }

    /// <summary>
    /// T8e  A locale file can override engine lifetime keys.
    /// </summary>
    [Fact]
    public void RenderLifetimeSpec_LocaleOverridesEngineDefault()
    {
        var renderer = Renderer();
        var locale   = new Dictionary<string, string>
        {
            ["engine.lifetime.turn_timer"] = "pendant {n} tour(s)",
        };
        var spec = new LifetimeSpec([new TurnTimer(3)]);

        var result = renderer.RenderLifetimeSpec(spec, locale);

        var span = Assert.IsType<TextSpan>(result);
        Assert.Equal("pendant 3 tour(s)", span.Text);
    }

    /// <summary>
    /// T8f  A permanent lifetime (no conditions) renders as an empty TextSpan.
    /// </summary>
    [Fact]
    public void RenderLifetimeSpec_Permanent_ReturnsEmptyTextSpan()
    {
        var renderer = Renderer();
        var result   = renderer.RenderLifetimeSpec(LifetimeSpec.Permanent, localeStrings: null);

        var span = Assert.IsType<TextSpan>(result);
        Assert.Equal(string.Empty, span.Text);
    }

    // -----------------------------------------------------------------------
    //  T8g — RenderStaticEffect produces SequenceNode
    // -----------------------------------------------------------------------

    /// <summary>
    /// T8g  RenderStaticEffect with a trigger and permanent lifetime produces
    ///      a SequenceNode containing: the fired-block render.
    ///      (No contribution block, no lifetime node since lifetime is permanent.)
    /// </summary>
    [Fact]
    public void RenderStaticEffect_WithTrigger_ProducesSequenceNodeWithTriggerEntry()
    {
        var renderer = Renderer();

        var firedBlock = new EffectBlockDef([
            new EffectBlockStep("add", [new Literal(1.0), new Literal(2.0)]),
        ]);

        var effect = new StaticEffectDef(
            Lifetime: LifetimeSpec.Permanent,
            Trigger: new TriggerDefinition(
                EventKeyword:   "add",
                Scope:          TriggerScope.ThisAction,
                EventParams:    [],
                Condition:      null,
                EventBindings:  [],
                FiredBlock:     firedBlock));

        var result = renderer.RenderStaticEffect(effect, localeStrings: null, bindings: null);

        var seq = Assert.IsType<SequenceNode>(result);
        // The sequence must contain the trigger entry.
        // The trigger entry is itself a SequenceNode with "when add: " prefix.
        Assert.Contains(seq.Items, item => Flat(item).Contains("when add:"));
    }

    // -----------------------------------------------------------------------
    //  T9 — Resolve (D18)
    // -----------------------------------------------------------------------

    /// <summary>
    /// T9a  Resolve returns a CompositeNode for a known primitive keyword,
    ///      using its TextTemplate with parameter name labels.
    /// </summary>
    [Fact]
    public void Resolve_KnownPrimitive_ReturnsRenderedDefinition()
    {
        var renderer = Renderer();

        var result = renderer.Resolve("add", localeStrings: null);

        Assert.NotNull(result);
        // "add" TextTemplate "{a} + {b}" with ParameterRef labels → "a + b"
        Assert.Equal("a + b", Flat(Summary(result!)));
    }

    /// <summary>
    /// T9b  Resolve returns null for an unknown keyword name.
    /// </summary>
    [Fact]
    public void Resolve_UnknownKeyword_ReturnsNull()
    {
        var renderer = Renderer();

        var result = renderer.Resolve("no-such-keyword", localeStrings: null);

        Assert.Null(result);
    }

    /// <summary>
    /// T9c  Resolve on a composite keyword renders its body tree.
    /// </summary>
    [Fact]
    public void Resolve_CompositeKeyword_RendersBodyTree()
    {
        var customDef = new KeywordDefinition(
            Name: "double-add",
            Parameters: [ new("x", TypeName.Number) ],
            ReturnType:  TypeName.Number,
            Description: "custom",
            Body: new Invocation("add", new ParameterRef("x"), new ParameterRef("x")));

        var renderer = RendererWith(customDef);
        var result   = renderer.Resolve("double-add", localeStrings: null);

        Assert.NotNull(result);
        // Body is Invocation("add", x, x) → renders as CompositeNode with "x + x"
        Assert.Contains("x", Flat(result!));
    }

    // -----------------------------------------------------------------------
    //  T10 — Definition-time caching
    // -----------------------------------------------------------------------

    /// <summary>
    /// T10  Rendering the same composite keyword definition twice in
    ///      definition-time mode returns the exact same body object (cached).
    ///      This verifies the <c>ConditionalWeakTable</c> body cache is active.
    /// </summary>
    [Fact]
    public void Caching_DefinitionTimeBodyIsCachedByReference()
    {
        var customDef = new KeywordDefinition(
            Name: "cached-kw",
            Parameters: [ new("x", TypeName.Number) ],
            ReturnType:  TypeName.Boolean,
            Description: "custom",
            Body: new Invocation("add", new ParameterRef("x"), new Literal(1.0)));

        var renderer = RendererWith(customDef);
        var inv      = new Invocation("cached-kw", new Literal(5.0));

        var first  = (CompositeNode)renderer.Render(inv, localeStrings: null, bindings: null);
        var second = (CompositeNode)renderer.Render(inv, localeStrings: null, bindings: null);

        // Same locale, same def → body is the exact same cached RenderNode instance.
        Assert.Same(first.Body, second.Body);
    }

    /// <summary>
    /// T10b  Invocation-time renders (bindings != null) are NOT cached —
    ///       two calls return different object instances for the body.
    /// </summary>
    [Fact]
    public void Caching_InvocationTimeBodyIsNotCached()
    {
        var customDef = new KeywordDefinition(
            Name: "uncached-kw",
            Parameters: [ new("x", TypeName.Number) ],
            ReturnType:  TypeName.Boolean,
            Description: "custom",
            Body: new Invocation("add", new ParameterRef("x"), new Literal(1.0)));

        var renderer = RendererWith(customDef);
        var inv      = new Invocation("uncached-kw", new ParameterRef("x"));
        var bindings = new Dictionary<string, object> { ["x"] = 5.0 };

        var first  = (CompositeNode)renderer.Render(inv, localeStrings: null, bindings);
        var second = (CompositeNode)renderer.Render(inv, localeStrings: null, bindings);

        // Invocation-time: body computed fresh each call → different instances.
        Assert.NotSame(first.Body, second.Body);
    }

    // -----------------------------------------------------------------------
    //  T11 — Dual-use invariant (D2 §1.1, D11)
    // -----------------------------------------------------------------------

    /// <summary>
    /// T11  The same <see cref="KeywordDefinition"/> body that drives execution
    ///      also drives definition-time text rendering without modification.
    ///      Verifies the central D2 dual-use invariant:
    ///      one representation, two uses (execute + render).
    /// <para>
    ///      This test authors a composite "deal-damage" keyword that composes
    ///      <c>modify-accumulator</c>, renders it in definition-time mode, and
    ///      confirms the output contains the expected template-expanded text.
    ///      The same definition object is used for both; no duplication.
    /// </para>
    /// </summary>
    [Fact]
    public void DualUse_SameDefinitionDrivesExecutionAndRendering()
    {
        // A composite keyword that wraps modify-accumulator.
        // TextTemplate uses "damage" as shorthand; body is the real expand.
        var dealDamageDef = new KeywordDefinition(
            Name: "deal-damage",
            Parameters: [
                new("target", TypeName.Atom),
                new("amount", TypeName.Number),
            ],
            ReturnType:  TypeName.Boolean,
            Description: "Reduces health of target by amount.",
            Body: new Invocation("modify-accumulator",
                new ParameterRef("target"),
                new Literal("health"),
                new Invocation("subtract", new Literal(0.0), new ParameterRef("amount"))),
            TextTemplate: "deal {amount} damage to {target}");

        var renderer = RendererWith(dealDamageDef);
        var inv      = new Invocation("deal-damage",
            new ParameterRef("target"),
            new Literal(5.0));

        // Definition-time render
        var result = renderer.Render(inv, localeStrings: null, bindings: null);

        // Summary uses the TextTemplate: "deal 5 damage to target"
        Assert.Equal("deal 5 damage to target", Flat(Summary(result)));

        // Body is the full recursive expansion.  modify-accumulator has
        // TextTemplate "modify {name} on {atom} by {delta}", so it renders as
        // "modify health on target by 0 - amount" (FlattenToText uses Summary).
        Assert.Contains("modify", Flat(Body(result)));
        Assert.Contains("health", Flat(Body(result)));
        // The delta arg is Invocation("subtract", 0, amount) → "0 - amount"
        // (subtract has TextTemplate "{a} - {b}")
        Assert.Contains("0 - amount", Flat(Body(result)));
    }

    /// <summary>
    /// T11b  Invocation-time mode substitutes bound values rather than labels.
    ///       Uses the same definition as T11.
    /// </summary>
    [Fact]
    public void DualUse_InvocationTimeSubstituesBoundValues()
    {
        var dealDamageDef = new KeywordDefinition(
            Name: "deal-damage",
            Parameters: [
                new("target", TypeName.Atom),
                new("amount", TypeName.Number),
            ],
            ReturnType:  TypeName.Boolean,
            Description: "Reduces health of target by amount.",
            Body: new Invocation("modify-accumulator",
                new ParameterRef("target"),
                new Literal("health"),
                new Invocation("subtract", new Literal(0.0), new ParameterRef("amount"))),
            TextTemplate: "deal {amount} damage to {target}");

        var renderer = RendererWith(dealDamageDef);

        // Invocation with bound values
        var inv      = new Invocation("deal-damage",
            new ParameterRef("target"),
            new ParameterRef("amount"));
        var bindings = new Dictionary<string, object>
        {
            ["target"] = "Goblin",
            ["amount"] = 3.0,
        };

        var result = renderer.Render(inv, localeStrings: null, bindings);

        // Invocation-time: parameter labels are replaced with bound values.
        Assert.Equal("deal 3 damage to Goblin", Flat(Summary(result)));
    }
}
