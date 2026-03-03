## ADDED Requirements

### Requirement: Session is a first-class atom kind
Session SHALL be a fourth first-class atom kind alongside Player, Card, and Zone. It participates in the full atom state model: accumulators, modifiers, and conditions/tags, all contribution-tracked and engine-managed. Session atom type definitions follow the same declaration model as other atom kinds (static property declarations and state map declarations, composable via shared schemas).

#### Scenario: Session atom is accessible via get-state
- **WHEN** a game creator writes `get-state(session, "turn-number")` in a keyword expression
- **THEN** the expression is valid, type-checks as `Number`, and returns the current turn count at runtime

#### Scenario: Session atom accepts conditions
- **WHEN** a game creator calls `apply-condition(session, "overtime")` in an effect block
- **THEN** the condition is applied to the session atom and is queryable via `get-state(session, "overtime")`

#### Scenario: Session atom accepts modifiers
- **WHEN** a game creator calls `apply-modifier(session, "some-property", additive, 1, permanent)` in an effect block
- **THEN** the modifier is tracked on the session atom and included in computed property evaluation

### Requirement: Session atom is a singleton created by the engine
The engine SHALL create exactly one session atom at game start, before any manifest provisioning or phase execution. The session atom is never destroyed during the game. Game creators cannot create additional session atoms via `create-card`, `copy-card`, or any other primitive.

#### Scenario: Session atom exists before first phase
- **WHEN** the first phase's init block begins execution
- **THEN** `session` resolves to a valid atom reference and all engine-managed fields are initialised

#### Scenario: Session atom cannot be instantiated by game creators
- **WHEN** a game creator attempts to define a card or zone whose effect block calls any creation primitive with a session-typed argument
- **THEN** the authoring tool rejects the expression as a type error at authoring time

### Requirement: Engine manages turn-number and phase-index on the session atom
The engine SHALL maintain two reserved accumulator fields on the session atom:
- `turn-number` — incremented by the engine at the start of each new turn; initialised to 1.
- `phase-index` — set by the engine to the ordinal index (0-based) of the current phase within the turn's phase sequence; reset to 0 at the start of each turn.

Game creators MAY read these fields via `get-state`. Game creators SHALL NOT write to these fields directly; the authoring tool SHALL reject any keyword expression that writes to a reserved session field.

#### Scenario: turn-number increments each turn
- **WHEN** the engine advances from one turn to the next
- **THEN** `get-state(session, "turn-number")` returns a value one greater than the previous turn

#### Scenario: phase-index reflects current phase
- **WHEN** the second phase (index 1) of a turn is executing
- **THEN** `get-state(session, "phase-index")` returns `1`

#### Scenario: Writing to turn-number is an authoring error
- **WHEN** a game creator writes `modify-accumulator(session, "turn-number", 1)` in a keyword expression
- **THEN** the authoring tool flags this as an error: write to reserved session field

### Requirement: session is a reserved atom reference
`session` SHALL be a reserved name in keyword expressions that always resolves to the singleton session atom. It is not a parameter name and cannot be declared as one. The authoring tool SHALL reject any keyword or effect block parameter declaration that uses the name `session`.

#### Scenario: session resolves in any keyword expression
- **WHEN** a game creator uses `session` as an argument in any keyword invocation
- **THEN** it resolves to the session atom at both authoring time (type: Session) and runtime (the singleton instance)

#### Scenario: session cannot be used as a parameter name
- **WHEN** a game creator declares a keyword with a parameter named `session`
- **THEN** the authoring tool rejects the definition with an error citing the reserved name

### Requirement: Game creators may extend the session atom with additional state
Game creators MAY declare additional static properties and state fields on the session atom type definition, using the same inline declaration and shared schema mechanisms available to all atom kinds. Extended fields are accessed via `get-state` and `get-property` like any other atom field.

#### Scenario: Game creator declares a custom session accumulator
- **WHEN** a game creator declares an accumulator field `"storm-count"` on the session atom type definition
- **THEN** `get-state(session, "storm-count")` is valid and type-checks as `Number`

#### Scenario: Game creator includes a shared schema on session
- **WHEN** a game creator includes a shared schema that declares a condition field `"is-sudden-death"` on the session atom type
- **THEN** `get-state(session, "is-sudden-death")` is valid and type-checks as `Boolean`
