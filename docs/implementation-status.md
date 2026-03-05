# Implementation Status

> Last updated: 2026-03-05 (D19 review blockers resolved — 45/45 tests passing)
> Branch: `back-to-basics`
> All source in `src/` (4 assemblies) + `tests/Archetype.Tests/`.

---

## Assembly Map

| Assembly | Role | Status |
|---|---|---|
| `Archetype.Core` | Immutable data types, interfaces, `BuiltInKeywords` registry | ✅ Complete (Tier 1 subset) |
| `Archetype.Build` | `Kw` factory shorthands for authoring effect blocks | ✅ Complete |
| `Archetype.Engine` | Runtime executor, `GameState`, `EventLog`, `LifetimeChecker`, `TriggerResolver`, `ActionResolver`, `GameSession`, `GameSessionBuilder` | ✅ Tier 1–4 complete (Text renderer excepted) |
| `Archetype.Text` | Card text renderer | ❌ Not started |

---

## Tier 1 — Core Types

### `KeywordNode` tree ✅
- `ParameterRef`, `Literal`, `Invocation` — immutable records in `Archetype.Core/Keywords.cs`
- `KeywordDefinition`: `Name`, `Parameters` (`ParameterDecl[]`), `ReturnType`, `Description`, `Body`, `PrimitiveSentinel`, `TextTemplate`
- `ParameterDecl` with `TypeName` enum (Atom, Card, Zone, Player, Session, Number, Boolean, ConditionName, PropertyName, ContributionId, Lifetime, EffectBlock, EventRef)
- `EffectBlockDef` / `EffectBlockStep` for block-level composition

### `BuiltInKeywords` registry ✅
- 34 entries: all mutation primitives, property queries, arithmetic, logic, `move-card`, `event-arg`, `declare-winner`, `declare-draw`, `player-by-name`, `get-atoms-in-zone`
- `BuiltInKeywords.All` list enforces sync invariant with Engine dispatch

### `EventLog` and `GameEvent` ✅
- `GameEvent` in `Archetype.Core/Events.cs`: `KeywordName`, `BoundArgs`, `SequenceNumber`, `Children` (tree structure), `SelfAndDescendants()` recursive traversal
- `EventRef` in `Archetype.Core/Events.cs`: first-class runtime value wrapping a `GameEvent`; used as `trigger_event` binding in triggered blocks
- `EventLog` in `Archetype.Engine/EventLog.cs`: append-only, frame-stack scope hierarchy (`OpenBlock`/`CloseBlock`, `OpenAction`/`CloseAction`, `OpenTurn`/`CloseTurn`); exposes `ThisBlock`, `ThisAction`, `ThisTurn`, `ThisGame` (all recursive via `SelfAndDescendants`)
- Composite parent stack (`PushCompositeParent`/`PopCompositeParent`): nests child events inside composite keyword wrappers

### `GameState` ✅
- `AtomSnapshot` (internal): mutable runtime atom state — `Kind`, `ZoneId`, `OwnerId`, `Accumulators`, `Modifiers`, `ConditionIndex`
- `GameState` (internal): atom registry, `NextContributionId`, `NextStaticEffectId`, `ActiveStaticEffects`, `DormantDeclarativeEffects`; implements `IGameStateReadable` (public interface in Core)

### Contribution types ✅
- `ContributionId`, `StaticEffectId`, `ModifierContribution`, `ConditionContribution`
- `LifetimeSpec` with `TurnTimer`, `TriggerCount`, `WhileCondition` lifetime conditions
- `StaticEffectDef`, `TriggerDefinition`, `TriggerScope`, `EventParamDecl`, `EventBinding`, `StaticEffect`, `DormantDeclarativeEffect`

### `GameDefinition` ✅
- `CardDefinition`, `ZoneDefinition`, `PhaseDefinition`, `ActionRuleDefinition`, `StateBasedRule`, `CardSet`, `PlayerDefinition`, `GameDefinition`
- `CardDefinition.ActivationCondition: KeywordNode?` (D19) — optional pure condition evaluated before offering PlayCard; card atom injected as `source`
- `GameDefinition.PlayableZoneNames: IReadOnlyList<string>?` (D19) — zone definition names from which cards may be played; `null` = no zone filter

### Interfaces ✅
- `IPlayerStrategy`, `IRandomSource`, `IEngineObserver` (`OnTriggerCascadeAsync` → `CascadeDirective`)
- Player action types: `PlayCard`, `ActivateAbility`, `Pass`
- Prompt types: `PromptContext`, `PromptResponse`, `ChoicePrompt`, `TriggerOrderPrompt`
- `GameStateView` (public read-only view), `IGameStateReadable`

