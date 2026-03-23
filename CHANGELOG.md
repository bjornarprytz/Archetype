# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.3.0-alpha.5] - 2026-03-23

### Added
- Generated `ArchetypeNode` now stores `GameStateView` and exposes seven GDScript-callable state query methods: `GetAccumulator`, `HasCondition`, `GetComputedProperty`, `GetZone`, `GetOwner`, `GetKind`, `GetAtoms` (D38)
- `archetype_interop.gd` forwards all seven query methods as snake_case GDScript functions
- New generated artifact `archetype_atom_kinds.gd` exposes `ArchetypeAtomKinds` integer constants (`CARD`, `ZONE`, `PLAYER`, `SESSION`) for use with `get_atoms` and `get_kind` (D39)

## [0.3.0-alpha.4] - 2026-03-22

### Fixed
- Generated `archetype_interop.gd` `start()` now always passes all three arguments to `StartGame` — GDScript cannot use C# default parameters, so the two-argument call path was an invalid call signature

## [0.3.0-alpha.3] - 2026-03-22

### Fixed
- `GameSessionBuilder.Build()` now throws `DefinitionException` when `GameDefinition.Phases` is empty — a game with no phases produces no action windows and would spin synchronously, blocking the Godot main thread

## [0.3.0-alpha.2] - 2026-03-22

### Fixed
- `KeywordNodeConverter.DeserializeLiteral` passed a fresh `Utf8JsonReader` (at `TokenType.None`) to `LiteralConverter.Read`, which expects `StartObject` — adding `reader.Read()` before the delegate call fixes the runtime exception when a `Literal` appears as an argument inside an `Invocation`

## [0.3.0-alpha.1] - 2026-03-22

### Fixed
- Generated `archetype_interop.gd` now connects to C# signals using PascalCase names (`ActionRequested`, `ActionResolved`, etc.) as required by Godot 4's C# interop — snake_case names silently fail at runtime
- `PromptRequested` and `GameError` lifecycle signals were declared on `ArchetypeNode` but not forwarded by the autoload; both are now connected and re-emitted
- `docs/architecture.md` D33 signal list now includes `GameErrorEventHandler`

## [0.2.0] - 2026-03-22

### Added
- NuGet release pipeline: auto-publishes `Archetype.Core`, `Archetype.Engine`, `Archetype.Text`, and `Archetype.Build` on merge to main when the version changes
- Version-check CI workflow: blocks preview versions from merging to main, enforces version bumps for `src/` changes, requires changelog entries for new versions
- README with full Godot integration guide (setup steps, signal wiring, `ArchetypeInterop.start()` usage)

### Fixed
- `[GlobalClass]` attribute added to generated `ArchetypeNode` so GDScript can use it as a type annotation
- `AtomId` implicit conversions (`long ↔ AtomId`) fix generated cast chain in `ArchetypeNode.cs`
- `game_definition.json` now serialises the rules-only definition (not the cards-merged definition) preventing duplicate card name exceptions at runtime
- `StartGame` now loads `game_definition.json` and card sets from the filesystem, making it callable from GDScript

### Removed
- PoC sample game (`poc/SampleGame/`)

## [0.1.0] - 2026-03-22

### Added
- Core domain model: `GameDefinition`, `CardDefinition`, `CardSet`, `AtomId`, `EffectBlockDef`
- Execution engine: `GameSession`, `EventLog`, `BlockExecutor`, `TriggerResolver`, built-in keywords
- Card text rendering: `TextRenderer`
- Godot export pipeline: `BuildRunner.Run` emits `archetype/` subdirectory with `ArchetypeNode.cs`, GDScript interop files, `game_definition.json`, and `archetype/card-sets/*.json`
- Generated `ArchetypeNode` bridges the async game loop to GDScript via signals
- `ArchetypeInterop` autoload with `start()` convenience method for zero-config game startup
- `AtomId` implicit conversions to/from `long` for Godot interop
- GDScript `##` documentation comments on all generated files
- Multi-target `net8.0` and `net10.0` for Godot 4.x compatibility
