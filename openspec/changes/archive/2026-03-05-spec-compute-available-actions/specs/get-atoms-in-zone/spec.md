## ADDED Requirements

### Requirement: get-atoms-in-zone primitive
The engine SHALL provide a `get-atoms-in-zone(zone: Zone) → Atom[]` built-in property query that returns the atom IDs of all atoms whose current `ZoneId` equals the given zone atom's ID. The query SHALL be pure (no state mutation, no event logging).

#### Scenario: Zone contains atoms
- **WHEN** `get-atoms-in-zone(z)` is called and atoms A and B currently have `ZoneId` equal to `z`
- **THEN** the call returns a collection containing A and B

#### Scenario: Zone contains no atoms
- **WHEN** `get-atoms-in-zone(z)` is called and no atom has `ZoneId` equal to `z`
- **THEN** the call returns an empty collection

#### Scenario: Argument must be a zone atom
- **WHEN** `get-atoms-in-zone(x)` is called and `x` is an atom of kind other than Zone
- **THEN** the engine raises an `EngineException` indicating the argument must be a Zone atom

### Requirement: get-atoms-in-zone is usable in ComputeAvailableActions
`ComputeAvailableActions` SHALL use `get-atoms-in-zone` (or the equivalent `GameState` read) to determine which atoms are in each relevant zone before constructing the available actions list.

#### Scenario: Hand zone query drives PlayCard list
- **WHEN** `ComputeAvailableActions` is called for the active player whose hand zone contains cards C1 and C2
- **THEN** the query for the hand zone returns C1 and C2, and both appear as `PlayCard` candidates (subject to activation condition filtering)
