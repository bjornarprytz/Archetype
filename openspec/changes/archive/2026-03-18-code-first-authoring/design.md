## Context

The original authoring model used a custom DSL parsed by an Electron-based desktop application (D26–D31). The DSL was the canonical form; the tooling parsed it into `KeywordNode` trees and serialized them to JSON for the engine to load. The motivation for this approach was a single source of truth that could drive both execution and text rendering.

The developer authoring games with Archetype is a C# developer. The DSL added a layer of indirection with no ergonomic benefit for this audience. The `KeywordNode` tree is the real source of truth — the DSL was just one way to construct it. C# can construct the same trees more expressively, with full IDE support and no parser.

## Goals / Non-Goals

**Goals:**
- Replace the DSL parser and Electron tooling with a C# builder API
- Preserve the `KeywordNode` tree as the canonical dual-use data structure (execution + text rendering)
- Establish a two-layer authoring model: rules compiled into the game binary, card sets distributed as JSON
- Provide a console app build tool that outputs card set JSON and Godot interop artifacts

**Non-Goals:**
- Changing engine behavior (`ActionResolver`, `GameState`, text renderer, event log)
- Providing a GUI authoring tool of any kind
- Supporting DSL migration or compatibility
- Hot-reload of rules at runtime

## Decisions

### D-CFA-1: Keywords as C# methods

Built-in keywords are exposed as static methods on a `Kw` static class in `Archetype.Core`. Each method accepts typed `KeywordNode` parameters and returns a `KeywordNode`. Game-creator keywords are plain C# methods (instance or static) that return `KeywordNode` trees by composing built-in and other game-creator methods.

```csharp
// Built-in (ships with Archetype.Core)
public static class Kw
{
    public static KeywordNode TakeDamage(KeywordNode atom, KeywordNode amount) { ... }
    public static KeywordNode MoveCard(KeywordNode card, KeywordNode zone) { ... }
}

// Game-creator keyword (in their rules project)
public static KeywordNode Attack(KeywordNode atom, KeywordNode amount) =>
    Kw.TakeDamage(atom, Kw.Max(0, Kw.Subtract(amount, Kw.Prop(atom, "defense"))));
```

**Alternatives considered:**
- *Fluent builder object*: `Keyword.Create("attack").Body(...).Text(...)`. Rejected: more verbose, weaker IDE support, no real advantage over methods.
- *Preserve DSL with a simpler CLI wrapper*: Rejected: still requires a parser; doesn't solve the fundamental impedance mismatch for a C# developer.

### D-CFA-2: Rules in binary, card sets as JSON

The rules layer is a C# class library (`[GameName].Rules`) that references `Archetype.Core`. It defines keywords, phases, zones, turn structure, state-based rules, and trigger resolution order. It exposes a `BuildDefinition()` method that constructs a `GameDefinition` in memory.

Card sets are authored in C# class library projects (`[GameName].[SetName]`) that reference the rules library. A build step serializes each card set to JSON. Godot loads the rules assembly as a compiled project dependency and loads card set JSON files from disk at startup.

```
[GameName].Rules.dll     → referenced by Godot project (compiled in)
[GameName].CoreSet.json  → loaded from disk at Godot startup
[GameName].Expansion.json→ loaded from disk at Godot startup
```

**Alternatives considered:**
- *Rules also as JSON*: Rejected — the developer owns the binary; round-tripping rules through JSON adds indirection with no distribution benefit.
- *Card sets as compiled assemblies*: Rejected — removes the ability to distribute sets independently without recompiling the game.

### D-CFA-3: CardSet type and load-time merge

A `CardSet` record holds a collection of `CardSpec` entries (name, effect blocks, costs, static effects, art metadata, localization). `GameDefinition` gains a `WithCardSets(IEnumerable<CardSet> sets)` factory/merge method that produces a new `GameDefinition` with the card definitions added. Keyword references within card specs are validated against the registered keywords at merge time; unknown references throw `DefinitionException`.

Card set JSON is the serialized form of `CardSet`. The schema is the same JSON serialization already defined for `CardSpec` in D29.

### D-CFA-4: Archetype.Build console app

`Archetype.Build` is a class library exposing a `BuildRunner` API, not a standalone opinionated console app. The game developer writes their own `Program.cs` that:
1. Calls `[GameName].Rules.BuildDefinition()` to get the base definition
2. Constructs card set instances
3. Calls `BuildRunner.Run(definition, sets, outputDir)` to serialize JSON and generate Godot artifacts

This keeps `Archetype.Build` reusable and avoids prescribing a CLI interface that would need to understand arbitrary game project structures.

**Alternatives considered:**
- *Standalone CLI with file-path arguments*: Rejected — cannot reference game-specific C# assemblies without a plugin/reflection model; programmatic API is simpler and more reliable.

### D-CFA-5: Godot interop artifact generation

`BuildRunner` generates Godot interop artifacts from the `GameDefinition`:
- A GDScript constants file with all registered keyword names and card definition names as typed constants
- The Level 1 signal derivation (from D30) emitted as GDScript signal definitions on a `GameEvents` autoload

The card importer (from D30) is generated per set, keyed to the set's JSON filename.

### D-CFA-6: Removal of tooling projects

`Archetype.Tooling.Server` and `tooling/` (the Electron application) are deleted. These were never shipped or published. No migration path is needed. D26–D31 in `docs/architecture.md` are superseded by a new section documenting the code-first authoring model.

## Risks / Trade-offs

- **No visual authoring**: Card creators must write C# or JSON directly. Acceptable — the target audience is a developer. A future GUI can be added if needed.
- **Card set JSON is not human-friendly to hand-write**: The `KeywordNode` JSON schema is verbose. Mitigated by D-CFA-1 — card sets are always authored in C# and serialized; no one hand-writes the JSON.
- **Rules changes require recompilation**: Unlike a DSL tooling approach where rules could be data, changing rules requires a new binary. Acceptable — this is a game where the developer controls the full stack.
- **Keyword name string coupling in card specs**: Card specs reference keywords by string name. A rename without updating card sets causes a load-time `DefinitionException`. Mitigated by load-time validation catching this immediately.
