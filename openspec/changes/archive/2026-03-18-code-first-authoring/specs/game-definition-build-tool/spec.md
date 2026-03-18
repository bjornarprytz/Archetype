## ADDED Requirements

### Requirement: BuildRunner is a programmatic API, not a standalone CLI
`Archetype.Build` SHALL expose a `BuildRunner` class with a static `Run(GameDefinition definition, IEnumerable<CardSet> sets, string outputDir)` method. The game developer SHALL write their own console application `Program.cs` that constructs the definition and invokes `BuildRunner.Run`. `Archetype.Build` SHALL NOT prescribe a CLI argument format or project discovery mechanism.

#### Scenario: BuildRunner writes outputs to the specified directory
- **WHEN** `BuildRunner.Run(definition, sets, "output/")` is called
- **THEN** the output directory is created if it does not exist and all output files are written within it

#### Scenario: BuildRunner is invoked from a game-specific console project
- **WHEN** a game developer creates a `[GameName].Build` console project that references `Archetype.Build` and their rules project, constructs the `GameDefinition`, and calls `BuildRunner.Run`
- **THEN** the build project compiles and runs without requiring changes to `Archetype.Build`

### Requirement: BuildRunner serializes each CardSet to a JSON file
For each `CardSet` in the supplied enumerable, `BuildRunner` SHALL serialize the set to a JSON file named `[set-name].json` in the output directory. The JSON SHALL conform to the `CardSet` schema (see `card-set-format` spec).

#### Scenario: Card set JSON file produced per set
- **WHEN** `BuildRunner.Run` is called with two `CardSet` instances named `"core"` and `"expansion-1"`
- **THEN** `output/core.json` and `output/expansion-1.json` are written

#### Scenario: Existing output file is overwritten
- **WHEN** `BuildRunner.Run` is called and a JSON file for a set already exists at the output path
- **THEN** the existing file is overwritten without error

### Requirement: BuildRunner generates a Godot keyword constants file
`BuildRunner` SHALL emit a GDScript file (`archetype_keywords.gd`) containing a class with string constants for every keyword name registered in the `GameDefinition`. This provides Godot-side code completion and prevents typos when referencing keywords from GDScript.

#### Scenario: Keyword constant generated for each registered keyword
- **WHEN** the `GameDefinition` contains keywords `"attack"` and `"take-damage"`
- **THEN** `archetype_keywords.gd` contains constants `ATTACK = "attack"` and `TAKE_DAMAGE = "take-damage"` (name normalised to SCREAMING_SNAKE_CASE)

### Requirement: BuildRunner generates Godot signal definitions from the event log schema
`BuildRunner` SHALL emit a GDScript file (`game_events.gd`) defining signals derived from the registered keywords' event log contributions, following the Level 1 signal derivation rules from D30. Keywords that opt out of signal generation (via a builder flag) SHALL be excluded.

#### Scenario: Signal emitted for keyword with event log contribution
- **WHEN** the keyword `"take-damage"` contributes a `DamageTaken` event to the log and has not opted out
- **THEN** `game_events.gd` contains a `signal damage_taken(atom_id: int, amount: int)` declaration

#### Scenario: Opted-out keyword excluded from signals
- **WHEN** a keyword is registered with `.NoSignal()` in the builder
- **THEN** no signal is emitted for that keyword in `game_events.gd`
