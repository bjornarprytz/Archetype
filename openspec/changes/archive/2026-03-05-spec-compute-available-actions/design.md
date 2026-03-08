## Context

`IPlayerStrategy.ChooseAction` receives a list of `PlayerAction` objects from `GameSession.ComputeAvailableActions`. A strategy cannot make a meaningful decision if the list contains illegal actions (e.g., playing a card that is not in hand, or activating an ability that cannot be paid for). The current implementation returns all cards owned by the active player with no filtering, which is a known gap documented in `docs/implementation-status.md`.

The core problem is that "legality" has multiple layers:

1. **Zone filter**: Only cards in the active player's hand zone (or equivalent) are playable.
2. **Activation condition**: Some cards have a `KeywordNode` condition that must evaluate to true.
3. **Cost pre-flight**: Some cards have costs; paying them must be possible given current state.

Each layer requires a different engine capability.

## Goals / Non-Goals

**Goals:**
- Specify a zone-aware filter as the minimum viable legality check.
- Specify how activation conditions are evaluated (pure, no side effects).
- Specify cost pre-flight as an optional second pass (can be deferred).
- Provide a `get-atoms-in-zone` query primitive sufficient for zone filtering.

**Non-Goals:**
- Full game-rule legality (game authors encode that in activation conditions).
- Undo/redo of cost evaluation.
- Specifying concrete hand-zone names (those are game-definition concerns).

## Decisions

### D-A: Zone filtering via get-atoms-in-zone primitive

**Decision**: Add `get-atoms-in-zone(zone: Zone) → Atom[]` as a built-in property query that returns all atom IDs currently in a given zone. `ComputeAvailableActions` uses this to filter cards to those in the defined "playable" zone (configured per `CardDefinition` or `GameDefinition`).

**Rationale**: `GameState` already has `AtomSnapshot.ZoneId`; a zone query is a pure read over existing state. Adding it as a named primitive keeps the query composable with keyword trees rather than requiring a bespoke C# API.

**Alternative considered**: A C# API on `IGameStateReadable` directly. Rejected: bypasses the keyword system and creates a second query surface that must be kept in sync.

### D-B: Activation conditions are pure-evaluated

**Decision**: `CardDefinition.ActivationCondition` (if present) is evaluated using `BlockExecutor.EvaluateCondition` — the same pure evaluation path used for trigger conditions and while-conditions. No side effects, no event logging.

**Rationale**: `EvaluateCondition` already exists and is WASM-safe. Reusing it avoids a new evaluation code path.

### D-C: Cost pre-flight is best-effort and deferred

**Decision**: Cost pre-flight (checking whether a card's cost can currently be paid) is not part of the minimum viable `ComputeAvailableActions`. It is deferred until a game definition actually requires it. When eventually added, it will be a dry-run through the cost keyword that checks sufficiency without mutation.

**Rationale**: No current test game requires cost filtering; speccing it now would be speculative. Deferral is called out explicitly so the implementer does not fill it with a placeholder silently.

### D-D: Pass is always available

**Decision**: `Pass` is always included in the available actions list, unconditionally.

**Rationale**: A player must always be able to end their turn. If no other actions are available, `Pass` is the only option. Strategies should never be stuck.

## Risks / Trade-offs

- **get-atoms-in-zone returns atom IDs, not typed card references** → Callers must filter by `AtomKind == Card` themselves. Mitigation: document this in the primitive's XML doc; `ComputeAvailableActions` applies the filter internally.
- **Activation condition evaluation is synchronous** → `EvaluateCondition` is currently synchronous; any future condition that requires async (e.g., network-based state) would break this contract. Mitigation: flag this in the implementation; conditions are currently pure keyword trees with no async primitives.
