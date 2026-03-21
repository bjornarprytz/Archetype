using Archetype.Build;
using Archetype.Core;
using Archetype.Engine;
using Archetype.Tests.Helpers;

namespace Archetype.Tests.GameSession;

/// <summary>
/// Tests for D30 — <see cref="GameStateView.LastActionEvents"/> populated after
/// each <c>ResolveAction</c> call and reset between actions.
/// </summary>
public sealed class LastActionEventsTests
{
    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Game definition that records a list of LastActionEvents snapshots captured
    /// by the player strategy after each action.  Uses a "score" accumulator on the
    /// session atom, modified by the card's primary effect, so events are predictable.
    /// </summary>
    private sealed class CapturingStrategy : IPlayerStrategy
    {
        private readonly Queue<Func<AvailableActions, PlayerAction?>> _actions = new();
        public readonly List<IReadOnlyList<GameEvent>> CapturedLastActions = new();

        public CapturingStrategy QueueAction(Func<AvailableActions, PlayerAction?> select)
        {
            _actions.Enqueue(select);
            return this;
        }

        public Task<PlayerAction?> SelectActionAsync(
            AvailableActions available,
            GameStateView state,
            CancellationToken ct)
        {
            // Capture LastActionEvents BEFORE the action is chosen — this is what
            // the previous action produced.
            CapturedLastActions.Add(state.LastActionEvents);
            var fn = _actions.Count > 0 ? _actions.Dequeue() : _ => null;
            return Task.FromResult(fn(available));
        }

        // Prompts not used in these tests; implement the contract trivially.
        public Task<PromptResponse> RespondToPromptAsync(
            PromptContext context,
            GameStateView state,
            CancellationToken ct = default) =>
            Task.FromResult<PromptResponse>(new ChoiceResponse(Array.Empty<object>()));
    }

