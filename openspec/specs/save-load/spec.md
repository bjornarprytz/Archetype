## ADDED Requirements

### Requirement: GameStateSnapshot captures complete turn-boundary state
The engine SHALL produce a `GameStateSnapshot` that fully describes the settled game state at a turn boundary — after all end-of-turn processing but before the next turn's first phase init block. The snapshot SHALL be sufficient to resume the session from the start of that turn with identical game outcomes given the same player responses.

#### Scenario: Snapshot is produced before turn 1's first phase init
- **WHEN** `GameSession.RunAsync` is called
- **THEN** `IEngineObserver.OnTurnStart(1, snapshot)` is called before any phase init block for turn 1 executes
- **THEN** the snapshot contains zero atoms with no contributions and an empty finalized log (pre-play state)

#### Scenario: Snapshot is produced at the start of each subsequent turn
- **WHEN** all end-of-turn processing for turn N completes (SBRs settled, all triggers resolved, all lifetime checks run)
- **THEN** `IEngineObserver.OnTurnStart(N+1, snapshot)` is called before any phase init block for turn N+1 executes
- **THEN** the snapshot reflects the fully settled state after turn N

#### Scenario: Null observer receives no snapshot
- **WHEN** `GameSession` is constructed with a null `IEngineObserver`
- **THEN** the session runs normally and `OnTurnStart` is a no-op

### Requirement: GameStateSnapshot round-trips through JSON without data loss
The engine SHALL provide `GameStateSnapshotSerializer` with `Serialize` and `Deserialize` methods. A snapshot serialized to JSON and deserialized SHALL produce an equivalent `GameStateSnapshot` with all fields intact.

#### Scenario: Serialize and deserialize preserves atom state
- **WHEN** a snapshot is serialized to JSON and deserialized
- **THEN** the deserialized snapshot contains the same atoms with the same `ZoneId`, `Kind`, `OwnerId`, `Accumulators`, and `RefName`

#### Scenario: Serialize and deserialize preserves active static effects
- **WHEN** a snapshot with active static effects is serialized and deserialized
- **THEN** all `ActiveStaticEffects` are present with the same `Id`, `TriggerFireCount`, `TriggerHighWaterMark`, and `OwnedContributions`

#### Scenario: Serialize and deserialize preserves BoundArgs type information
- **WHEN** a `GameEvent` in `FinalizedLog` has `BoundArgs` containing an `AtomId` value
- **AND** the snapshot is serialized and deserialized
- **THEN** the deserialized `BoundArgs` entry is an `AtomId` (not a plain `long` or `JsonElement`)

#### Scenario: EventRef in BoundArgs resolves to the correct GameEvent on load
- **WHEN** a `GameEvent`'s `BoundArgs` contains an `EventRef` value referencing another event with `SequenceNumber` S
- **AND** the snapshot is serialized and deserialized
- **THEN** the deserialized `EventRef` references the `GameEvent` in `FinalizedLog` with `SequenceNumber` S

### Requirement: FromSavedState resumes the session from the snapshot turn
The engine SHALL support `GameSessionBuilder.FromSavedState(snapshot)` to construct a session that begins at the turn captured in the snapshot, skipping manifest provisioning.

#### Scenario: Session loaded from snapshot begins at the correct turn
- **WHEN** a session is saved at the start of turn 3 (snapshot produced before turn 3's phase init)
- **AND** a new session is constructed with `FromSavedState(snapshot)`
- **THEN** the session begins execution at turn 3's first phase init block

#### Scenario: GameDefinitionId mismatch is rejected at load time
- **WHEN** `FromSavedState(snapshot)` is called with a snapshot whose `GameDefinitionId` does not match the `GameDefinition.Id` being built into
- **THEN** `Build()` throws `DefinitionException` with a message identifying the mismatch

#### Scenario: FromSavedState does not require WithRandomSource
- **WHEN** `GameSessionBuilder` is used with `FromSavedState(snapshot)` and no `WithRandomSource` call
- **THEN** `Build()` succeeds and seeds `SeededRandom` from `snapshot.Rng`

#### Scenario: Loaded session produces same outcome as continued session
- **WHEN** a session is saved at turn boundary N and loaded into a new session
- **AND** both the original and loaded sessions receive identical player responses
- **THEN** both sessions produce the same final `GameResult`

### Requirement: GameDefinition requires an Id
The engine SHALL require every `GameDefinition` to have a non-empty `Id: string`. `GameDefinitionBuilder.Build()` SHALL throw `DefinitionException` if `Id` is null or empty.

#### Scenario: Build succeeds when Id is set
- **WHEN** `GameDefinitionBuilder` has `Id` set to a non-empty string before `Build()` is called
- **THEN** `Build()` succeeds and `GameDefinition.Id` equals the provided value

#### Scenario: Build fails when Id is missing
- **WHEN** `GameDefinitionBuilder.Build()` is called without setting `Id`
- **THEN** `Build()` throws `DefinitionException`

### Requirement: SeededRandom is reproducible across .NET versions
The engine SHALL implement `SeededRandom` using an engine-owned deterministic algorithm (xoshiro128\*\*) that is independent of `System.Random`. Given the same `Seed` and `CallCount`, the sequence of values produced SHALL be identical regardless of .NET version.

#### Scenario: Same seed produces same sequence
- **WHEN** two `SeededRandom` instances are constructed with the same `Seed`
- **THEN** they produce identical sequences of values for the same sequence of calls

#### Scenario: Fast-forward by CallCount produces the correct state
- **WHEN** a `SeededRandom` is constructed from `RngSnapshot { Seed, CallCount }`
- **THEN** its next value equals the value that would be produced by a fresh instance with the same `Seed` after `CallCount` prior calls

### Requirement: Declarative static effects are resolved from GameDefinition on load
The engine SHALL serialize declarative static effects by reference (`CardDefinitionName` + `EffectIndex`) and resolve the full `StaticEffectDef` from `GameDefinition` at load time.

#### Scenario: Declarative effect round-trips by reference
- **WHEN** a session with an active declarative static effect is saved
- **THEN** the snapshot stores a `DeclarativeRef { CardDefinitionName, EffectIndex }` for that effect
- **THEN** on load, the engine resolves the full `StaticEffectDef` from `GameDefinition` using the ref

#### Scenario: Dynamic effect round-trips with inline trigger
- **WHEN** a session with an active dynamic static effect (created at runtime, with a trigger) is saved
- **THEN** the snapshot stores the trigger inline as `DynamicTrigger` with no `DeclarativeRef`
- **THEN** on load, the engine reconstructs the effect from the inlined data

### Requirement: ModifierIndex and ConditionIndex are reconstructed on load
The engine SHALL NOT store `ModifierIndex` or `ConditionIndex` in `AtomSnapshot`. These indices SHALL be reconstructed by iterating the deserialized `Contributions` list after all contributions are loaded.

#### Scenario: Atom modifier index is correct after load
- **WHEN** a session with atoms that have active modifier contributions is saved and loaded
- **THEN** each atom's `ModifierIndex` contains exactly the contributions targeting it, in registration order
