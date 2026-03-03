## ADDED Requirements

### Requirement: Player definitions are a named registry
The engine SHALL support a named registry of player atom type definitions. Each entry has a unique name and follows the same atom type definition model as cards and zones (static property declarations, state map declarations, shared schema inclusion). The registry replaces the previous hardcoded two-slot model (Player1, Player2).

#### Scenario: Game creator defines two asymmetric player types
- **WHEN** a game creator defines player types named `"hero"` and `"dungeon-master"` with different static properties
- **THEN** both definitions are valid and the engine instantiates them independently per the init manifest

#### Scenario: Game creator defines a single player type for a solo game
- **WHEN** a game creator defines exactly one player type and the init manifest instantiates one player of that type
- **THEN** the game definition is valid and the engine runs with one participant

### Requirement: At least one player definition is required
A game definition SHALL contain at least one player type definition. A game definition with zero player type definitions SHALL be rejected as a `DefinitionException` at load time. An init manifest SHALL instantiate at least one player atom; a manifest with zero player instances SHALL be rejected at provisioning time.

#### Scenario: Empty player registry is rejected
- **WHEN** a game creator builds a `GameDefinition` with no player type definitions
- **THEN** a `DefinitionException` is thrown at definition build time

#### Scenario: Manifest with no player instances is rejected
- **WHEN** a game definition has player type definitions but the init manifest specifies no player instances
- **THEN** a provisioning error is raised before the first phase begins

### Requirement: Ownership references player atoms by atom idatom
Ownership of a card or zone SHALL be expressed as a reference to the owning player atom (by atom idatom). Ownership is set at creation and is immutable. The engine SHALL expose a built-in property keyword `owner-of(atom)` that returns the owning player atom reference for any card or zone atom.

#### Scenario: owner-of returns the owning player atom
- **WHEN** a game creator writes `owner-of(some-card)` in a keyword expression
- **THEN** the expression type-checks as `Player` and returns the player atom that owns `some-card` at runtime

#### Scenario: owner-of is valid for zones
- **WHEN** a game creator writes `owner-of(some-zone)` in a keyword expression
- **THEN** the expression type-checks as `Player` and returns the player atom that owns `some-zone` at runtime

#### Scenario: Ownership is immutable after creation
- **WHEN** a card is created via `create-card` or `copy-card` with a declared owner
- **THEN** no keyword or effect exists that can change the owning player atom for the lifetime of that card

### Requirement: Active player is game-creator-managed session state
The engine SHALL NOT enforce whose turn it is or which player may take actions. Game creators SHALL express active-player idatom as session state (e.g. a condition or accumulator on the session atom) and enforce it via activation conditions on their effect blocks.

#### Scenario: Game creator tracks active player via session condition
- **WHEN** a game creator applies `apply-condition(session, "player1-active")` in a phase init block
- **THEN** subsequent activation conditions can test `get-state(session, "player1-active")` to gate which cards or abilities are playable

#### Scenario: Engine does not gate actions by participant idatom
- **WHEN** the engine opens an action window
- **THEN** it evaluates activation conditions for all atoms without any engine-level restriction based on participant slot or idatom; filtering is entirely determined by the game creator's activation condition expressions
