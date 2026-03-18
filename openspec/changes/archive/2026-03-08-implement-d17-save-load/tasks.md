## 1. Breaking Changes — Update Existing Code

- [x] 1.1 Add `Id: string` to `GameDefinition` record in `Archetype.Core/GameDefinition.cs`; update `GameDefinitionBuilder.Build()` to throw `DefinitionException` if `Id` is null or empty
- [x] 1.2 Add `Task OnTurnStart(int turnNumber, GameStateSnapshot snapshot)` to `IEngineObserver` in `Archetype.Core`; update all test double implementations (`MockObserver` or equivalent in test files) to add an empty stub
- [x] 1.3 Update all `GameDefinitionBuilder` usages in existing tests to set a non-empty `Id` (e.g., `WithId("test-game")`) so they continue to pass after the `Build()` validation is added

## 2. Core Snapshot Types

- [x] 2.1 Add `BoundValue` discriminated union to `Archetype.Core` — cases: `NumberValue`, `BoolValue`, `StringValue`, `AtomIdValue`, `ContribIdValue`, `EventRefValue { SequenceNumber: long }`, `CollectionValue { Items: IReadOnlyList<BoundValue> }` — annotated with `[JsonDerivedType]` for `System.Text.Json`
- [x] 2.2 Add `RngSnapshot { Seed: long, CallCount: long }` record to `Archetype.Core`
- [x] 2.3 Add `StaticEffectDefRef { CardDefinitionName: string, EffectIndex: int }` record to `Archetype.Core`
- [x] 2.4 Add `DormantEffectSnapshot { OwnerAtomId: AtomId, CardDefinitionName: string, EffectIndex: int }` record to `Archetype.Core`
- [x] 2.5 Add `AtomSnapshot { Id, Kind, RefName, OwnerName, ZoneId, Accumulators }` record to `Archetype.Core` (no `ModifierIndex`/`ConditionIndex` — reconstructed on load)
- [x] 2.6 Add `ContributionSnapshot` discriminated union to `Archetype.Core` — cases: `ModifierContributionSnapshot`, `ConditionContributionSnapshot` — annotated with `[JsonDerivedType]`
- [x] 2.7 Add `StaticEffectSnapshot { Id, Origin, OwnerAtomId, LifetimeSpec, TriggerFireCount, TriggerHighWaterMark, OwnedContributions, DeclarativeRef?, DynamicTrigger? }` to `Archetype.Core`; enforce exactly one of `DeclarativeRef`/`DynamicTrigger` non-null in constructor
- [x] 2.8 Add `GameStateSnapshot { Version, GameDefinitionId, NextAtomId, NextContributionId, NextStaticEffectId, NextScopeId, Atoms, SessionAtomId, Contributions, ActiveStaticEffects, DormantEffects, FinalizedLog, Rng }` record to `Archetype.Core`

## 3. SeededRandom Reimplementation

- [x] 3.1 Reimplement `SeededRandom` in `Archetype.Engine` using xoshiro128\*\* (replace `System.Random` dependency); document the chosen algorithm in an XML summary comment; `IRandomSource` interface is unchanged
- [x] 3.2 Add `SeededRandom(long seed, long callCount)` constructor (or equivalent fast-forward factory) that advances `callCount` steps from the given seed — used by `FromSavedState` load path

## 4. Snapshot Serializer

- [x] 4.1 Implement `GameStateSnapshotSerializer` in `Archetype.Engine` with `static string Serialize(GameStateSnapshot)` and `static GameStateSnapshot Deserialize(string json)` using `System.Text.Json` with `[JsonDerivedType]` on `BoundValue`, `ContributionSnapshot`, and `StaticEffectSnapshot`
- [x] 4.2 Implement `object` → `BoundValue` conversion in the serializer: map `double` → `NumberValue`, `bool` → `BoolValue`, `string` → `StringValue`, `AtomId` → `AtomIdValue`, `ContributionId` → `ContribIdValue`, `EventRef` → `EventRefValue { SequenceNumber }`, `IReadOnlyList<object>` → `CollectionValue`
- [x] 4.3 Implement `BoundValue` → `object` conversion on deserialization; for `EventRefValue`, build a `Dictionary<long, GameEvent>` from `FinalizedLog` and resolve by `SequenceNumber`

## 5. GameState Snapshot Capture

- [x] 5.1 Implement `GameState.ToSnapshot()` (internal) — captures `NextAtomId`, `NextContributionId`, `NextStaticEffectId`, `NextScopeId`, `SessionAtomId`, all atoms as `AtomSnapshot`, all contributions as `ContributionSnapshot`, all active static effects as `StaticEffectSnapshot`, all dormant effects as `DormantEffectSnapshot`
- [x] 5.2 Implement `GameState.LoadFromSnapshot(GameStateSnapshot, GameDefinition)` (internal) — restores all fields, resolves `DeclarativeRef` to `StaticEffectDef` from `GameDefinition`, reconstructs `ModifierIndex`/`ConditionIndex` by iterating contributions