---

## Tier 2 — Execution

### `ExecutionContext` ✅
- Internal; carries `GameState`, `EventLog`, `Bindings`, `Strategies`, `RandomSource`, `Definition`, `ActivePlayerName`
- `CreateChildActionContext(Dictionary<string,object> extraBindings)` for trigger firing
- `TriggerEvaluationContext` removed (was dead code; replaced by plain `Dictionary<string,object>` with `"source"` binding)

### `BlockExecutor` ✅
- `ExecuteBlock(EffectBlockDef, ExecutionContext) → Task`
- Manages block-scope open/close on `EventLog`
- Dispatches each step through `KeywordEvaluator`
- `EvaluateCondition(KeywordNode, GameState, Dictionary<string,object>) → bool` — pure-mode evaluation for while-conditions and trigger conditions

### `KeywordEvaluator` ✅
- `EvaluateNode` — resolves `ParameterRef`, `Literal`, `Invocation` recursively
- `EvaluateInvocation` — dispatches through `MutationDispatch` or `PropertyDispatch`
- `EvaluateComposite` — expands `KeywordDefinition.Body` with parameter bindings; uses composite parent stack so child events nest correctly
- `ApplyParameterModifications` — applies active `ParameterModification` static effects

### Built-in dispatch ✅
- `MutationDispatch` / `PropertyDispatch` in `Archetype.Engine/Dispatch.cs`
- `BuiltInHandlers` in `Archetype.Engine/BuiltInHandlers.cs`: all 30 built-ins registered
- **`move-card`** handler: validates card/zone kinds, captures `origin`, updates `card.ZoneId`, logs event
- **`event-arg`** handler: extracts a named arg from an `EventRef`; used in trigger-fired blocks
- `AssertSync()` startup check: every `BuiltInKeywords.All` name has a handler; no extra handlers

### `Kw` factory ✅
- `Archetype.Build/Kw.cs`: typed shorthand for every built-in keyword
- `Kw.MoveCard(card, destination)`, `Kw.EventArg(ev, name)` added

### `PromptChannel` ⚠️ Partial
- `IPlayerStrategy`-based prompt dispatch wired in `ExecutionContext`
- `TaskCompletionSource<T>` suspension pattern not yet tested end-to-end

---

## Tier 3 — Rules Engine

### `TriggerResolver` ✅
- `TriggerFiring` record: `(StaticEffect Effect, GameEvent Event)`
- `CollectSatisfiedTriggers(GameState, EventLog)`: high-water-mark scan, condition eval, advances mark past ALL candidates (matched or not)
- `BuildEventParamBindings`: always includes `["source"] = ownerAtom` so trigger conditions can reference `ParameterRef("source")` (D8/D13 addendum); also maps game-creator `EventParams`
- `OrderTriggerFirings(firings, TriggerResolutionOrder)`: `OldestFirst` (StaticEffectId ASC, seq ASC), `OldestLast` (desc), `PromptPlayer` falls back to `OldestFirst`
- `FireTrigger(TriggerFiring, ExecutionContext, currentTurn)`: populates `trigger_event` + `EventBindings`, increments `TriggerFireCount` **before** `ExecuteBlock` (D8), manages own action scope, calls `CheckLifetimes`

### `ActionResolver` ✅
- `ResolveAction(EffectBlockDef? primaryBlock, GameState, EventLog, string playerName, int turn)`: full D7 post-action sequence — primary block → CheckLifetimes → RunStateBasedRules → cascade loop (observer check → collect → fire each → CheckLifetimes → RunStateBasedRules → repeat)
- `RunStateBasedRules`: fixpoint loop — all conditions snapshotted (`.ToList()`) before any blocks fire in a pass; each SBR block runs in its own action scope to prevent `OpenAction` from discarding uncommitted events
- `ActionResolver.Create(strategies, random, def, observer?)`: convenience factory that creates two `BlockExecutor` instances (one shared with `TriggerResolver`, one for primary/SBR blocks); `AssertSync` runs twice per `Create` call
- `IEngineObserver.OnTriggerCascadeAsync` wired; `null` → always `Continue`

### `LifetimeChecker` ✅
- Two-phase `CheckLifetimes(GameState, int currentTurn)`:
  - **Phase 1**: expire active effects whose `TurnTimer`, `TriggerCount`, or `WhileCondition` are satisfied
  - **Phase 2**: re-activate dormant declarative effects whose `WhileCondition` is now true
