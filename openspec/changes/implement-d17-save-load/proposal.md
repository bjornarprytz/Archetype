## Why

D17 (Save/Load) is the last unimplemented piece of the signed-off architecture. All runtime tiers (execution, trigger resolution, action lifecycle, text rendering) are now complete and stable, making this the right moment: the shape of a turn-boundary snapshot is fully determined and will not change.

## What Changes

- **New types in `Archetype.Core`**: `GameStateSnapshot`, `AtomSnapshot`, `ContributionSnapshot` (discriminated union), `StaticEffectSnapshot`, `DormantEffectSnapshot`, `StaticEffectDefRef`, `RngSnapshot`, `BoundValue` (discriminated union, snapshot layer only)
- **`GameDefinition` gains `Id: string`**: required field; `Build()` rejects definitions without one
- **`SeededRandom` reimplemented**: engine-owned deterministic RNG (xoshiro128\*\* or PCG32) replacing `System.Random`; `IRandomSource` interface unchanged
- **`IEngineObserver.OnTurnStart(int turnNumber, GameStateSnapshot snapshot)`**: new method called before each turn's first phase init block; existing null-observer handling covers it without breaking callers
- **`GameStateSnapshotSerializer`**: new class in `Archetype.Engine`; `System.Text.Json` + `[JsonDerivedType]` for `BoundValue`, `ContributionSnapshot`, `StaticEffectSnapshot`
- **`GameSession.RunAsync` wired**: calls `IEngineObserver.OnTurnStart` at the correct turn-boundary save point
- **`GameSessionBuilder.FromSavedState(snapshot)` implemented**: currently throws `NotSupportedException`; seeds `SeededRandom` from snapshot, restores `GameState` from snapshot, skips `ProvisionManifest`

## Capabilities

### New Capabilities
- `save-load`: Turn-boundary game state serialization and deserialization — `GameStateSnapshot` type hierarchy, `BoundValue` discriminated union, `SeededRandom` engine-owned RNG, `GameStateSnapshotSerializer`, and the `FromSavedState` load path in `GameSessionBuilder`

### Modified Capabilities
- `action-lifecycle`: `IEngineObserver` gains `OnTurnStart`; `GameSession.RunAsync` calls it at each turn boundary before the first phase init block

## Non-goals

- Mid-turn (intra-action) save points — explicitly out of scope per D17
- Snapshot migration / versioning beyond `Version: int` field — not needed for initial implementation
- Compression or encryption of snapshot JSON — host responsibility
- `PromptPlayer` trigger ordering via snapshots — not required; prompt state is never mid-block at a save point

## Impact

- **`Archetype.Core`**: new snapshot and `BoundValue` types; `GameDefinition.Id` field
- **`Archetype.Engine`**: `SeededRandom` rewrite; `GameStateSnapshotSerializer`; `GameSession`/`GameSessionBuilder` changes; `IEngineObserver` interface extension
- **`Archetype.Build`**: `GameDefinitionBuilder` must require `Id` at `Build()` time
- **Callers of `IEngineObserver`**: must add `OnTurnStart` stub (or rely on null observer pattern)
- **No changes** to `BlockExecutor`, `KeywordEvaluator`, `TriggerResolver`, `ActionResolver`, or `Archetype.Text`

## Personas

Implementation: **Implementer**. Review: **Reviewer**.