## 6. GameSession and GameSessionBuilder Wiring

- [x] 6.1 Wire `IEngineObserver.OnTurnStart(turnNumber, snapshot)` in `GameSession.RunAsync` — call it at the start of each turn (after prior-turn cleanup, before the first phase init block); construct snapshot via `GameState.ToSnapshot()` + current `EventLog.FinalizedLog` + `SeededRandom.Snapshot()`
- [x] 6.2 Implement `GameSessionBuilder.FromSavedState(GameStateSnapshot snapshot)` — store snapshot; `Build()` validates `snapshot.GameDefinitionId == definition.Id` (throw `DefinitionException` on mismatch); constructs `SeededRandom` from `snapshot.Rng`; `Build()` does NOT require `WithRandomSource` when snapshot is set
- [x] 6.3 In `GameSession.RunAsync`, detect the `FromSavedState` path and skip `ProvisionSession`/`ProvisionManifest`; instead call `GameState.LoadFromSnapshot` and begin execution at the snapshot's turn number

## 7. Tests

- [x] 7.1 `SeededRandom_SameSeed_ProducesSameSequence` — two instances with the same seed produce identical values for N calls
- [x] 7.2 `SeededRandom_FastForward_ProducesCorrectNextValue` — instance constructed with `(seed, callCount=5)` produces the same value as a fresh instance that has made 5 prior calls
- [x] 7.3 `Snapshot_RoundTrip_PreservesAtomState` — session saved and deserialized; atom `ZoneId`, `Kind`, `Accumulators` match
- [x] 7.4 `Snapshot_RoundTrip_PreservesActiveStaticEffects` — `TriggerFireCount`, `TriggerHighWaterMark`, `OwnedContributions` survive serialization
- [x] 7.5 `Snapshot_RoundTrip_BoundArgs_AtomIdPreservesType` — `AtomId` in `BoundArgs` deserializes as `AtomId`, not `long` or `JsonElement`
- [x] 7.6 `Snapshot_RoundTrip_BoundArgs_EventRefResolvesCorrectly` — `EventRef` in `BoundArgs` deserializes to the correct `GameEvent` by `SequenceNumber`
- [x] 7.7 `FromSavedState_ResumesAtCorrectTurn` — session saved at turn boundary N, loaded session begins at turn N (observer's `OnTurnStart` receives `turnNumber == N`)
- [x] 7.8 `FromSavedState_GameDefinitionIdMismatch_ThrowsDefinitionException` — snapshot with wrong `GameDefinitionId` causes `Build()` to throw
- [x] 7.9 `FromSavedState_DoesNotRequireWithRandomSource` — `Build()` succeeds without `WithRandomSource` when `FromSavedState` is set
- [x] 7.10 `OnTurnStart_CalledBeforeFirstPhaseInit` — observer records call order; asserts `OnTurnStart(1)` fires before any phase init event appears in the event log
- [x] 7.11 `GameDefinitionBuilder_Build_ThrowsWhenIdMissing` — `Build()` without `WithId(...)` throws `DefinitionException`
- [x] 7.12 `ModifierIndex_ReconstructedCorrectly_AfterLoad` — atom with modifier contributions; after `LoadFromSnapshot`, `ModifierIndex` contains the correct contributions
- [x] 7.13 `ManifestProvisionedCondition_SurvivesSnapshotRoundTrip` — regression test for BLOCKER 1: conditions applied via `ApplyConditions` survive snapshot round-trip and are queryable after load

## 8. Reviewer Checks

- [x] 8.1 Reviewer: verify `SeededRandom` has zero dependency on `System.Random` and that the algorithm is documented by name in XML summary
- [x] 8.2 Reviewer: verify `StaticEffectSnapshot` constructor enforces exactly one of `DeclarativeRef`/`DynamicTrigger` non-null
- [x] 8.3 Reviewer: verify `EventRefValue` deserialization builds the `SequenceNumber → GameEvent` dictionary once (not per-event) and correctly handles missing sequence numbers with a clear exception message
- [x] 8.4 Reviewer: verify `OnTurnStart` is called BEFORE the first phase init block of the turn (not after)
- [x] 8.5 Reviewer: verify `FromSavedState` path calls `LoadFromSnapshot` instead of `ProvisionSession`/`ProvisionManifest`, and that the turn counter starts at the snapshot's turn (not 1)
- [x] 8.6 Reviewer: verify all 13 tests pass and all pre-existing tests (72) still pass after the breaking changes to `IEngineObserver` and `GameDefinition.Id`