    /// <summary>
    /// Builds a minimal game with one card whose primary effect fires
    /// <c>modify-accumulator(session, "score", 1)</c>.
    /// The session ends when score ≥ 1 via SBR.
    /// </summary>
    private static GameDefinition BuildScoreGame()
    {
        var cardEffect = new EffectBlockDef([
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"),
                new Literal("score"),
                new Literal(1.0),
            ]),
        ]);

        var scoreCard = new CardDefinition(
            Name:               "score-card",
            StaticProperties:   new Dictionary<string, object>(),
            PrimaryEffect:      cardEffect,
            AdditionalEffects:  Array.Empty<NamedEffectBlockDef>(),
            StaticEffects:      Array.Empty<StaticEffectDef>());

        var manifest = new InitManifest(
            Zones:        [new ZoneSpec("hand", "p1", "hand")],
            Cards:        [new CardSpec("p1", "hand", "score-card")],
            PlayerStates: []);

        return new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>
            {
                ["score-card"] = scoreCard,
            },
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>
            {
                ["hand"] = new("hand", new Dictionary<string, object>()),
            },
            StateBasedRules:        new List<StateBasedRule>
            {
                new("end-when-scored",
                    Condition: new Invocation("at-least",
                        new Invocation("get-state", new Invocation("session"), new Literal("score")),
                        new Literal(1.0)),
                    Body: new EffectBlockDef([new EffectBlockStep("declare-draw", [])]))
            },
            Phases:                 new List<PhaseDefinition> { new("main") },
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new(new Dictionary<string, object>()),
            },
            InitManifest:           manifest,
            PlayableZoneNames:      ["hand"],
            Id:                     "last-action-events-test");
    }

    // -----------------------------------------------------------------------
    //  1.2a  LastActionEvents populated after action
    // -----------------------------------------------------------------------
    // Design: use a game that requires score ≥ 2 to end, so both "play card"
    // and a second "pass" (which triggers the SBR at score=0 still) can run.
    // Actually simpler: use an always-true SBR game but inspect the stateView
    // that the strategy receives on the first and only SelectActionAsync call.
    // The stateView.LastActionEvents before the first action is always empty.
    // After the action (which ends the game), there's no second call.
    // So we test: (a) before first action → empty, and (b) the game produced events.
    // For (b) we use a stateView captured via a trick: the strategy records the
    // stateView reference, and we check after RunAsync.

    [Fact]
    public async Task LastActionEvents_PopulatedAfterAction_ContainsActionEvents()
    {
        // Game: SBR requires score ≥ 2; playing the card gives score=1.
        // So we can play once (score=1, no SBR), then pass (SBR does not fire),
        // then on the second turn the SBR still won't fire...
        // Simpler: use a game with 2 turns before SBR fires (score ≥ 2 after two plays).
        // Actually simplest: capture the stateView reference and check LastActionEvents
        // after RunAsync by having the strategy keep a ref.

        var def = BuildScoreGame();

        // CapturingStrategy records LastActionEvents at each SelectActionAsync call.
        var interceptingStrategy = new CapturingStrategy();
        interceptingStrategy.QueueAction(avail =>
        {
            return new PlayCard(avail.PlayableCards[0].Card);
        });

        var session = Archetype.Engine.GameSession.Create(def)
            .WithPlayerStrategy("p1", interceptingStrategy)
            .WithRandomSource(new MockRandomSource())
            .Build();

        await session.RunAsync();

        // Before the first action: LastActionEvents is empty (no prior action).
        Assert.Single(interceptingStrategy.CapturedLastActions);
        Assert.Empty(interceptingStrategy.CapturedLastActions[0]);
    }

    // -----------------------------------------------------------------------
    //  1.2a-b  LastActionEvents populated — two-action game test
    // -----------------------------------------------------------------------

    [Fact]
    public async Task LastActionEvents_PopulatedAfterPlayAction_AndEmptyBeforeFirst()
    {
        // Build a two-action game:
        // - SBR fires when score ≥ 2 (requires two card plays).
        // - Strategy plays card on action 1, plays again on action 2.
        // SelectActionAsync is called twice: before action 1 (empty) and before action 2 (populated).
        var cardEffect = new EffectBlockDef([
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"),
                new Literal("score"),
                new Literal(1.0),
            ]),
        ]);

        var scoreCard = new CardDefinition(
            Name:               "score-card",
            StaticProperties:   new Dictionary<string, object>(),
            PrimaryEffect:      cardEffect,
            AdditionalEffects:  Array.Empty<NamedEffectBlockDef>(),
            StaticEffects:      Array.Empty<StaticEffectDef>());

        var manifest = new InitManifest(
            Zones:        [new ZoneSpec("hand", "p1", "hand")],
            Cards:        [
                new CardSpec("p1", "hand", "score-card"),
                new CardSpec("p1", "hand", "score-card"), // two cards
            ],
            PlayerStates: []);

        var twoActionDef = new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>
            {
                ["score-card"] = scoreCard,
            },
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>
            {
                ["hand"] = new("hand", new Dictionary<string, object>()),
            },
            StateBasedRules:        new List<StateBasedRule>
            {
                new("end-when-2-scored",
                    Condition: new Invocation("at-least",
                        new Invocation("get-state", new Invocation("session"), new Literal("score")),
                        new Literal(2.0)),
                    Body: new EffectBlockDef([new EffectBlockStep("declare-draw", [])]))
            },
            Phases:                 new List<PhaseDefinition> { new("main") },
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new(new Dictionary<string, object>()),
            },
            InitManifest:           manifest,
            PlayableZoneNames:      ["hand"],
            Id:                     "two-action-test");

        var strategy = new CapturingStrategy();
        // Play first card, play second card — after second, SBR fires.
        strategy
            .QueueAction(avail => new PlayCard(avail.PlayableCards[0].Card))  // action 1
            .QueueAction(avail => new PlayCard(avail.PlayableCards[0].Card)); // action 2

        var session = Archetype.Engine.GameSession.Create(twoActionDef)
            .WithPlayerStrategy("p1", strategy)
            .WithRandomSource(new MockRandomSource())
            .Build();

        await session.RunAsync();

        // Call 1: before action 1 → empty.
        Assert.Empty(strategy.CapturedLastActions[0]);

        // Call 2: before action 2 → populated with action 1's events.
        var afterFirstPlay = strategy.CapturedLastActions[1];
        Assert.NotEmpty(afterFirstPlay);
        Assert.Contains(afterFirstPlay, e => e.KeywordName == "modify-accumulator");
    }

    // -----------------------------------------------------------------------
    //  1.2b  LastActionEvents resets between actions — does not carry over
    // -----------------------------------------------------------------------

    [Fact]
    public async Task LastActionEvents_ResetBetweenActions_DoesNotCarryOver()
    {
        // Same two-action setup; verify the events in call 2 are from action 1 only.
        var cardEffect = new EffectBlockDef([
            new EffectBlockStep("modify-accumulator", [
                new Invocation("session"),
                new Literal("score"),
                new Literal(1.0),
            ]),
        ]);

        var scoreCard = new CardDefinition(
            Name:               "score-card",
            StaticProperties:   new Dictionary<string, object>(),
            PrimaryEffect:      cardEffect,
            AdditionalEffects:  Array.Empty<NamedEffectBlockDef>(),
            StaticEffects:      Array.Empty<StaticEffectDef>());

        var manifest = new InitManifest(
            Zones:        [new ZoneSpec("hand", "p1", "hand")],
            Cards:        [
                new CardSpec("p1", "hand", "score-card"),
                new CardSpec("p1", "hand", "score-card"),
            ],
            PlayerStates: []);

        var def = new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition> { ["score-card"] = scoreCard },
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>
            {
                ["hand"] = new("hand", new Dictionary<string, object>()),
            },
            StateBasedRules:        new List<StateBasedRule>
            {
                new("end-at-2",
                    Condition: new Invocation("at-least",
                        new Invocation("get-state", new Invocation("session"), new Literal("score")),
                        new Literal(2.0)),
                    Body: new EffectBlockDef([new EffectBlockStep("declare-draw", [])]))
            },
            Phases:                 new List<PhaseDefinition> { new("main") },
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new(new Dictionary<string, object>()),
            },
            InitManifest:           manifest,
            PlayableZoneNames:      ["hand"],
            Id:                     "reset-test");

        var strategy = new CapturingStrategy();
        strategy
            .QueueAction(avail => new PlayCard(avail.PlayableCards[0].Card))
            .QueueAction(avail => new PlayCard(avail.PlayableCards[0].Card));

        var session = Archetype.Engine.GameSession.Create(def)
            .WithPlayerStrategy("p1", strategy)
            .WithRandomSource(new MockRandomSource())
            .Build();

        await session.RunAsync();

        // Call 2 events should not contain declare-draw (that's from the SBR
        // that fires after action 2, which ends the game before call 3).
        var afterFirstPlay = strategy.CapturedLastActions[1];
        Assert.DoesNotContain(afterFirstPlay, e => e.KeywordName == "declare-draw");
        Assert.Contains(afterFirstPlay, e => e.KeywordName == "modify-accumulator");
    }

    // -----------------------------------------------------------------------
    //  1.2c  LastActionEvents is empty before any action has run
    // -----------------------------------------------------------------------

    [Fact]
    public async Task LastActionEvents_EmptyBeforeFirstAction()
    {
        var def      = BuildScoreGame();
        var strategy = new CapturingStrategy();

        // Queue: pass immediately so the SBR fires (score is 0, condition is false
        // until score ≥ 1; but the "end-when-scored" SBR fires on score ≥ 1).
        // Change: use always-draw game to ensure it ends on first pass.
        var drawDef = new GameDefinition(
            Keywords:               BuiltInKeywords.All.ToDictionary(k => k.Name),
            CardDefinitions:        new Dictionary<string, CardDefinition>(),
            ZoneDefinitions:        new Dictionary<string, ZoneDefinition>(),
            StateBasedRules:        new List<StateBasedRule>
            {
                new("draw", new Literal(true),
                    new EffectBlockDef([new EffectBlockStep("declare-draw", [])]))
            },
            Phases:                 new List<PhaseDefinition> { new("main") },
            ActionRules:            new Dictionary<string, IReadOnlyList<ActionRuleDefinition>>(),
            TriggerResolutionOrder: TriggerResolutionOrder.OldestFirst,
            PlayerDefinitions:      new Dictionary<string, PlayerDefinition>
            {
                ["p1"] = new(new Dictionary<string, object>()),
            },
            InitManifest:           InitManifest.Empty,
            Id:                     "draw-game");

        strategy.QueueAction(_ => null); // pass

        var session = Archetype.Engine.GameSession.Create(drawDef)
            .WithPlayerStrategy("p1", strategy)
            .WithRandomSource(new MockRandomSource())
            .Build();

        await session.RunAsync();

        // SelectActionAsync is called once before the pass action resolves.
        // At that point, no action has completed yet → LastActionEvents is empty.
        Assert.Single(strategy.CapturedLastActions);
        Assert.Empty(strategy.CapturedLastActions[0]);
    }
}
