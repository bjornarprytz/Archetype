## ADDED Requirements

### Requirement: Architecture doc supersedes D26–D31 with code-first authoring model
`docs/architecture.md` SHALL replace the content of D26–D31 (Electron app, sidecar server, sidecar protocol, project file format, validation trigger model, Godot export via sidecar, missing-translation gate) with a new section documenting the code-first authoring model: builder API as the authoring surface, two-layer split (rules in binary, card sets as JSON), and `Archetype.Build` as the build tool.

#### Scenario: No references to Electron or sidecar in architecture doc
- **WHEN** a reader searches `docs/architecture.md` for "Electron", "sidecar", or "Archetype.Tooling.Server"
- **THEN** no current (non-superseded) decision references these concepts

#### Scenario: Code-first authoring model is documented
- **WHEN** a reader reads the authoring section of `docs/architecture.md`
- **THEN** they find decisions describing the `Kw` static class, `GameDefinitionBuilder`, `CardSet`, `BuildRunner`, and the two-layer deployment model

### Requirement: Architecture doc amends D2 to remove parser ownership from tooling
The D2 section of `docs/architecture.md` SHALL be amended to remove the statement that "the tooling owns the parser" and replace it with a statement that game rules are authored using the C# builder API. The remainder of D2 (keyword representation as interpreted expression trees, JSON serialization boundary for card sets, engine never sees raw DSL text) SHALL be preserved or updated to reflect that cards reference keywords by name in JSON.

#### Scenario: No parser ownership claim in D2
- **WHEN** a reader reads D2 in `docs/architecture.md`
- **THEN** the text does not state that a parser exists or that the tooling is responsible for parsing

#### Scenario: Builder API authoring surface described in D2
- **WHEN** a reader reads D2 in `docs/architecture.md`
- **THEN** the text states that `KeywordNode` trees are constructed via the C# builder API and that built-in keywords are exposed as static methods on the `Kw` class
