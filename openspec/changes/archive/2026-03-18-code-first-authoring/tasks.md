## 1. Architecture Documentation

- [x] 1.1 Amend D2 in `docs/architecture.md`: remove parser ownership claim, add builder API as authoring surface, preserve JSON serialization boundary for card sets
  - reads: `openspec/changes/code-first-authoring/specs/architecture-consistency/spec.md`, `docs/architecture.md#D2`
  - writes: `docs/architecture.md`
- [x] 1.2 Supersede D26–D31 in `docs/architecture.md`: replace with code-first authoring model section documenting `Kw` class, `GameDefinitionBuilder`, `CardSet`, `BuildRunner`, and two-layer deployment
  - reads: `openspec/changes/code-first-authoring/design.md`, `docs/architecture.md#D26`
  - writes: `docs/architecture.md`

## 2. Remove Tooling Projects

- [x] 2.1 Delete `tooling/` (Electron application)
  - reads: `openspec/changes/code-first-authoring/proposal.md`
  - writes: `tooling/` (deleted)
- [x] 2.2 Delete `src/Archetype.Tooling.Server/` project and remove from solution
  - reads: `openspec/changes/code-first-authoring/proposal.md`
  - writes: `src/Archetype.Tooling.Server/` (deleted), `Archetype.sln`

## 3. Builder API — Kw Static Class

- [x] 3.1 Create `src/Archetype.Core/Builder/Kw.cs`: static class exposing all D12/D14 built-in keywords as typed static methods returning `KeywordNode`
  - reads: `openspec/changes/code-first-authoring/specs/builder-api/spec.md`, `docs/architecture.md#D12`, `docs/architecture.md#D14`
  - writes: `src/Archetype.Core/Builder/Kw.cs`
- [x] 3.2 Write unit tests for `Kw` static methods: verify each returns correct `KeywordNode` kind with correct arguments
  - reads: `src/Archetype.Core/Builder/Kw.cs`
  - writes: `tests/Archetype.Core.Tests/Builder/KwTests.cs`

## 4. Builder API — GameDefinitionBuilder

- [x] 4.1 Create `src/Archetype.Core/Builder/GameDefinitionBuilder.cs`: fluent builder for registering zones, phases, keywords, state-based rules, trigger resolution order; `Build()` returns `GameDefinition`
  - reads: `openspec/changes/code-first-authoring/specs/builder-api/spec.md`, `openspec/changes/code-first-authoring/design.md#D-CFA-1`
  - writes: `src/Archetype.Core/Builder/GameDefinitionBuilder.cs`
- [x] 4.2 Implement `RegisterKeyword(name, method, textTemplate)` on `GameDefinitionBuilder`: wraps a C# method as a named keyword entry, validates parameter references at `Build()` time
  - reads: `src/Archetype.Core/Builder/GameDefinitionBuilder.cs`, `openspec/changes/code-first-authoring/specs/builder-api/spec.md`
  - writes: `src/Archetype.Core/Builder/GameDefinitionBuilder.cs`
- [x] 4.3 Write unit tests for `GameDefinitionBuilder`: valid build, duplicate keyword name rejection, unknown parameter reference rejection
  - reads: `src/Archetype.Core/Builder/GameDefinitionBuilder.cs`
  - writes: `tests/Archetype.Core.Tests/Builder/GameDefinitionBuilderTests.cs`

## 5. Card Set Format

- [x] 5.1 Create `src/Archetype.Core/CardSet.cs`: `CardSet` record with name, format version, and `IReadOnlyList<CardSpec>`; JSON-serializable using System.Text.Json
  - reads: `openspec/changes/code-first-authoring/specs/card-set-format/spec.md`, `docs/architecture.md#D29`
  - writes: `src/Archetype.Core/CardSet.cs`
- [x] 5.2 Implement `GameDefinition.WithCardSets(IEnumerable<CardSet>)`: additive merge, duplicate card name detection, keyword reference validation
  - reads: `openspec/changes/code-first-authoring/specs/card-set-format/spec.md`, `src/Archetype.Core/CardSet.cs`
  - writes: `src/Archetype.Core/GameDefinition.cs`
- [x] 5.3 Write unit tests for `WithCardSets`: successful merge, duplicate card name error, unknown keyword reference error, multi-set merge
  - reads: `src/Archetype.Core/GameDefinition.cs`
  - writes: `tests/Archetype.Core.Tests/CardSetTests.cs`

## 6. Archetype.Build Library

- [x] 6.1 Create `src/Archetype.Build/` project: class library referencing `Archetype.Core`; add to solution
  - reads: `openspec/changes/code-first-authoring/design.md#D-CFA-4`
  - writes: `src/Archetype.Build/Archetype.Build.csproj`, `Archetype.sln`
- [x] 6.2 Implement `BuildRunner.Run(GameDefinition, IEnumerable<CardSet>, string outputDir)`: creates output dir, serializes each card set to `[name].json`
  - reads: `openspec/changes/code-first-authoring/specs/game-definition-build-tool/spec.md`, `src/Archetype.Core/CardSet.cs`
  - writes: `src/Archetype.Build/BuildRunner.cs`
- [x] 6.3 Implement Godot keyword constants file generation in `BuildRunner`: emits `archetype_keywords.gd` with SCREAMING_SNAKE_CASE constants for all registered keyword names
  - reads: `openspec/changes/code-first-authoring/specs/game-definition-build-tool/spec.md`, `src/Archetype.Build/BuildRunner.cs`
  - writes: `src/Archetype.Build/BuildRunner.cs`, `src/Archetype.Build/GodotEmitter.cs`
- [x] 6.4 Implement Godot signal definitions file generation in `BuildRunner`: emits `game_events.gd` with Level 1 signals from keyword event log contributions; respects `.NoSignal()` opt-out
  - reads: `openspec/changes/code-first-authoring/specs/game-definition-build-tool/spec.md`, `docs/architecture.md#D30`, `src/Archetype.Build/GodotEmitter.cs`
  - writes: `src/Archetype.Build/GodotEmitter.cs`
- [x] 6.5 Write unit tests for `BuildRunner`: output directory creation, per-set JSON files, keyword constants file content, signal file content, opt-out exclusion
  - reads: `src/Archetype.Build/BuildRunner.cs`, `src/Archetype.Build/GodotEmitter.cs`
  - writes: `tests/Archetype.Build.Tests/BuildRunnerTests.cs`

## 7. Documentation and Status

- [x] 7.1 Update `docs/implementation-status.md` to reflect removal of tooling projects and addition of `Archetype.Builder` and `Archetype.Build`
  - reads: `docs/implementation-status.md`
  - writes: `docs/implementation-status.md`
