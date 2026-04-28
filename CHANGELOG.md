# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.6.0] - 2026-04-29

### Added
- Generated `CardAtom`, `ZoneAtom`, `PlayerAtom`, `SessionAtom` GDScript classes now expose the full atom view:
  - `CardAtom`: typed `get_zone() -> ZoneAtom`, `get_owner() -> PlayerAtom` (alongside raw ID getters)
  - `ZoneAtom`: `get_owner() -> PlayerAtom`, `get_cards() -> Array[CardAtom]`
  - `PlayerAtom`: `get_zones() -> Array[ZoneAtom]`, `get_cards() -> Array[CardAtom]`
  - `SessionAtom`: `get_turn_number()`, `get_phase_index()`, `get_current_phase_name()`
  - All atom types: `get_definition_name()`, `get_static_property(key)`
- Discovery methods on all atom types: `get_static_property_keys()`, `get_accumulators()`, `get_active_conditions()`, `get_modifier_keys()`
- `ArchetypeInterop` typed list getters: `get_all_cards()`, `get_all_zones()`, `get_all_players()`
- `ArchetypeInterop` game definition metadata: `get_card_definition_names()`, `get_zone_definition_names()`, `get_keyword_names()`, `get_current_phase_name()`
- `GameStateView` gains `GetDefinitionName`, `GetStaticProperty`, `GetStaticPropertyKeys`, `GetAccumulators`, `GetActiveConditions`, `GetModifierKeys`, `GetCurrentPhaseName`, `GetCardDefinitionNames`, `GetZoneDefinitionNames`, `GetKeywordNames`
- `IGameStateReadable` gains `GetAccumulators`, `GetActiveConditions`, `GetModifierKeys`
- All generated GDScript methods have `##` docstrings

## [0.4.0] - 2026-04-28

### Added
- `Archetype.Build.Extensions` NuGet package — fluent builder classes for all major game definition types: `KeywordDefinitionBuilder`, `CardDefinitionBuilder`, `EffectBlockBuilder`, `PhaseDefinitionBuilder`, `ZoneDefinitionBuilder`, `PlayerDefinitionBuilder`, `NamedEffectBlockDefBuilder`, `StaticEffectDefBuilder`, `TriggerDefinitionBuilder`
- `Action<Builder>` overloads throughout — `GameDefinitionBuilder.AddCard/AddZone/AddPlayer/AddPhase/RegisterKeyword`, `CardDefinitionBuilder.AddEffect/AddStaticEffect`, and nested effect/trigger builders all accept callback-style configuration
- `StaticEffectDefBuilder` lifetime helpers: `Permanent()`, `ForTurns(n)`, `While(expr)`
- `GameDefinitionBuilder.AddZone(ZoneDefinition)` and `AddPlayer(string, PlayerDefinition)` direct overloads

### Removed
- Electron desktop tooling (`tooling/`) — scrapped in favour of a future lightweight approach

## [0.3.0] - 2026-03-23

### Added
- Game state is now queryable from GDScript via `ArchetypeInterop`: `get_accumulator`, `has_condition`, `get_computed_property`, `get_zone`, `get_atom_owner`, `get_kind`, `get_atoms` (D38)
- `archetype_atom_kinds.gd` — `ArchetypeAtomKinds` integer constants (`CARD`, `ZONE`, `PLAYER`, `SESSION`) for use with `get_atoms`/`get_kind` (D39)
- Typed GDScript atom view classes — `CardAtom`, `ZoneAtom`, `PlayerAtom`, `SessionAtom` — with pull-model property getters generated from state map declarations; factory methods `get_card(id)`, `get_zone_atom(id)`, `get_player(id)`, `get_session()` on `ArchetypeInterop` (D40)
- `StateFieldDecl`/`StateFieldType` — explicit state map declarations (`Number` accumulators, `Bool` conditions) on all four atom definition types; `GameDefinitionBuilder.Build()` validates keyword invocations against declared fields at build time

### Fixed
- Generated `archetype_interop.gd` now connects to C# signals using PascalCase names as required by Godot 4's C# interop — snake_case names silently fail at runtime
- `PromptRequested` and `GameError` lifecycle signals were missing from the autoload; both are now connected and re-emitted
- `KeywordNodeConverter.DeserializeLiteral` passed an unadvanced `Utf8JsonReader` to `LiteralConverter.Read`, causing a runtime exception when a `Literal` appeared as a keyword argument
- `GameSessionBuilder.Build()` now throws `DefinitionException` when `GameDefinition.Phases` is empty
- Generated `start()` always passes all three arguments to `StartGame` — GDScript cannot use C# default parameters
- Renamed `get_atom_owner`/`GetAtomOwner` (was `get_owner`/`GetOwner`) — the old name shadowed a Godot `Node` built-in causing a GDScript signature mismatch error

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
