## ADDED Requirements

### Requirement: Built-in keywords exposed as typed static methods
`Archetype.Core` SHALL expose all built-in keywords (as enumerated in D12/D14) as static methods on a `Kw` static class. Each method SHALL accept typed `KeywordNode` parameters corresponding to the keyword's declared parameter list and SHALL return a `KeywordNode` representing the invocation.

#### Scenario: Built-in keyword call produces a KeywordNode
- **WHEN** a game creator calls `Kw.TakeDamage(atomNode, amountNode)`
- **THEN** the return value is a `KeywordNode` of kind `Invoke` referencing the `take-damage` built-in with the supplied arguments bound positionally

#### Scenario: Type mismatch is caught at compile time
- **WHEN** a game creator passes a `KeywordNode` typed `Int` where an `Atom`-typed parameter is expected
- **THEN** the C# compiler reports a type error before any runtime execution occurs

### Requirement: Game-creator keywords are C# methods returning KeywordNode
The builder API SHALL not require game creators to register keywords via a separate registration call. A game-creator keyword SHALL be expressible as an ordinary C# method (static or instance) that accepts `KeywordNode` parameters and returns a `KeywordNode` tree by composing built-in or other game-creator methods. The method body constitutes the keyword definition.

#### Scenario: Composite keyword composes built-ins
- **WHEN** a game creator defines `static KeywordNode Attack(KeywordNode atom, KeywordNode amount) => Kw.TakeDamage(atom, Kw.Max(Kw.Literal(0), Kw.Subtract(amount, Kw.Prop(atom, "defense"))))`
- **THEN** calling `Attack(atomNode, amountNode)` returns a `KeywordNode` tree equivalent to the DSL expression `take_damage(atom, max(0, subtract(amount, atom.defense)))`

#### Scenario: Game-creator keyword can be registered by name for card text
- **WHEN** a game creator registers a C# method as a named keyword via `GameDefinitionBuilder.RegisterKeyword("attack", Attack, textTemplate: "{atom} attacks for {amount}")`
- **THEN** the keyword is addressable by name in card text `RulesRef` nodes and in card spec effect blocks

### Requirement: GameDefinitionBuilder constructs GameDefinition in memory
`Archetype.Core` (or `Archetype.Builder`) SHALL provide a `GameDefinitionBuilder` class with a fluent API for registering the game's rules. Calling `Build()` on the builder SHALL return a fully validated `GameDefinition` instance. No file I/O SHALL occur during this process.

#### Scenario: Builder produces a valid GameDefinition
- **WHEN** a game creator chains registrations on `GameDefinitionBuilder` (zones, phases, keywords, state-based rules, trigger resolution order) and calls `Build()`
- **THEN** the return value is a `GameDefinition` that can be passed directly to `ActionResolver` without further transformation

#### Scenario: Builder rejects duplicate keyword names
- **WHEN** a game creator registers two keywords with the same name
- **THEN** `Build()` throws `DefinitionException` with a message identifying the duplicate name

### Requirement: Parameter references within keyword bodies are type-checked at build time
When a game creator constructs a keyword node tree that references a parameter (e.g., `Kw.Param("target")`), the builder SHALL validate that the parameter name exists in the enclosing keyword's declared parameter list and that its type is compatible with the position it is used in.

#### Scenario: Unknown parameter name rejected
- **WHEN** a keyword body references `Kw.Param("typo")` and no parameter named `"typo"` is declared
- **THEN** `Build()` throws `DefinitionException` identifying the unknown parameter reference

#### Scenario: Valid parameter reference accepted
- **WHEN** a keyword body references `Kw.Param("target")` and `"target"` is declared as an `Atom` parameter
- **THEN** `Build()` succeeds and the parameter node is included in the serialized tree
