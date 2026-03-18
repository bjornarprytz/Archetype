## Why

The domain model explicitly states that the engine provides zone membership tracking and that card movement is defined by game creators — but §9.1 lists no primitive for changing a card's zone after creation. Without a zone-assignment primitive, game creators cannot implement any zone-transition mechanic (drawing, playing, discarding, shuffling back) regardless of how they author their keywords. The engine tracks `ZoneId` per card but provides no way to change it.

## What Changes

- A new mutation primitive `move-card(card, destination)` is added to the domain model (§9.1) as amendment A16.
- The architecture reflects A16: `move-card` is registered in `BuiltInKeywords` (Core), gains a `Kw.MoveCard(...)` shorthand (Build), and the D12 primitives table is updated.
- The D9 consequences block is corrected: the stale `IPromptChannel` constructor reference is replaced with the `IPlayerStrategy`-based signature that D14/A15 established.

## Capabilities

### New Capabilities

- `zone-movement`: The engine's primitive for relocating an existing card from its current zone to a destination zone. Covers the signature, event log entry, and constraints (owner unchanged, runtime state preserved).

### Modified Capabilities

*(none — no existing spec-level requirements change)*

## Non-goals

- Defining game-specific movement semantics (draw, discard, play, mill, etc.) — those remain game-creator-defined keywords composed on top of the primitive.
- Moving zones themselves or moving multiple cards atomically — a game creator who needs bulk movement calls `move-card` in a loop.
- Changing card ownership on movement — ownership is immutable; `move-card` changes zone only.
- Destroying a card on zone exit or entry — game creators model that with state-based rules.

## Impact

- **Domain model (`docs/domain-model.md`)**: A16 amendment adds `move-card` to §9.1 Mutation Primitives.
- **Architecture (`docs/architecture.md`)**: D12 primitives table updated; `BuiltInKeywords` list in D15 updated; `Kw` factory in D14 updated; D9 constructor reference corrected.
- **`Archetype.Core`**: `BuiltInKeywords` gains `move-card` entry (name + `ParameterDecl[]`).
- **`Archetype.Build`**: `Kw.MoveCard(card, destination)` shorthand added.
- **`Archetype.Engine`**: C# implementation of `move-card` registered at startup; updates `AtomSnapshot.ZoneId` and logs a `move-card` event.

## Personas

The **domain modeler** owns the A16 domain model amendment. The **technical architect** owns the corresponding architecture updates. Both must be completed before the implementer can act.
