## Context

The domain model (§2.3) states that cards occupy exactly one zone at all times and that zone movement semantics are game-creator-defined — but §9.1 lists no primitive that changes a card's current zone. `AtomSnapshot.ZoneId` (D17) shows the engine tracks zone membership per card, and `in-zone` (§9.2) reads it, but nothing writes it after creation. Game creators composing a `draw-card` or `discard` keyword have no engine call to put at the bottom of their composition tree.

The fix is mechanical: add one mutation primitive, `move-card`, to §9.1 of the domain model and reflect it throughout the architecture. This change also corrects a stale constructor reference in D9 that was superseded by D14/A15 but never cleaned up.

## Goals / Non-Goals

**Goals:**
- Provide a single engine primitive for changing a card's current zone.
- Ensure the move is observable: the primitive logs a `move-card` event with enough information for triggers to react to zone transitions.
- Existing `CheckLifetimes` machinery handles while-condition re-evaluation after a move with no additional work.
- Clean up the D9 constructor reference.

**Non-Goals:**
- Game-specific move semantics (draw, discard, play, mill) — these remain game-creator-defined composites.
- Bulk move (moving a collection of cards atomically) — game creators loop `move-card`.
- Changing card ownership on zone transit — ownership is immutable by design (§2.4).
- Destroying a card on zone exit or entry — modeled via state-based rules.
- Any "stack" or "resolving" zone concept — if a game needs a stack it defines a zone named "stack" and uses `move-card`.

## Decisions

### Decision 1 — Signature: `move-card(card: Card, destination: Zone) → void`

`Card` and `Zone` atom kinds are explicit rather than the generic `Atom`. The engine can statically validate at authoring time that both arguments resolve to the correct kinds. Returning void is correct: the card's `AtomId` is unchanged by a move.

*Alternative considered:* `set-zone(atom: Atom, zone: Zone)` — a more generic name. Rejected because only cards live in zones (zones are not in zones; players are not in zones); a generic name implies generality the primitive does not have.

### Decision 2 — Event logged: `move-card { card: AtomId, origin: AtomId, destination: AtomId }`

The event includes `origin` (the card's zone before the move). This is essential for trigger conditions like "when a card leaves the hand" — without `origin`, the trigger would have to query zone membership at trigger-evaluation time, which is after the move has already happened.

`origin` is read from the card's current `ZoneId` at the start of `move-card` execution, before the zone assignment is updated. This matches how all other mutation primitives record their arguments: at call time, not at event-log-finalization time.

*Alternative considered:* Omit `origin`, require triggers to track it via `get-state`. Rejected: `ZoneId` is structural state, not a game-creator-named accumulator; `get-state` does not read it. The event is the only reliable way to expose the origin zone to trigger conditions.

### Decision 3 — Static effect lifetime re-evaluation: delegate entirely to existing `CheckLifetimes`

`move-card` does not call `CheckLifetimes` directly. It mutates `ZoneId` and logs an event. The `ActionResolver`'s post-block `CheckLifetimes` call (D6) then evaluates all while-conditions — including any `in-zone(card, X)` expressions — and expires or re-activates static effects as needed. No special-casing in `move-card`.

This is consistent with how all other mutations work: mutation primitives mutate state; `CheckLifetimes` observes the resulting state. Introducing a special `CheckLifetimes` call inside a primitive would break the clean separation.

### Decision 4 — Self-move (destination == origin) is valid and logged

If `destination` equals the card's current zone, `move-card` still executes and logs a `move-card` event with `origin == destination`. No special-casing. This keeps the primitive simple and predictable; game creators who want to prevent no-op moves write a condition in their composite keyword.

### Decision 5 — D9 constructor fix is bundled into this change

The D9 consequences block references `ActionResolver(GameDefinition, IPromptChannel, long seed, IEngineObserver?)`. `IPromptChannel` was retired by D14 and replaced by `IReadOnlyDictionary<string, IPlayerStrategy>`. Correcting this as part of the same change keeps the architecture doc consistent before the implementer starts work.

## Risks / Trade-offs

- **`origin` in event requires reading `ZoneId` before mutation**: The implementation must be careful to capture `origin` as the first step, before any zone assignment. This is a one-line discipline; the risk is low but worth making explicit in the implementation task.
- **`move-card` does not validate that the destination zone exists at the time of the call**: Zone definitions are resolved at load time (D12), but dynamic zones created via `create-zone` at runtime are added to `GameState`. The engine should verify the destination `AtomId` resolves to an active zone atom at dispatch time and throw a runtime `EngineException` if not. This is consistent with how other primitives handle invalid atom references.
- **No ordering guarantee within a bulk move loop**: If a game creator calls `move-card` three times in one block, cards move in step order. This is expected and correct; it is not a risk but worth documenting.

## Open Questions

*(none — this is a narrow, well-scoped addition)*
