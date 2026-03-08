## ADDED Requirements

### Requirement: ActionResolver orchestrates the full post-action sequence
After executing a player action's primary effect block, the engine SHALL run a defined post-action sequence: state-based rule fixpoint, then a cascade loop of trigger collection and firing, until no new triggers are satisfied.

#### Scenario: Primary block completes, no triggers — action resolves immediately
- **WHEN** a player action's primary block executes and produces no events that satisfy any active trigger
- **THEN** `CollectSatisfiedTriggers` returns an empty list
- **THEN** the cascade loop exits and the next player action window opens

#### Scenario: Primary block triggers a static effect — trigger fires before next action
- **WHEN** a player action's primary block produces an event satisfying a static effect's trigger
- **THEN** the trigger's fired block executes within the same action resolution sequence
- **THEN** `CheckLifetimes` runs after the trigger-fired block
- **THEN** the cascade loop collects triggers again before opening the next player action window

#### Scenario: Cascade terminates when no new triggers are satisfied
- **WHEN** the cascade loop runs and `CollectSatisfiedTriggers` returns an empty list
- **THEN** the cascade loop exits
- **THEN** the `ActionResolver` considers the action fully resolved

### Requirement: State-based rules run to fixpoint after every effect block
After every effect block completes (primary block or trigger-fired block), the engine SHALL evaluate all registered `StateBasedRule` conditions and execute any triggered rules in registration order. This fixpoint loop SHALL repeat until no rules trigger in a pass.

#### Scenario: No state-based rules registered — loop exits immediately
- **WHEN** `GameDefinition.StateBasedRules` is empty
- **AND** a block completes
- **THEN** `RunStateBasedRules` returns without executing any blocks

#### Scenario: State-based rule conditions are evaluated before any rules fire in a pass
- **WHEN** multiple state-based rules have their conditions satisfied simultaneously
- **THEN** all satisfied conditions are identified before any rule's block executes
- **THEN** all identified rules execute in registration order before conditions are re-evaluated

### Requirement: CheckLifetimes runs after every block, including trigger-fired blocks
The engine SHALL call `CheckLifetimes` after every `ExecuteBlock` call in the post-action sequence — after the primary block, after each trigger-fired block, and after each state-based rule block.

#### Scenario: WhileCondition expires after trigger-fired block moves a card
- **WHEN** a trigger fires and its block moves a card out of a while-condition zone
- **THEN** `CheckLifetimes` runs after the trigger-fired block
- **THEN** the static effect whose while-condition is now false is expired

### Requirement: IEngineObserver halt mechanism is wired into the cascade loop
The engine SHALL call `IEngineObserver.OnTriggerCascade(batchCount)` before each trigger collection pass in the cascade loop. If the observer returns `Halt`, the cascade loop SHALL exit without collecting or firing further triggers.

#### Scenario: Null observer always continues
- **WHEN** no `IEngineObserver` is provided (null)
- **THEN** the cascade loop treats every batch as `Continue`

#### Scenario: Observer halts cascade after a configurable number of batches
- **WHEN** an `IEngineObserver` is provided that returns `Halt` after `N` batches
- **THEN** the cascade loop exits after `N` batches
- **THEN** the game state is not rolled back — the halt is clean and the game continues

### Requirement: Trigger-fired blocks execute in a child action scope
Each trigger-fired block SHALL execute in a child action context (new `ActionScopeId`), so its events are properly scoped and do not merge into the primary block's action scope.

#### Scenario: Trigger-fired events appear in their own action scope
- **WHEN** a trigger fires and its block produces event E2
- **THEN** E2's action scope is distinct from the primary block's action scope
- **THEN** E2 is visible in `events.this_game` and `events.this_turn` but NOT in `events.this_action` of the primary block

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
