# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

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
