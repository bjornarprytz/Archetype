## Why

The domain model currently hardcodes exactly two participants (Player1, Player2) and has no first-class representation of game-level state, forcing the engine to track turn number and phase index as hidden internal counters rather than inspectable game state. This prevents game creators from expressing "whose turn it is" through the same keyword system used everywhere else, and makes the player model inflexible for games with asymmetric or non-standard participant counts.

## What Changes

- **New atom kind: Session** — a singleton atom created by the engine at game start, accessible via the reserved reference `session` in all keyword expressions. The engine owns two accumulators on it (`turn-number`, `phase-index`); game creators may extend it with additional state via shared schemas or inline declarations. The three existing atom kinds (Player, Card, Zone) are joined by Session as a fourth first-class kind.
- **Players become a named registry** — `PlayerDefinition` entries are now a game-creator-defined named registry (minimum one required), rather than a hardcoded pair (Player1, Player2). The engine no longer encodes the number of participants. "Whose turn it is" and "who is the active player" are expressed as session state, managed by the game creator via phase init/cleanup effect blocks.
- **`owner-of` generalises** — ownership relationships now reference player atoms by their atom idatom, not by a fixed slot enum.

## Capabilities

### New Capabilities

- `session-atom`: The session atom kind — its definition, engine-managed state fields, game-creator extension model, and the `session` reserved reference.
- `player-registry`: The player atom registry model — named player type definitions, minimum-one constraint, and how ownership references player atoms.

### Modified Capabilities

None. No existing spec files exist yet; the domain model document is the current source of truth and will be amended directly.

## Non-goals

- Architecture-level implementation decisions (renaming `AtomId` → `AtomId`, retiring `PlayerSlot`, replacing `IPlayerStrategy` with `IDecisionHandler`, manifest provisioning changes) — these are deferred to an architecture session and will amend `docs/architecture.md`.
- Multiplayer beyond the engine's current two-strategy model — the player registry generalises the *definition* side; session routing changes are architecture concerns.
- Any changes to the existing three atom kinds (Player, Card, Zone) beyond acknowledging Session as a peer.

## Impact

- `docs/domain-model.md` — §2 (atom kinds) gains a Session section; §2.2 (Players) is amended to describe the named registry model; the atom type table gains a Session row.
- `docs/architecture.md` — D14 (Game Creator API), D6 (Static Effect Lifecycle), D7 (State-Based Rule Runner) reference turn number and phase index; these will need a follow-up architecture amendment to reference the session atom. Flagged as a known gap, not resolved here.

## Personas

Domain Modeler owns the specification work. Technical Architect reviews for consistency with signed-off architecture decisions before sign-off.
