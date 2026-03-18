## Context

`GameSession.RunAsync` drives the game loop (phases, turns, actions) and must know when the game has ended. D14 of `docs/architecture.md` described the termination condition abstractly — "repeat until a state-based rule produces an outcome" — but left the signalling mechanism unspecified. The implementation filled this gap with a terminal-flag pattern: two built-in primitives write to a flag on `GameState`, and the loop checks it after each action.

All implementation is already complete and tested. This design document records the rationale so the architectural decisions can be ratified in `docs/architecture.md`.

## Goals / Non-Goals

**Goals:**
- Ratify the terminal-flag pattern as the canonical game-ending mechanism.
- Record the first-call-wins invariant for `DeclareOutcome`.
- Specify where in the action lifecycle `GameIsOver` is checked.
- Document `player-by-name` as the canonical way to reference a player from a static keyword tree.

**Non-Goals:**
- Changing the existing D7/D8 trigger/SBR lifecycle.
- Multi-winner outcomes.
- Specifying UI-layer handling of the outcome.

## Decisions

### D-A: Terminal-flag pattern over return value

**Decision**: Game-ending primitives write a flag (`GameIsOver`) to `GameState`; `GameSession.RunAsync` polls the flag after each `ResolveAction` call rather than receiving a return value from it.

**Rationale**: `ResolveAction` returns `void` (`Task`) — it handles a full post-action sequence including SBR fixpoint and trigger cascade. Threading a `GameResult?` return value through `ResolveAction`, `RunStateBasedRules`, and `FireTrigger` would require changing every call site and every intermediate signature. The flag pattern keeps all those signatures stable and the check is a single `if (_state.GameIsOver) break` at each level.

**Alternative considered**: Returning `GameResult?` from `ResolveAction`. Rejected: too invasive; the cascade loop already has multiple early-exit points that would all need updating.

### D-B: First-call-wins for DeclareOutcome

**Decision**: `GameState.DeclareOutcome(winner?)` ignores all calls after the first. If a trigger cascade fires two `declare-winner` calls in the same batch, the first one wins and the second is silently dropped.

**Rationale**: During a cascade, multiple triggers can fire before any `GameIsOver` check. Without this invariant, the last-writer-wins semantics would make game outcomes depend on trigger ordering, which is fragile. First-call-wins gives the state-based rule that fires first (and thus the highest-priority rule by the engine's ordering) authority over the outcome.

### D-C: player-by-name as the runtime player reference primitive

**Decision**: Add `player-by-name(name: PropertyName) → Player` as a built-in that reverse-looks up a player atom by its registered name string.

**Rationale**: A card's `KeywordNode` tree is authored statically before atom IDs are assigned at runtime. There is no way to embed a concrete player atom ID in a tree that says "player 1 wins." `player-by-name` bridges authoring-time names and runtime atoms, analogous to how zone names work in `move-card` targets. The name string is a `PropertyName` (a typed parameter) so it is part of the formal keyword type system.

**Alternative considered**: Passing a player atom reference through the effect block's bindings. Rejected: this would require every game definition that wants to declare a winner to thread the player reference through all intervening keyword parameters, which is unergonomic for common SBR patterns.

### D-D: Early-exit placement in RunStateBasedRules

**Decision**: `RunStateBasedRules` checks `GameIsOver` at the top of each fixpoint iteration and returns immediately if true.

**Rationale**: Without this check, an always-true SBR that fires `declare-winner` would re-fire on the next fixpoint iteration (because the SBR condition is still true), causing an infinite loop. The early-exit is the minimal guard against this.

## Risks / Trade-offs

- **Silent drop on second DeclareOutcome call** → The first-call-wins invariant means a bug in rule ordering could cause the "wrong" player to win without any error. Mitigation: log both calls to the event log so post-hoc analysis can detect conflicts.
- **player-by-name fails at runtime if name is unregistered** → Throws `EngineException`. Mitigation: `GameSessionBuilder.Build()` validates that all players in `PlayerDefinitions` have registered names before the session starts, so unregistered names indicate a game-definition bug caught at build time.