- `InstantiateStaticEffect` helper used by `GameSession` provisioning
- `ProvisionDeclarativeEffect(StaticEffectDef, AtomId, GameState)`: evaluates while-condition; activates immediately if absent/true, adds to `DormantDeclarativeEffects` if false

### Static effect lifecycle manager ✅
- Active→Dormant and Dormant→Active transitions in `LifetimeChecker`
- `TriggerCount` expiry: `TriggerFireCount` incremented by `TriggerResolver.FireTrigger` before block execution; next `CheckLifetimes` call expires the effect

---

## Tier 4 — API Surface

### `GameSessionBuilder` ✅
- Fluent builder in `Archetype.Engine/GameSession.cs`
- `WithPlayerStrategy(name, strategy)` — registers one strategy per defined player; validated at `Build()` time
- `WithRandomSource(source)` — required; `Build()` throws if absent
- `WithObserver(observer)` — optional cascade observer
- `UseDefaultInit()` / `WithInitManifest(manifest)` — manifest selection; last call wins
- `FromSavedState(snapshot)` — throws `NotSupportedException` (D17 deferred)
- `Build()` — validates all players have strategies; no extra strategies for undefined players

### `GameSession` ✅
- `static GameSessionBuilder Create(GameDefinition)` — factory entry point
- `async Task<GameResult> RunAsync(CancellationToken)` — provisions manifest, drives the phase/turn loop
- **Turn loop**: advances `turn-number` and `phase-index` accumulators; each phase runs Init → ActionWindow → Cleanup
- **`GameIsOver` propagation**: checks `_state.GameIsOver` after every `ResolveAction` call; also short-circuits mid-cascade in `ActionResolver`
- **Round-robin active player**: `(turn-1) % playerCount` over `PlayerDefinitions` insertion order
- **`ProvisionSession()`**: creates session atom (turn-number=1, phase-index=0), player atoms, calls `ProvisionManifest`
- **`ProvisionManifest(InitManifest)`**: zones → cards (with `ProvisionDeclarativeEffect`) → card overrides → player overrides
- **`ComputeAvailableActions`**: two independent passes — Pass 1 (PlayCard, zone-filtered + activation condition); Pass 2 (abilities, all owned cards regardless of zone); cost pre-flight deferred (D19 D-C)
- **`TranslatePlayerAction`**: `PlayCard` → definition's `PrimaryEffect`; `ActivateAbility` → named `AdditionalEffects` body; `Pass` → null

### `get-atoms-in-zone` built-in ✅
- `get-atoms-in-zone(zone: Zone) → Collection` — pure read; returns all atoms whose `ZoneId` matches the given zone atom
- Used by `ComputeAvailableActions` (via equivalent internal `GetAllAtoms()` read) and composable in keyword trees
- `GetAllAtoms()` added to `GameState` to support the handler
- `BlockExecutor.EvaluateProperty(node, state, bindings?)` added (internal) for testing non-boolean primitive results

### `ComputeAvailableActions` ✅
- Zone filter (D19 step 2): checks zone definition name ∈ `PlayableZoneNames` **and** zone owner == active player; prevents cross-player zone exploitation
- Activation condition (D19): `source` injected manually per D13 (no `StaticEffect` wrapper on this path)
- Ability loop (D19 step 4): independent second pass over all owned cards, ignores zone membership entirely; zone restrictions expressed via ability's own `ActivationCondition`
- Tests: 9 tests covering get-atoms-in-zone (3), zone filter, activation condition, source injection, ability-in-non-playable-zone, zone-owner restriction, pass-always-present
- **Open gap (pre-existing)**: runtime-created atoms (`create-card`/`create-zone`) are invisible to `ComputeAvailableActions` — `_atomDefinitionNames` is only populated during `ProvisionManifest`

### `declare-winner` / `declare-draw` built-ins ✅
- Architecture gap: D14 says "repeat until state-based rule produces outcome" but didn't specify mechanism
- **Resolution**: added `declare-winner(player)` and `declare-draw()` as primitives (33 total built-ins)
- `GameState.DeclareOutcome(string?)`: first-call-wins; sets `GameIsOver` and `PendingWinner`
- `GameState.RegisterPlayerName` / `TryGetPlayerName` / `TryGetPlayerAtomByName`: bidirectional player name registry
- `RunStateBasedRules` exits immediately at loop top when `GameIsOver` is true (prevents infinite loop when an always-true SBR fired the terminal rule)
- Cascade loop breaks when `GameIsOver` is true; per-trigger `GameIsOver` check stops mid-batch firing

