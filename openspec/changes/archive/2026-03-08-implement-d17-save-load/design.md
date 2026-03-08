## Context

D17 is fully specified in `docs/architecture.md`. This design doc captures the key decisions and their rationale for the implementer. The architecture doc is the authoritative source for type shapes; this doc explains why those shapes were chosen and flags the implementation concerns most likely to go wrong.

**Current state:** `GameSessionBuilder.FromSavedState(snapshot)` exists and throws `NotSupportedException`. `IEngineObserver` exists with one method (`OnTriggerCascadeAsync`). `SeededRandom` wraps `System.Random`. Everything else is new.

## Goals / Non-Goals

**Goals:**
- Turn-boundary save/load: snapshot is valid only between turns (after all cleanup, before next turn's phase init)
- Type-safe snapshot serialization via `System.Text.Json` + `[JsonDerivedType]`
- Engine-owned RNG that produces identical sequences across .NET versions given the same seed
- Full round-trip: a loaded session produces identical game outcomes to a continued session (given the same player responses)

**Non-Goals:**
- Mid-turn save points
- Snapshot migration between `Version` numbers
- Compression, encryption, or host-side persistence strategy (host responsibility)
- `PromptPlayer` trigger ordering — prompt state never exists at a save point

## Decisions

### Decision 1 — Turn boundary as the only save point

**Chosen:** Save only at turn boundaries — after all end-of-turn processing (SBRs, triggers, lifetime checks) but before the next turn's first phase init block.

**Rationale:** At a turn boundary, no block is executing, no `async` continuation is in-flight, and `GameState` is fully settled. This eliminates the need to serialize: execution call stack, block-scope bindings, `ExecutionContext`, scope accumulators, or open `EventLog` frames. The snapshot is pure settled state. For card games where turns are short, resuming from the start of the current turn is acceptable.

**Alternative considered:** Mid-action save points (at every `await`/prompt suspension). Rejected — requires capturing the full async continuation chain, block-scope bindings, and open event log frames. Complexity is prohibitive and the benefit for card games is minimal.

### Decision 2 — `BoundValue` discriminated union (snapshot layer only)

**Chosen:** `GameEvent.BoundArgs` remains `Dictionary<string, object>` at runtime. A `BoundValue` discriminated union is introduced solely for snapshot serialization/deserialization. The serializer converts `object → BoundValue` on write and `BoundValue → object` on read.

**Rationale:** `System.Text.Json`'s default `object` deserialization loses type information (e.g., `long` deserializes as `JsonElement`, not `AtomId`). `BoundValue` gives each value type a stable JSON form. Keeping the runtime type as `object` avoids rippling the change through `KeywordEvaluator`, `MutationDispatch`, `PropertyDispatch`, and all built-in handlers.

**`BoundValue` cases:**
```
NumberValue    { Value : double }
BoolValue      { Value : bool }
StringValue    { Value : string }
AtomIdValue    { Id    : long }
ContribIdValue { Id    : long }
EventRefValue  { SequenceNumber : long }   // EventRef → sequence number; resolved from FinalizedLog on load
CollectionValue { Items : IReadOnlyList<BoundValue> }
```

`EventRef` is serialized as its event's `SequenceNumber`. On load, the deserializer resolves it back to the `GameEvent` by scanning `FinalizedLog`.

### Decision 3 — Engine-owned RNG (`SeededRandom`)

**Chosen:** `SeededRandom` is reimplemented using a simple, engine-owned deterministic algorithm — **xoshiro128\*\*** — rather than `System.Random`. The `IRandomSource` interface is unchanged.

**Rationale:** `System.Random`'s internal algorithm changed in .NET 6 and is not guaranteed stable in future versions. Serializing seed + call count and replaying on a future .NET version could produce different shuffle results. xoshiro128\*\* is a well-known, stable, public-domain algorithm with no .NET dependency. Fast-forwarding by `CallCount` steps at load time is O(CallCount) — negligible at card-game scales (hundreds of calls).

**`RngSnapshot` structure:**
```
RngSnapshot { Seed: long, CallCount: long }
```
At load: construct `SeededRandom(seed)`, advance `CallCount` steps, assign to `ExecutionContext`.

`FromSavedState` reads the seed from the snapshot. The host does NOT call `WithRandomSource` when loading — the builder derives it from the snapshot.

### Decision 4 — `StaticEffectSnapshot` with declarative ref or inline dynamic definition

**Chosen:** `StaticEffectSnapshot` carries either a `DeclarativeRef` (for effects with a `GameDefinition` backing definition) or an inline `DynamicTrigger` (for effects created at runtime by `apply-modifier`/`apply-condition`).

**Rationale:** Declarative effects can be resolved from `GameDefinition` at load time using `(CardDefinitionName, EffectIndex)`. Dynamic effects have no backing definition — their trigger (if any) must be inlined in the snapshot. Contributions owned by a dynamic effect are already in the `ContributionSnapshot` list and are reconstructed independently.

**Exactly one of `DeclarativeRef` / `DynamicTrigger` is non-null.** The implementer should enforce this with a constructor guard.

### Decision 5 — `IEngineObserver.OnTurnStart` as the save-point notification

**Chosen:** `IEngineObserver` gains `Task OnTurnStart(int turnNumber, GameStateSnapshot snapshot)`, called by `GameSession.RunAsync` at the start of each turn before the first phase init block.

**Rationale:** The engine constructs the snapshot at exactly the right moment, removing any ambiguity about when it is valid to save. A `CreateSnapshot()` method on `GameSession` would let callers call it at invalid times (mid-block). Routing through `IEngineObserver` keeps the save contract enforcement inside the engine.

**Null observer:** `null` → no-op (consistent with existing `OnTriggerCascadeAsync` null handling). No stub required.

### Decision 6 — `GameDefinition.Id: string` required

**Chosen:** `GameDefinition` gains `Id: string`. `Build()` throws `DefinitionException` if absent. The snapshot stores `GameDefinitionId`; the loader validates it matches the definition being loaded into.

**Rationale:** Without an identity check, a snapshot from game definition A could be loaded into game definition B, producing undefined behavior. This is a load-time guard, not a runtime guard — by the time the session runs, the check has passed.

### Decision 7 — `ModifierIndex` / `ConditionIndex` reconstructed on load, not stored

**Chosen:** `AtomSnapshot` does not store `ModifierIndex` or `ConditionIndex`. They are reconstructed by iterating `Contributions` after all contributions are loaded.

**Rationale:** These indices are derived from the contribution registry — storing them redundantly risks desync. Reconstruction is O(contributions) and happens once at load time.

## Risks / Trade-offs

**[Risk] `EventRef` resolution on load requires a linear scan of `FinalizedLog`** → Mitigation: at card-game scale, `FinalizedLog` contains at most thousands of events. Build a `Dictionary<long, GameEvent>` keyed by `SequenceNumber` during deserialization and use it for all `EventRefValue` resolutions.

**[Risk] `[JsonDerivedType]` requires .NET 7+** → Mitigation: the project already targets a modern .NET version (WASM/Godot); this is not a new constraint.

**[Risk] `SeededRandom` fast-forward is O(CallCount)** → Mitigation: acceptable at card-game scale. If it ever becomes a concern, xoshiro128\*\* supports O(1) jump-ahead; document that as a future option.

**[Risk] `GameDefinition.Id` is a breaking change for existing test fixtures** → Mitigation: `GameDefinitionBuilder` tests that call `Build()` without setting `Id` will fail. All existing test builders must be updated to set a dummy `Id`. Scope this as a required task.

**[Risk] `IEngineObserver.OnTurnStart` is a breaking interface change** → Mitigation: the only existing implementations are test doubles (mock observers in `TriggerResolutionTests`, `StateBasedRuleTests`, `GameSessionTests`). All must add the stub method. Scope as a required task.

## Open Questions

None — D17 in `docs/architecture.md` resolves all design questions for this feature.
