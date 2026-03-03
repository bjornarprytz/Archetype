## 1. Domain Model — Session Atom

- [x] 1.1 Add Session to the atom kinds table in §2 of `docs/domain-model.md`, alongside Player, Card, and Zone
- [x] 1.2 Write a new §2.x "Session" section defining Session as a singleton atom, its creation timing, and the engine-managed fields (`turn-number`, `phase-index`)
- [x] 1.3 Define `session` as a reserved atom reference in §4.3 (reserved names), alongside `candidate`, `trigger_event`, `source`, and `original`
- [x] 1.4 Declare `turn-number` and `phase-index` as engine-reserved accumulator fields; specify that the authoring tool rejects writes to these fields
- [x] 1.5 Add a shared schema entry for the Session atom's universal schema (engine-managed fields) in §2.6
- [x] 1.6 Update §1.4 type vocabulary to include `Session` as a valid parameter type alongside `Atom`, `Number`, `Boolean`, etc.

## 2. Domain Model — Player Registry

- [x] 2.1 Amend §2.2 (Players) to replace "two participants (Player1, Player2)" with the named registry model; remove all fixed-slot language
- [x] 2.2 State the minimum-one constraint: a game definition with no player type definitions is invalid; a manifest with no player instances is invalid
- [x] 2.3 Update the ownership definition (§2.4) to describe ownership as a reference to a player atom by atom idatom, not by slot index
- [x] 2.4 Confirm `owner-of(atom) → Player` remains a built-in property keyword in §9 (Built-in Keywords); update its return type description if needed

## 3. Domain Model — Consistency Pass

- [x] 3.1 Search `docs/domain-model.md` for any remaining references to "Player1", "Player2", "two participants", or "PlayerSlot" and update or remove them
- [x] 3.2 Verify §9 (Built-in Keywords) does not reference `session` as a mutation target; confirm `session` reads are covered by existing `get-state` and `get-property` primitives
- [x] 3.3 Update the scope hierarchy diagram (§4 or CLAUDE.md) if it references player slots

## 4. Review and Sign-off

- [x] 4.1 Technical Architect reviews the amended domain model for consistency with signed-off architecture decisions (D1–D16), flags any new gaps for a follow-up architecture session
- [x] 4.2 Update the domain model status header and resolved open items table once review is complete
