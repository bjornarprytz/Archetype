## Why

The Electron-based authoring tool (D26–D31) was too heavyweight for its purpose: the primary game creator is a developer who is fluent in C# and GDScript and prefers to express game logic in code rather than a custom DSL. Replacing the DSL parser and GUI with a C# builder API removes the tooling complexity while preserving the dual-use invariant — `KeywordNode` trees remain the single data structure that drives both execution and text rendering, they are just constructed in C# instead of parsed from text.

## What Changes

- **BREAKING** — The DSL and its parser are removed. Keywords and game rules are authored in C# using a builder API.
- **BREAKING** — D26–D31 (Electron app, sidecar server, sidecar protocol) are superseded entirely.
- D2 is amended: parser ownership moves from "tooling" to "not applicable"; the builder API is the new authoring surface.
- A two-layer authoring model is introduced: rules (compiled C# binary) and card sets (C# authored, JSON distributed).
- A console application replaces the Electron app as the build/export tool. It outputs card set JSON and Godot interop artifacts.
- The engine, `KeywordNode` tree structure, text renderer, and Godot runtime experience are unchanged.

## Capabilities

### New Capabilities

- `builder-api`: C# fluent API for constructing `KeywordNode` trees and composing game rules (keywords, phases, zones, turn structure, state-based rules). Built-in keywords are exposed as static typed methods. Game-creator keywords are ordinary C# methods that return `KeywordNode` trees. The rules layer calls `BuildDefinition()` to produce a `GameDefinition` in memory at startup — no JSON round-trip for rules.
- `card-set-format`: `CardSet` as a first-class concept. Card sets are authored in C# against the published rules assembly, then serialized to JSON by the build tool. JSON card sets are loaded at runtime and merged into the in-memory `GameDefinition`. Multiple sets can be loaded independently, enabling separate distribution of base sets and expansions.
- `game-definition-build-tool`: A console application (`Archetype.Build`) that takes a rules project and one or more card set projects as input and outputs: card set JSON files and Godot interop artifacts (signal definitions, GDScript wrappers).

### Modified Capabilities

- `architecture-consistency`: D2 (parser ownership), D26–D31 (tooling platform decisions) are superseded by this change. Architecture doc requires corresponding amendments.

## Impact

- `Archetype.Tooling.Server` project: removed.
- `tooling/` Electron application: removed.
- New project: `Archetype.Build` (console app).
- New project: `Archetype.Builder` (class library — builder API and card set serialization).
- `Archetype.Core`: gains `GameDefinition.Merge(CardSet)` or equivalent load-time composition method.
- Godot integration: unchanged at the runtime level; interop artifacts are now emitted by `Archetype.Build` rather than the sidecar.

## Non-goals

- A GUI authoring tool of any kind (deferred indefinitely).
- DSL preservation or migration tooling.
- Hot-reload or live-editing of rules at runtime.
- Validation tooling beyond what the C# compiler and load-time checks already provide.

## Owners

- Technical architect: amend architecture doc (D2, supersede D26–D31), design builder API surface and card set format.
- Implementer: build `Archetype.Builder`, `Archetype.Build`, remove tooling projects, wire Godot interop.
