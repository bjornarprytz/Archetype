using Archetype.Build;
using Archetype.Core;
using Archetype.Engine;
using Archetype.Tests.Helpers;

// Disambiguate: Archetype.Tests.GameSession is a namespace (test folder);
// Archetype.Engine.GameSession is the class we want.
using EngineGameSession = Archetype.Engine.GameSession;

namespace Archetype.Tests.CostModel;

/// <summary>
/// Tests for the sequential cost validation model (D21) and cost payment at
/// execution time (D23).
/// </summary>
public sealed class CostModelTests
{
    // -----------------------------------------------------------------------
    //  CostValidator unit tests (Layer 1 — isolated)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Builds a <see cref="CostDef"/> whose body asserts that the session
    /// accumulator <paramref name="accName"/> is &gt;= <paramref name="required"/>.
    /// Deducts <paramref name="required"/> from the accumulator on payment.
    /// </summary>
    private static CostDef BuildEnergyCheckerCost(
        string accName, double required, AtomId sessionAtomLiteral)
    {
        // assert(get-state(session, accName) >= required)
        var assertStep = new EffectBlockStep("assert", [
            new Invocation("at-least",
                new Invocation("get-state",
                    new Literal(sessionAtomLiteral),
                    new Literal(accName)),
                new Literal(required)),
            new Literal((double)(int)OnFail.Panic),
            new Literal((double)(int)NotifyFlag.Off),
        ]);

        // modify-accumulator(session, accName, -required)
        var payStep = new EffectBlockStep("modify-accumulator", [
            new Literal(sessionAtomLiteral),
            new Literal(accName),
            new Literal(-required),
        ]);

        return new CostDef(
            Body: new EffectBlockDef([assertStep, payStep]),
            Parameters: [],
            TextTemplate: $"Pay {required} {accName}");
    }

    /// <summary>
    /// Minimal definition and infrastructure needed for CostValidator.
    /// </summary>
    private static (GameState state, GameDefinition def, AtomId sessionAtomId)
        BuildValidatorInfra(string accName = "energy", double accValue = 5.0)
    {
        var state   = new GameStateBuilder().Build();
        var session = state.SessionAtomId;
        state.GetAtom(session).Accumulators[accName] = accValue;

        var def = TestDefinition.Minimal();
        return (state, def, session);
    }

    private static IReadOnlyDictionary<string, IPlayerStrategy> NoStrategies =>
        new Dictionary<string, IPlayerStrategy>();

    // -----------------------------------------------------------------------
    //  9.7 — single cost, affordable → IsValid = true; state unchanged
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.7  Single cost whose assert passes: <c>IsValid = true</c>;
    ///      original <see cref="GameState"/> is not mutated.
    /// </summary>
    [Fact]
    public void CostValidator_SingleCost_Affordable_ReturnsValid()
    {
        var (state, def, session) = BuildValidatorInfra("energy", 5.0);
        var cost = BuildEnergyCheckerCost("energy", 2.0, session);
        var action = new PlayCard(AtomId.None); // atom id irrelevant for validator test

        var result = CostValidator.Validate(
            costs:            [cost],
            action:           action,
            state:            state,
            definition:       def,
            strategies:       NoStrategies,
            randomSource:     new MockRandomSource(),
            activePlayerName: "p1");

        Assert.True(result.IsValid);
        // Original state must not be mutated — energy still 5.0.
        Assert.Equal(5.0, state.GetAtom(session).Accumulators["energy"]);
    }

    // -----------------------------------------------------------------------
    //  9.8 — single cost, not affordable → IsValid = false; state unchanged
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.8  Single cost whose assert fails: <c>IsValid = false</c>;
    ///      original <see cref="GameState"/> is not mutated.
    /// </summary>
    [Fact]
    public void CostValidator_SingleCost_NotAffordable_ReturnsInvalid()
    {
        var (state, def, session) = BuildValidatorInfra("energy", 1.0);
        var cost = BuildEnergyCheckerCost("energy", 3.0, session); // requires 3, has 1

        var result = CostValidator.Validate(
            costs:            [cost],
            action:           new PlayCard(AtomId.None),
            state:            state,
            definition:       def,
            strategies:       NoStrategies,
            randomSource:     new MockRandomSource(),
            activePlayerName: "p1");

        Assert.False(result.IsValid);
        // Original state must not be mutated — energy still 1.0.
        Assert.Equal(1.0, state.GetAtom(session).Accumulators["energy"]);
    }

