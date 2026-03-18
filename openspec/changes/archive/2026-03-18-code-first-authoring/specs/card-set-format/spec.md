## ADDED Requirements

### Requirement: CardSet is a named, serializable collection of card definitions
A `CardSet` SHALL be a record type containing a set name, a format version, and a list of `CardSpec` entries. `CardSet` SHALL be JSON-serializable and JSON-deserializable using the same schema as `CardSpec` established in D29.

#### Scenario: CardSet round-trips through JSON without data loss
- **WHEN** a `CardSet` is serialized to JSON and then deserialized from that JSON
- **THEN** the resulting `CardSet` is structurally equal to the original (same name, version, and card specs)

#### Scenario: CardSet JSON is a self-contained file
- **WHEN** a card set JSON file is loaded in isolation (without the rules binary)
- **THEN** its contents are parseable as a `CardSet`; keyword name references remain as strings (not resolved)

### Requirement: Card set JSON is the distribution format; C# is the authoring format
Card sets SHALL be authored in C# using the builder API (which provides type-checked construction of effect blocks). The build tool SHALL be responsible for serializing `CardSet` instances to JSON. No game creator SHALL be required to hand-write card set JSON.

#### Scenario: Effect block with branching serializes correctly
- **WHEN** a card spec is authored in C# with an effect block containing an `if`/`then`/`else` construct using `Kw.If`
- **THEN** the serialized JSON represents the full `KeywordNode` tree including the conditional node

### Requirement: GameDefinition supports load-time merge of card sets
`GameDefinition` SHALL expose a `WithCardSets(IEnumerable<CardSet> sets)` method that returns a new `GameDefinition` containing the merged card definitions. The merge SHALL be additive; no existing keyword or rule definitions SHALL be modified by the merge.

#### Scenario: Multiple sets merge without conflict
- **WHEN** two `CardSet` instances with non-overlapping card names are merged into a `GameDefinition`
- **THEN** the resulting `GameDefinition` contains all cards from both sets

#### Scenario: Duplicate card name across sets raises an error
- **WHEN** two `CardSet` instances both contain a card with the same name and are merged into the same `GameDefinition`
- **THEN** `WithCardSets` throws `DefinitionException` identifying the duplicate card name and the conflicting set names

### Requirement: Load-time validation checks keyword references in card specs
When card sets are merged into a `GameDefinition`, all keyword name references within card spec effect blocks SHALL be validated against the registered keyword names in the `GameDefinition`. An unresolved reference SHALL cause a `DefinitionException` before the game session can start.

#### Scenario: Valid keyword reference accepted at merge time
- **WHEN** a card spec references keyword `"attack"` and `"attack"` is registered in the `GameDefinition`
- **THEN** the merge succeeds and the reference is resolved

#### Scenario: Unknown keyword reference rejected at merge time
- **WHEN** a card spec references keyword `"frobnicate"` and no such keyword is registered
- **THEN** `WithCardSets` throws `DefinitionException` identifying the card name and the unresolved keyword reference

### Requirement: Rules GameDefinition and card sets are loaded independently at Godot startup
The Godot host SHALL call `[GameName].Rules.BuildDefinition()` to obtain the base `GameDefinition`, then load card set JSON files from disk, then call `WithCardSets` to produce the final definition before constructing `ActionResolver`. The rules assembly SHALL be a compiled Godot project dependency; card set JSON files SHALL be loaded at runtime from a configurable directory.

#### Scenario: Card set added after initial build
- **WHEN** a new card set JSON file is placed in the card sets directory and the Godot application is restarted
- **THEN** the new cards are available in the game without recompiling the rules assembly
