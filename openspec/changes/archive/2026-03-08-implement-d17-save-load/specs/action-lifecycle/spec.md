## ADDED Requirements

### Requirement: IEngineObserver notifies the host at the start of each turn with a snapshot
The engine SHALL call `IEngineObserver.OnTurnStart(int turnNumber, GameStateSnapshot snapshot)` at the start of each turn, after all end-of-turn processing for the previous turn has completed but before any phase init block for the current turn executes. This gives the host the opportunity to persist the snapshot.

#### Scenario: OnTurnStart is called before turn 1's phase init
- **WHEN** `GameSession.RunAsync` begins
- **THEN** `IEngineObserver.OnTurnStart(1, snapshot)` is called before any phase init block for turn 1 executes

#### Scenario: OnTurnStart is called at each subsequent turn boundary
- **WHEN** all end-of-turn cleanup, trigger resolution, and lifetime checks for turn N complete
- **THEN** `IEngineObserver.OnTurnStart(N+1, snapshot)` is called before any phase init block for turn N+1

#### Scenario: Null observer skips OnTurnStart silently
- **WHEN** `GameSession` is constructed with a null `IEngineObserver`
- **THEN** the turn loop proceeds normally with no `OnTurnStart` notification

#### Scenario: Snapshot passed to OnTurnStart reflects fully settled state
- **WHEN** `OnTurnStart(N+1, snapshot)` is called
- **THEN** `snapshot` contains all end-of-turn state changes from turn N (expired effects removed, dormant effects updated, accumulators merged into finalized log)
- **THEN** no phase init block for turn N+1 has yet modified the state
