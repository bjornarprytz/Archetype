# game-outcome-primitives Specification

## Purpose
TBD - created by archiving change ratify-game-outcome-primitives. Update Purpose after archive.
## Requirements
### Requirement: declare-winner primitive
The engine SHALL provide a `declare-winner(player: Player)` built-in keyword that terminates the game and records the given player atom as the winner. The primitive SHALL be callable from any effect block, including state-based rule blocks and trigger-fired blocks.

#### Scenario: SBR fires declare-winner
- **WHEN** a state-based rule's condition evaluates to true and its block calls `declare-winner(p)` where `p` is a valid player atom
- **THEN** `GameState.GameIsOver` is set to true, `GameState.PendingWinner` is set to the name of player `p`, and `GameSession.RunAsync` returns a `GameResult` with `Winner` equal to that name

#### Scenario: declare-winner from a trigger-fired block
- **WHEN** a triggered effect block calls `declare-winner(p)`
- **THEN** the game is flagged as over after that trigger fires; the remaining triggers in the same cascade batch are not fired

#### Scenario: Two declare-winner calls in the same cascade
- **WHEN** two triggers in the same cascade batch each fire `declare-winner` for different players
- **THEN** the first call determines the winner; the second call is silently ignored (first-call-wins)

### Requirement: declare-draw primitive
The engine SHALL provide a `declare-draw()` built-in keyword that terminates the game with no winner.

#### Scenario: SBR fires declare-draw
- **WHEN** a state-based rule's block calls `declare-draw()`
- **THEN** `GameState.GameIsOver` is set to true, `GameState.PendingWinner` is null, and `GameSession.RunAsync` returns a `GameResult` with `Winner` equal to null

#### Scenario: declare-draw after declare-winner in same cascade
- **WHEN** `declare-winner` fires first in a cascade, and a subsequent trigger fires `declare-draw`
- **THEN** the `declare-draw` call is silently ignored; the outcome from `declare-winner` stands

### Requirement: GameIsOver propagation contract
After every `ResolveAction` call (primary action, trigger-fired action, or SBR block), the engine SHALL check `GameState.GameIsOver` and halt further processing if true. Specifically:

- `GameSession.RunAsync` SHALL exit its turn loop immediately after any `ResolveAction` returns when `GameIsOver` is true.
- The trigger cascade loop in `ActionResolver` SHALL break before firing each new batch when `GameIsOver` is true.
- `RunStateBasedRules` SHALL exit at the top of each fixpoint iteration when `GameIsOver` is true.

#### Scenario: Game ends during phase init block
- **WHEN** a phase's init block calls `declare-winner` and `ResolveAction` returns
- **THEN** `RunAsync` exits without processing any further phases or turns, and returns a `GameResult` with the declared winner

#### Scenario: SBR fixpoint does not loop infinitely on terminal rule
- **WHEN** a state-based rule with an always-true condition fires `declare-winner`
- **THEN** the fixpoint loop exits on the next iteration rather than re-firing the rule, because `GameIsOver` is checked at the top of each pass

### Requirement: player-by-name primitive
The engine SHALL provide a `player-by-name(name: PropertyName) → Player` built-in keyword that resolves a player atom at runtime from a name string registered during session provisioning. This is the canonical way for statically-authored keyword trees to reference a player atom whose ID is not known at authoring time.

#### Scenario: Resolve a registered player name
- **WHEN** `player-by-name("alice")` is called and a player atom with name "alice" was provisioned by `GameSessionBuilder`
- **THEN** the call returns the atom ID of the "alice" player atom

#### Scenario: Unregistered name throws
- **WHEN** `player-by-name("unknown")` is called and no player atom with that name exists in the current game state
- **THEN** the engine raises an `EngineException` with a message identifying the unknown name

#### Scenario: Chaining with declare-winner
- **WHEN** a keyword tree contains `declare-winner(player-by-name("alice"))`
- **THEN** the engine resolves the player atom for "alice" first, then passes it to `declare-winner`, terminating the game with "alice" as the winner