    // -----------------------------------------------------------------------
    //  9.9 — multiple costs, combined body fails → IsValid = false; state unchanged
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.9  Two costs combined: cost1 deducts 3 energy; cost2 asserts energy &gt;= 3.
    ///      The combined body fails because after cost1's deduction, only 2 energy
    ///      remain.  <c>IsValid = false</c>; original state unchanged.
    /// </summary>
    [Fact]
    public void CostValidator_MultipleCosts_CombinedBodyFails_ReturnsInvalid()
    {
        var (state, def, session) = BuildValidatorInfra("energy", 5.0);

        // cost1: assert energy >= 3, then deduct 3 (leaves 2 on clone)
        var cost1 = BuildEnergyCheckerCost("energy", 3.0, session);
        // cost2: assert energy >= 3 (but only 2 remain after cost1)
        var cost2 = BuildEnergyCheckerCost("energy", 3.0, session);

        var result = CostValidator.Validate(
            costs:            [cost1, cost2],
            action:           new PlayCard(AtomId.None),
            state:            state,
            definition:       def,
            strategies:       NoStrategies,
            randomSource:     new MockRandomSource(),
            activePlayerName: "p1");

        Assert.False(result.IsValid);
        // Original state must not be mutated — energy still 5.0.
        Assert.Equal(5.0, state.GetAtom(session).Accumulators["energy"]);
    }

    // -----------------------------------------------------------------------
    //  9.10 — all costs pass; GameState unchanged
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.10  Validation succeeds; original <see cref="GameState"/> is not mutated.
    /// </summary>
    [Fact]
    public void CostValidator_AllCostsPass_GameStateUnchanged()
    {
        var (state, def, session) = BuildValidatorInfra("energy", 10.0);

        var cost1 = BuildEnergyCheckerCost("energy", 3.0, session);
        var cost2 = BuildEnergyCheckerCost("energy", 2.0, session);

        var result = CostValidator.Validate(
            costs:            [cost1, cost2],
            action:           new PlayCard(AtomId.None),
            state:            state,
            definition:       def,
            strategies:       NoStrategies,
            randomSource:     new MockRandomSource(),
            activePlayerName: "p1");

        Assert.True(result.IsValid);
        // Original state must not be mutated — energy still 10.0.
        Assert.Equal(10.0, state.GetAtom(session).Accumulators["energy"]);
    }

    // -----------------------------------------------------------------------
    //  9.11 — missing cost arg → IsValid = false
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.11  A cost that uses a missing binding raises an engine error.
    ///       <c>IsValid = false</c>.
    /// </summary>
    [Fact]
    public void CostValidator_MissingCostArg_ReturnsInvalid()
    {
        var (state, def, session) = BuildValidatorInfra("energy", 5.0);

        // Cost body references Param("discard-target") but CostChoices is empty.
        // The Invocation will fail when the binding is absent.
        var costWithMissingParam = new CostDef(
            Body: new EffectBlockDef([
                new EffectBlockStep("modify-accumulator", [
                    new ParameterRef("discard-target"), // missing — will throw
                    new Literal("energy"),
                    new Literal(-1.0),
                ]),
            ]),
            Parameters: [new ParameterDecl("discard-target", TypeName.Atom)],
            TextTemplate: "Discard {discard-target}");

        var result = CostValidator.Validate(
            costs:            [costWithMissingParam],
            action:           new PlayCard(AtomId.None), // CostChoices is null
            state:            state,
            definition:       def,
            strategies:       NoStrategies,
            randomSource:     new MockRandomSource(),
            activePlayerName: "p1");

        // Missing binding causes an engine exception — IsValid = false.
        Assert.False(result.IsValid);
    }

    // -----------------------------------------------------------------------
    //  9.12 — CostTexts populated regardless of outcome
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.12  Validation fails; <c>CostTexts</c> still contains one resolved
    ///       string per <see cref="CostDef"/>, populated from
    ///       <see cref="CostDef.TextTemplate"/>.
    /// </summary>
    [Fact]
    public void CostValidator_CostTexts_PopulatedRegardlessOfOutcome()
    {
        var (state, def, session) = BuildValidatorInfra("energy", 0.0);
        var cost = BuildEnergyCheckerCost("energy", 5.0, session); // will fail: 0 < 5

        var result = CostValidator.Validate(
            costs:            [cost],
            action:           new PlayCard(AtomId.None),
            state:            state,
            definition:       def,
            strategies:       NoStrategies,
            randomSource:     new MockRandomSource(),
            activePlayerName: "p1");

        Assert.False(result.IsValid);
        // CostTexts must still be populated.
        Assert.Single(result.CostTexts);
        Assert.Equal("Pay 5 energy", result.CostTexts[0]);
    }

    // -----------------------------------------------------------------------
    //  9.16 — PlayCard with cost: cost events appear before primary effect
    // -----------------------------------------------------------------------

