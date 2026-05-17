---
name: archetype-game-structure
description: Use this skill when implementing the top-level structure of a card game built on Archetype — when working with GameDefinitionBuilder, ZoneDefinition, PhaseDefinition, PlayerDefinition, ActionRuleDefinition, StateBasedRule, InitManifest, HostManifest, or assembling a GameSession.
version: 1.0.0
user-invocable: false
---

# Archetype — Game Structure

A complete game definition assembles zones, players, phases, keywords, cards, rules, and an initial state manifest into an immutable `GameDefinition`. Use `GameDefinitionBuilder` to construct it.

---

## GameDefinitionBuilder — Complete API

```csharp
using Archetype.Build;
using Archetype.Core;
using Archetype.Engine;

var definition = new GameDefinitionBuilder()
    .WithId("my-game")

    // Zones
    .AddZone("deck",       staticProps: new Dictionary<string, object>())
    .AddZone("hand",       staticProps: new Dictionary<string, object>())
    .AddZone("play-area",  staticProps: new Dictionary<string, object>())
    .AddZone("graveyard",  staticProps: new Dictionary<string, object>())

    // Players (minimum one; named registry)
    .WithPlayerDefinition("player-one", new PlayerDefinition(
        StaticProperties: new Dictionary<string, object>(),
        StateMapDeclarations: playerStateMap))
    .WithPlayerDefinition("player-two", new PlayerDefinition(
        StaticProperties: new Dictionary<string, object>(),
        StateMapDeclarations: playerStateMap))

    // Session state beyond built-in turn-number and phase-index
    .WithSessionStateMap(sessionStateMap)

    // Keywords (register all game-creator keywords)
    .RegisterKeyword(takeDamage)
    .RegisterKeyword(attack)
    .RegisterKeyword(heal)

    // Cards
    .RegisterCard(fireball)
    .RegisterCard(stoneGolem)

    // Turn structure
    .AddPhase(new PhaseDefinition("draw",  Init: drawPhaseInit,  Cleanup: null))
    .AddPhase(new PhaseDefinition("main",  Init: null,           Cleanup: null))
    .AddPhase(new PhaseDefinition("end",   Init: null,           Cleanup: endPhaseCleanup))

    // Which zones let a player play cards
    .WithPlayableZones(["hand"])

    // Action rules (what actions are legal in each phase)
    .WithActionRules("main", [
        new ActionRuleDefinition("play-card"),
        new ActionRuleDefinition("activate-ability"),
        new ActionRuleDefinition("end-turn"),
    ])

    // State-based rules (checked after every effect block)
    .AddStateBasedRule(new StateBasedRule("check-defeat", checkDefeatBody))

    // Trigger resolution order
    .WithTriggerResolutionOrder(TriggerResolutionOrder.OldestFirst)

    // Initial state (mandatory)
    .WithInitManifest(initManifest)

    .Build();
```

`Build()` validates the entire definition: all referenced keyword names exist, all zones named in `PlayableZoneNames` are defined, all atom kind restrictions in parameters are satisfied.

---

## Zones

`ZoneDefinition` declares a named region of play.

```csharp
// Zone with static properties and a state map
new ZoneDefinition(
    Name:                 "battlefield",
    StaticProperties:     new Dictionary<string, object> { ["is-public"] = true },
    StateMapDeclarations: null) // zones rarely need per-instance runtime state
```

- Zones are never destroyed once created. Model inactive zones via conditions.
- Zones created during play via `create-zone` follow the same rules.
- Cards know which zone they occupy; `in-zone(card, zone)` queries this.

---

## Player Definitions

Players form a named registry. Minimum one player is required.

```csharp
var playerStateMap = new StateMapBuilder()
    .AddAccumulator("life",   defaultValue: 20.0)
    .AddAccumulator("mana",   defaultValue: 0.0)
    .AddCondition("has-priority")
    .Build();

builder.WithPlayerDefinition("alice", new PlayerDefinition(
    StaticProperties:     new Dictionary<string, object>(),
    StateMapDeclarations: playerStateMap));
```

- Player names are the keys used in `InitManifest` provisioning and in `player-by-name`.
- "Whose turn it is" is not an engine concept — model it via session state (e.g. an accumulator or condition).

---

## Session State

The session atom is a singleton created before the first phase. It carries engine-managed `turn-number` and `phase-index` accumulators (read-only). Extend it with game-specific state:

```csharp
var sessionStateMap = new StateMapBuilder()
    .AddAccumulator("active-player-index", defaultValue: 0.0)
    .AddAccumulator("round-number",        defaultValue: 1.0)
    .Build();

builder.WithSessionStateMap(sessionStateMap);
```

