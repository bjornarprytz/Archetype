# Implementation Status

> Last updated: 2026-03-04 (implement-trigger-resolver complete; 19/19 tests green)
> Branch: `back-to-basics`
> All source in `src/` (4 assemblies) + `tests/Archetype.Tests/`.

---

## Assembly Map

| Assembly | Role | Status |
|---|---|---|
| `Archetype.Core` | Immutable data types, interfaces, `BuiltInKeywords` registry | ✅ Complete (Tier 1 subset) |
| `Archetype.Build` | `Kw` factory shorthands for authoring effect blocks | ✅ Complete |
| `Archetype.Engine` | Runtime executor, `GameState`, `EventLog`, `LifetimeChecker`, `TriggerResolver`, `ActionResolver` | ✅ Tier 1–3 complete |
| `Archetype.Text` | Card text renderer | ❌ Not started |

---

## Tier 1 — Core Types

### `KeywordNode` tree ✅
- `ParameterRef`, `Literal`, `Invocation` — immutable records in `Archetype.Core/Keywords.cs`
- `KeywordDefinition`: `Name`, `Parameters` (`ParameterDecl[]`), `ReturnType`, `Description`, `Body`, `PrimitiveSentinel`, `TextTemplate`
- `ParameterDecl` with `TypeName` enum (Atom, Card, Zone, Player, Session, Number, Boolean, ConditionName, PropertyName, ContributionId, Lifetime, EffectBlock, EventRef)
- `EffectBlockDef` / `EffectBlockStep` for block-level composition

### `BuiltInKeywords` registry ✅
- 30 entries: all mutation primitives, property queries, arithmetic, logic, `move-card`, and `event-arg`
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
- `OrderTriggerFirings(firings, TriggerResolutionOrder)`: `OldestFirst` (StaticEffectId ASC, seq ASC), `OldestLast` (desc), `PromptPlayer` falls back to `OldestFirst`
- `FireTrigger(TriggerFiring, ExecutionContext, currentTurn)`: populates `trigger_event` + `EventBindings`, increments `TriggerFireCount` **before** `ExecuteBlock` (D8), manages own action scope, calls `CheckLifetimes`

### `ActionResolver` ✅
- `ResolveAction(EffectBlockDef? primaryBlock, GameState, EventLog, string playerName, int turn)`: full D7 post-action sequence — primary block → CheckLifetimes → RunStateBasedRules → cascade loop (observer check → collect → fire each → CheckLifetimes → RunStateBasedRules → repeat)
- `RunStateBasedRules`: fixpoint loop — all conditions evaluated before any blocks fire in a pass; each SBR block runs in its own action scope to prevent `OpenAction` from discarding uncommitted events
- `ActionResolver.Create(strategies, random, def, observer?)`: convenience factory that wires a shared `BlockExecutor` between `ActionResolver` and `TriggerResolver`
- `IEngineObserver.OnTriggerCascadeAsync` wired; `null` → always `Continue`

### `LifetimeChecker` ✅
- Two-phase `CheckLifetimes(GameState, int currentTurn)`:
  - **Phase 1**: expire active effects whose `TurnTimer`, `TriggerCount`, or `WhileCondition` are satisfied
  - **Phase 2**: re-activate dormant declarative effects whose `WhileCondition` is now true
- `InstantiateStaticEffect` helper used by `GameSession` provisioning (when implemented)

### Static effect lifecycle manager ✅
- Active→Dormant and Dormant→Active transitions in `LifetimeChecker`
- `TriggerCount` expiry: `TriggerFireCount` incremented by `TriggerResolver.FireTrigger` before block execution; next `CheckLifetimes` call expires the effect

---

## Tier 4 — API Surface

### `GameSessionBuilder` ❌ Not started
### `GameSession` ❌ Not started
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
| `TriggerResolution/TriggerResolutionTests.cs` | 9 | ✅ All passing |

**Total: 19 tests, 19 passing.**

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
- `CompositeKeyword_CallingMoveCard_ProducesNestedEventTree_AndTriggerFires` *(refactored: now uses real `ActionResolver`)*

### Trigger resolution (full D7/D8 lifecycle)
- `TriggerFires_WhenEventMatchesKeywordAndNoCondition`
- `TriggerDoesNotFire_WhenConditionIsFalse`
- `TriggerDoesNotFire_WhenNoMatchingEvents`
- `HighWaterMark_PreventsTriggerDoubleFiring`
- `TriggerChain_SecondTriggerFiresInNextBatch`
- `TriggerFiring_FireCount_CausesExpiry_ViaTriggerCount`
- `OldestFirst_OrdersMultipleTriggers`
- `TriggerEvent_Binding_IsAvailableInFiredBlock`
- `NullObserver_DoesNotHaltCascade`

---

## Open Gaps / Known Issues

1. **`PromptChannel` untested** — the `IPlayerStrategy` prompt dispatch exists but no test exercises a mid-block prompt (`choose`, target selection).
2. **Text renderer** — `Archetype.Text` project is scaffolded; no implementation.
3. **`GameSessionBuilder` / `GameSession`** — Tier 4 API surface not started. `ActionResolver` is ready to be consumed.
4. **`ResolveAction` takes `EffectBlockDef?` not `PlayerAction`** — `GameSession` will translate `PlayCard`/`ActivateAbility`/`Pass` → `EffectBlockDef?` before calling `ResolveAction`. This is a Tier 4 responsibility.

### Resolved (implement-trigger-resolver)
- ✅ `TriggerResolver` (`CollectSatisfiedTriggers`, `OrderTriggerFirings`, `FireTrigger`)
- ✅ `ActionResolver` (`ResolveAction`, `RunStateBasedRules`)
- ✅ `event-arg` built-in + `EventRef` type
- ✅ `GameEvent.SelfAndDescendants()` — recursive event tree traversal
- ✅ Composite parent stack in `EventLog` — child events nest, not duplicated
- ✅ Layer 2 test 6.7 now uses real `ActionResolver` (was manual simulation)

### Resolved (add-move-card-primitive reviewer findings)
- ✅ `MoveCard` uses `RequireAtomOfKind` for consistent validation
- ✅ Test covers Zone-as-card guard (`MoveCard_CardArgIsZone_ThrowsEngineException`)
- ✅ Test 6.7b asserts `draw-card` has exactly one child (`move-card`)
- ✅ `EvaluateComposite` uses push/pop composite stack (no event duplication)

---

## Blocked Modules

| Module | Blocked By |
|---|---|
| `GameSession` | Needs `ActionResolver` ✅ — ready to start |
| `GameSessionBuilder` | Needs `GameSession` |
| Text renderer | No blockers — can start any time |
| Persistence | Deferred (D17) |