    /// <summary>
    /// 9.16  PlayCard action where the card has a cost: cost events appear in
    ///       <c>events.this_action</c> before the primary effect events (D23).
    /// </summary>
    [Fact]
    public async Task PlayCard_WithCost_CostBodyExecutesBeforeEffect()
    {
        var eventLog = new List<string>(); // captures keyword names in order

        // Card has a cost that calls modify-accumulator("energy", -1) and
        // a primary effect that calls modify-accumulator("score", +1).
        // Both use the session atom.

        var capturing = new CapturingWithLogStrategy();

        var def = BuildGameDefWithCostCard(capturing);

        var result = await EngineGameSession.Create(def)
            .WithPlayerStrategy("p1", capturing)
            .WithRandomSource(new MockRandomSource())
            .UseDefaultInit()
            .Build()
            .RunAsync();

        // Verify events from the action are in the log.
        // The cost's modify-accumulator("cost-paid") must appear before
        // the primary effect's modify-accumulator("effect-fired").
        var actionEvents = result.FinalLog
            .Where(e => e.KeywordName == "modify-accumulator")
            .Select(e => e.BoundArgs.TryGetValue("name", out var n) ? n as string : null)
            .Where(n => n is not null)
            .ToList();

        Assert.Contains("cost-paid",    actionEvents);
        Assert.Contains("effect-fired", actionEvents);

        var costIdx   = actionEvents.IndexOf("cost-paid");
        var effectIdx = actionEvents.IndexOf("effect-fired");
        Assert.True(costIdx < effectIdx,
            $"Expected cost-paid (idx {costIdx}) before effect-fired (idx {effectIdx}).");
    }

    // -----------------------------------------------------------------------
    //  Section 9.16 helpers
    // -----------------------------------------------------------------------

    private static GameDefinition BuildGameDefWithCostCard(IPlayerStrategy strategy)
    {
        // Always-true SBR to end the game after the action window.
        var alwaysTrue = Kw.AtLeast(new Literal(1.0), new Literal(1.0));

        // Session atom id is allocated at provisioning, but we can use
        // Invocation("session") to refer to it at runtime.
        var costBody = new EffectBlockDef([
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"),
                new Literal("cost-paid"),
                new Literal(1.0),
            ]),
        ]);

        var primaryEffect = new EffectBlockDef([
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"),
                new Literal("effect-fired"),
                new Literal(1.0),
            ]),
        ]);

        var cardDef = new CardDefinition(
            Name:               "test-card",
            StaticProperties:   new Dictionary<string, object>(),
            PrimaryEffect:      primaryEffect,
            AdditionalEffects:  [],
            StaticEffects:      [],
            ActivationCondition: null,
            Cost: [new CostDef(costBody, [], "cost text")]);

        return new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>
            {
                ["test-card"] = cardDef,
            },
            ZoneDefinitions: new Dictionary<string, ZoneDefinition>
            {
                ["hand"] = new ZoneDefinition("hand", new Dictionary<string, object>()),
            },
            CardSets:               new Dictionary<string, CardSet>(),
            StateBasedRules:
            [
                new StateBasedRule("end-game", alwaysTrue,
                    new EffectBlockDef([new EffectBlockStep("declare-draw", [])])),
            ],
            Phases:
            [
                new PhaseDefinition("main"),
            ],
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions: new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new PlayerDefinition(new Dictionary<string, object>()),
            },
            DefaultInitManifest: new InitManifest(
                Zones:  [new ZoneSpec("p1-hand", "p1", "hand")],
                Cards:  [new CardSpec("p1", "p1-hand", "test-card")],
                PlayerStates: []),
            PlayableZoneNames: ["hand"],
            Id: "test-game");
    }
}

// ---------------------------------------------------------------------------
//  Test-local strategy: plays the first available card then passes
// ---------------------------------------------------------------------------

/// <summary>
/// A player strategy that plays the first available card on the first call,
/// then always returns <see cref="Pass"/>.
/// </summary>
file sealed class CapturingWithLogStrategy : IPlayerStrategy
{
    private bool _played;

    public Task<PlayerAction?> SelectActionAsync(
        AvailableActions available, GameStateView state, CancellationToken ct = default)
    {
        if (!_played && available.PlayableCards.Count > 0)
        {
            _played = true;
            return Task.FromResult<PlayerAction?>(new PlayCard(available.PlayableCards[0].Card));
        }
        return Task.FromResult<PlayerAction?>(new Pass());
    }

    public Task<PromptResponse> RespondToPromptAsync(
        PromptContext context, GameStateView state, CancellationToken ct = default)
        => throw new InvalidOperationException("Not expected.");
}