Access the session atom in keyword bodies via the reserved `session` invocation:

```csharp
new Invocation("get-state", new Invocation("session"), new Literal("active-player-index"))
```

---

## Turn Structure

Phases execute in order, repeating each turn. Each phase has an optional `Init` block (runs at phase start) and `Cleanup` block (runs at phase end).

```csharp
// Draw phase: give active player one card from their deck.
var drawPhaseInit = new EffectBlockDef([
    new EffectBlockStep("move-card", [
        new Invocation("top-card-of-deck", new ParameterRef("active-player")),
        new Invocation("hand-of",          new ParameterRef("active-player")),
    ]),
]);

new PhaseDefinition(
    Name:    "draw",
    Init:    drawPhaseInit,
    Cleanup: null)
```

### Action Rules

Action rules declare what actions players may take during a phase. Each `ActionRuleDefinition` names a legal action type. The engine calls `ComputeAvailableActions` to produce the concrete action list for each player.

```csharp
.WithActionRules("main", [
    new ActionRuleDefinition("play-card"),
    new ActionRuleDefinition("attack"),
    new ActionRuleDefinition("pass"),
])
```

---

## State-Based Rules

State-based rules run after every effect block in a fixpoint loop until no rule fires.

```csharp
// Check whether any player has <= 0 life; if so, declare the other the winner.
var checkDefeat = new StateBasedRule(
    Name: "check-defeat",
    Body: new EffectBlockDef([
        new EffectBlockStep("if-player-defeated", []) // game-creator keyword
    ]));

builder.AddStateBasedRule(checkDefeat);
```

**Invariant**: The engine makes no attempt to detect infinite loops. Convergence is the game creator's responsibility. A state-based rule must only fire when it actually changes state.

---

## Trigger Resolution Order

Configures the order in which simultaneous triggers are resolved between actions.

```csharp
builder.WithTriggerResolutionOrder(TriggerResolutionOrder.OldestFirst);  // default
builder.WithTriggerResolutionOrder(TriggerResolutionOrder.NewestFirst);
builder.WithTriggerResolutionOrder(TriggerResolutionOrder.PlayerChoice);
```

---

## InitManifest (Required)

`InitManifest` specifies the initial game state provisioned before the first phase runs. It is mandatory — `InitManifest.Empty` is valid only for games with no pre-built state.

```csharp
var initManifest = new InitManifest(
    Players: [
        new PlayerSpec("alice"),
        new PlayerSpec("bob"),
    ],
    Zones: [
        // Each player gets a deck, hand, and play area zone.
        new ZoneSpec("deck",      ownerPlayerName: "alice"),
        new ZoneSpec("hand",      ownerPlayerName: "alice"),
        new ZoneSpec("play-area", ownerPlayerName: "alice"),
        new ZoneSpec("deck",      ownerPlayerName: "bob"),
        new ZoneSpec("hand",      ownerPlayerName: "bob"),
        new ZoneSpec("play-area", ownerPlayerName: "bob"),
    ],
    Cards: [
        // Alice's starting deck — 20 Stone Golems.
        ..Enumerable.Range(0, 20).Select(_ =>
            new CardSpec("Stone Golem", zoneDefName: "deck", ownerPlayerName: "alice")),
    ]);
```

Provisioning order: session atom → players → zones → cards → session state overrides → player state overrides → card state overrides.

---

## HostManifest (Optional)

`HostManifest` is an append-only layer on top of `InitManifest`. Use it to express host-specific starting state (e.g. tournament-configured starting life totals) without modifying the base definition.

```csharp
var host = new HostManifestBuilder()
    .WithPlayerStateOverride("alice", "life", 25.0)  // tournament variant
    .WithPlayerStateOverride("bob",   "life", 25.0)
    .Build();
```

`HostManifest` entries are applied after `InitManifest` provisioning. They cannot remove or reorder atoms.

---

## Assembling a GameSession

```csharp
var session = GameSession.Create(definition)
    .WithPlayerStrategy("alice", aliceStrategy)
    .WithPlayerStrategy("bob",   bobStrategy)
    .WithRandomSource(new SeededRandom(seed: 42))
    .WithHostManifest(host)     // optional
    .Build();

// Run the game to completion.
GameResult result = await session.RunAsync();
```

- Every registered player must have a corresponding `IPlayerStrategy`.
- A `IRandomSource` is required (use `SeededRandom` for reproducible games).
- `RunAsync` drives the full game loop — phases, player actions, state-based rules, trigger resolution — until `declare-winner` or `declare-draw` is invoked.
