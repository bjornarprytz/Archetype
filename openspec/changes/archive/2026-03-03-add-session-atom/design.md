## Context

The domain model defines three first-class atom kinds (Player, Card, Zone) and a state model that applies uniformly to all of them. However, two pieces of engine state sit outside this model:

- **Turn number and phase index** are tracked as hidden internal counters. They are not accessible via `get-state`, cannot be referenced in keyword expressions, and cannot be used as lifetime condition inputs without special-casing in the engine.
- **Player idatom** is hardcoded as a fixed two-slot enum (Player1, Player2). The engine enforces exactly two participants, which prevents asymmetric games and makes "whose turn it is" an engine concern rather than a game-creator concern.

Both problems share the same root: there is no atom that represents the game itself. Introducing one closes both gaps with minimal new machinery.

## Goals / Non-Goals

**Goals:**
- Define Session as a fourth first-class atom kind, consistent with the existing atom model
- Specify the engine-managed fields on the session atom (`turn-number`, `phase-index`)
- Define the `session` reserved reference so game creators can access it in keyword expressions
- Generalise the player model to a named registry with a minimum-one constraint

**Non-Goals:**
- Architecture implementation decisions (AtomId, IDecisionHandler, PlayerSlot retirement, manifest provisioning) — architecture session
- Multiplayer beyond generalising the definition model
- Changing the state model shared by all atom kinds (§3 of the domain model)
- Adding any game-creator-visible primitives beyond `session` reference access

## Decisions

### Decision 1: Session as an atom, not a special engine object

**Chosen:** Session is a full atom — it participates in the same state model (accumulators, modifiers, conditions) and is accessible via `get-state` and `get-property` like any other atom.

**Alternative considered:** A separate `GameContext` object with hand-crafted accessors. Rejected because it introduces a second access pattern for game creators to learn, and because properties like "turn number" are already expressible as accumulators under the existing model. Making session an atom costs nothing and gives game creators the full keyword vocabulary for free.

**Consequence:** Session atom type definitions follow the same declaration model as cards, zones, and players (§2.5 of the domain model). Engine-managed fields are declared as part of the session's universal schema.

### Decision 2: Engine-owned fields are declared in a universal session schema

**Chosen:** `turn-number` and `phase-index` are declared in a required universal schema for the Session atom kind — the same shared schema mechanism already defined in §2.6. The engine increments them; game creators read them but do not write them directly.

**Alternative considered:** Reserving field names and blocking game creators from writing to them via a new "read-only field" concept. Rejected — the existing model has no read-only fields, and adding that concept inflates the type system. Instead: the engine simply never exposes a keyword that allows arbitrary writes to reserved session fields. Game creators cannot call `modify-accumulator(session, "turn-number", ...)` in the DSL because the engine is the only caller of that path. This is enforced by the authoring tool, not the type system.

**Consequence:** The domain model must list `turn-number` and `phase-index` as engine-reserved accumulator names that the authoring tool prevents game creators from writing.

### Decision 3: `session` as a reserved atom reference

**Chosen:** `session` is a reserved name in keyword expressions that always resolves to the singleton session atom. It is not a parameter name; it is a built-in reference, similar to how `null` or `true` are reserved in most languages.

**Alternative considered:** Requiring game creators to pass the session atom as a parameter wherever they need it. Rejected because session is always uniquely available — there is exactly one — and requiring explicit threading is ergonomic friction with no benefit.

**Consequence:** `session` joins `candidate`, `trigger_event`, `source`, and `original` as a reserved name (§4.3 of the domain model). The authoring tool must prevent game creators from declaring a parameter named `session`.

### Decision 4: Players as a named registry, minimum one

**Chosen:** `PlayerDefinition` entries are a named registry (like cards and zones), not a fixed two-slot pair. The engine requires at least one player definition and at least one player instance in the manifest. "Whose turn it is" is not an engine concept — it is session state managed by the game creator.

**Alternative considered:** Keeping Player1/Player2 and adding a third optional slot. Rejected as a half-measure that still hardcodes assumptions about participant count and turn order.

**Consequence:** The domain model's §2.2 (Players) must be amended to describe the registry model. The sentence "The engine targets two participants" becomes "The engine requires at least one participant; game creators define the participant model." Ownership is described in terms of atom idatom, not slot index.

## Risks / Trade-offs

- **[Risk] Game creators expect turn management to be automatic** → The engine no longer owns whose-turn logic. Game creators must manage `active-player` via phase init/cleanup blocks. Mitigation: clear documentation; the domain model should provide a canonical example.

- **[Risk] `session` reserved name conflicts with existing game creator keywords** → Any existing game that already uses `session` as a parameter name would break. Mitigation: the engine is pre-implementation; no existing games need migration.

- **[Risk] Engine-owned fields are bypassed** → Game creators might attempt `modify-accumulator(session, "turn-number", ...)`. Mitigation: the authoring tool flags writes to reserved session fields as errors at authoring time.

## Open Questions

None. Architecture-level questions (AtomId, IDecisionHandler, manifest provisioning) are deferred explicitly and do not block domain model sign-off.