### `player-by-name` built-in ✅
- `player-by-name(name: PropertyName) → Player` — reverse-looks up a player atom from its registered name string
- The idiomatic way to pass a player reference to `declare-winner` from a static `KeywordNode` tree (where the atom ID is not known at definition time)
- Added to `BuiltInKeywords.All` (33 total), `BuiltInHandlers`, and `Kw.PlayerByName`
- End-to-end test `DeclareWinner_ViaPlayerByName_ReturnsCorrectWinnerName` asserts `GameResult.Winner == "p1"` (resolves PR #4 blocker)

### Text renderer ❌ Not started (`Archetype.Text` project scaffolded but empty)

---

## Tier 5 — Persistence

### `GameStateSnapshot` ❌ Deferred (per D17)

---

## Test Coverage

| File | Tests | Status |
|---|---|---|
| `MoveCard/MoveCardLayer1Tests.cs` | 7 | ✅ All passing |
| `MoveCard/MoveCardLayer2Tests.cs` | 3 | ✅ All passing |
| `TriggerResolution/TriggerResolutionTests.cs` | 10 | ✅ All passing |
| `StateBasedRules/StateBasedRuleTests.cs` | 4 | ✅ All passing |
| `GameSession/GameSessionTests.cs` | 12 | ✅ All passing |
| `ComputeAvailableActions/ComputeAvailableActionsTests.cs` | 7 | ⚠️ Passing but missing tests for D19 steps 2 & 4 invariants |

**Total: 43 tests, 43 passing. (D19 invariant tests missing — see blockers.)**

### Layer 1 (unit, isolated state)
- `MoveCard_UpdatesCardZoneId_ToDestination`
- `MoveCard_LogsEvent_WithCorrectCardOriginAndDestination`
- `MoveCard_OriginReflectsZoneAtCallTime_NotAfterSubsequentMove`
- `MoveCard_SelfMove_CompletesWithoutError_AndLogsEvent`
- `MoveCard_InvalidDestination_NonExistentAtom_ThrowsEngineException`
- `MoveCard_CardArgIsZone_ThrowsEngineException`
- `MoveCard_DestinationIsCard_NotZone_ThrowsEngineException`

### Layer 2 (block integration)
- `CheckLifetimes_ExpiresActiveEffect_WhenCardLeavesWhileConditionZone`
- `CheckLifetimes_ActivatesDormantEffect_WhenCardEntersWhileConditionZone`
- `CompositeKeyword_CallingMoveCard_ProducesNestedEventTree_AndTriggerFires`

### GameSession end-to-end (Layer 3)
- `Builder_ThrowsWhenRandomSourceMissing`
- `Builder_ThrowsWhenPlayerStrategyMissing`
- `Builder_ThrowsForUnknownPlayerName`
- `DeclareWinner_InPhaseInit_ReturnsResultWithCorrectWinner` — SBR fires draw on turn 1
- `DeclareDraw_ViaSBR_ReturnsDrawResult` — always-true SBR with declare-draw; validates GameIsOver terminates the loop
- `FinalLog_ContainsEvents_FromWinningTurn` — phase Init block runs, SBR fires draw; events present in FinalLog
- `Manifest_ProvisionesPlayerAndZones_GameStateHasCorrectAtoms` — zone + player state provisioning; SBR ends game
- `DeclareWinner_PlayerNameResolvedCorrectly` — card provisioning + manifest + declare-draw
- `MultiTurn_GameRunsTwoTurns_BeforeDeclareDraw` — SBR condition on turn-number ≥ 2; player queues 2 passes
- `PlayCard_Action_ExecutesPrimaryEffect` — LambdaPlayerStrategy plays card; SBR fires when score ≥ 1; event logged
- `FromSavedState_ThrowsNotSupportedException`

### Trigger resolution (full D7/D8 lifecycle)
- `TriggerFires_WhenEventMatchesKeywordAndNoCondition`
- `TriggerDoesNotFire_WhenConditionIsFalse`
- `TriggerDoesNotFire_WhenNoMatchingEvents`
- `HighWaterMark_PreventsTriggerDoubleFiring`
- `TriggerChain_SecondTriggerFiresInNextBatch`
- `TriggerFiring_FireCount_CausesExpiry_ViaTriggerCount`
- `OldestFirst_OrdersMultipleTriggers`
- `TriggerEvent_Binding_IsAvailableInFiredBlock` *(rewritten: fired block calls `event-arg` and uses AtomId as modify-accumulator target)*
- `NullObserver_DoesNotHaltCascade`
- `TriggerCondition_CanReferenceSourceBinding` *(new: verifies `["source"]` binding in condition evaluation)*

### State-based rules (fixpoint loop, D7)
- `SBR_Fires_WhenConditionIsTrue`
- `SBR_DoesNotFire_WhenConditionIsFalse`
- `SBR_FixpointLoop_SBR2_FiresInSecondPass`
- `SBR_AllConditionsSnapshotted_BeforeAnyBlockFires`

---

## Open Gaps / Known Issues

1. **`PromptChannel` untested** — the `IPlayerStrategy` prompt dispatch exists but no test exercises a mid-block prompt (`choose`, target selection).
2. **Text renderer** — `Archetype.Text` project is scaffolded; no implementation.
3. **`ComputeAvailableActions` cost pre-flight deferred** — zone filtering and activation-condition evaluation are now implemented (D19). Cost pre-flight (checking whether a card's cost can be paid) remains deferred per the D19 design doc; no current game definition requires it.
4. **`declare-winner` architecture gap** — D14 didn't specify how a game-ending primitive signals `GameSession`. Resolved by adding `declare-winner(player)` / `declare-draw()` built-ins; decision documented here and in source XML docs. Architecture doc should be updated to ratify this (open item for architect).

---

## Blocked Modules

| Module | Blocked By |
|---|---|
| Text renderer | No blockers — can start any time |
| Persistence | Deferred (D17) |

---

## Resolved Issues

### Tier 4 — GameSession / GameSessionBuilder (2026-03-05) — rework complete
- ✅ `GameSessionBuilder`: fluent builder with full player-strategy validation
- ✅ `GameSession.RunAsync`: turn/phase loop, `GameIsOver` propagation, manifest provisioning
- ✅ `declare-winner` / `declare-draw` end-to-end verified; `GameResult.Winner == "p1"` asserted
- ✅ `player-by-name(name)` primitive added — resolves the "how to pass a player atom to declare-winner" gap
- ✅ `LifetimeChecker.ProvisionDeclarativeEffect`: while-condition-aware provisioning used by manifest
- ✅ `RunStateBasedRules` exits early on `GameIsOver` (prevents infinite loop from always-true terminal SBRs)
- ✅ Cascade loop and per-trigger checks also break on `GameIsOver`
- ✅ `Kw.DeclareWinner` / `Kw.DeclareDraw` / `Kw.PlayerByName` shorthands added to `Archetype.Build`
- ✅ `GameStateView` constructor made `public` (was `internal`; needed cross-assembly from Engine)
- ✅ 36/36 tests passing

### implement-trigger-resolver rework (2026-03-04)
- ✅ **BLOCKER resolved**: `RunStateBasedRules` now has 4 tests covering all D7 fixpoint invariants
- ✅ **MINOR.1 resolved**: `source` binding always populated in `BuildEventParamBindings`; `TriggerEvaluationContext` removed
- ✅ **MINOR.2 resolved**: Test 6.8 now calls `event-arg(trigger_event, "card")` and uses the result as modify-accumulator's atom arg
- ✅ **MINOR.3 resolved**: `ActionResolver.Create` doc comment correctly describes two-executor behaviour

### implement-trigger-resolver (2026-03-04)
- ✅ `TriggerResolver` (`CollectSatisfiedTriggers`, `OrderTriggerFirings`, `FireTrigger`)
- ✅ `ActionResolver` (`ResolveAction`, `RunStateBasedRules`)
- ✅ `event-arg` built-in + `EventRef` type
- ✅ `GameEvent.SelfAndDescendants()` — recursive event tree traversal
- ✅ Composite parent stack in `EventLog` — child events nest, not duplicated
- ✅ Layer 2 test 6.7 uses real `ActionResolver`

### add-move-card-primitive (2026-03-04)
- ✅ `MoveCard` uses `RequireAtomOfKind` for consistent validation
- ✅ Test covers Zone-as-card guard (`MoveCard_CardArgIsZone_ThrowsEngineException`)
- ✅ Test 6.7b asserts `draw-card` has exactly one child (`move-card`)
- ✅ `EvaluateComposite` uses push/pop composite stack (no event duplication)
