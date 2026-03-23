---
status: draft
owner: implementer
last-updated: 2026-03-24
depends-on:
  - docs/architecture.md
  - openspec/changes/state-map-declarations/requirements.md
  - src/Archetype.Core/GameDefinition.cs
  - src/Archetype.Build/GameDefinitionBuilder.cs
  - src/Archetype.Build/GodotEmitter.cs
---

# State Map Declarations — Task List

All work targets the `impl/d38-d39-state-query-atomkind` branch. Tasks are
ordered by dependency: core model lands first, then builder validation, then
the GDScript emission step. All tasks in a numbered group may proceed in
parallel unless a dependency is called out explicitly.

---

## Group 1 — Core model: `StateFieldDecl` and declaration fields

### 1.1  Add `StateFieldType` enum and `StateFieldDecl` record
  - reads: `openspec/changes/state-map-declarations/requirements.md#R1`,
    `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Core/StateFieldDecl.cs` (new file)
  - Add `StateFieldType { Number, Bool }` enum.
  - Add `StateFieldDecl(Name: string, FieldType: StateFieldType, TextTemplate: string? = null)`
    record. XML-doc each member. `TextTemplate` follows the same convention as
    `KeywordDefinition.TextTemplate` — optional localizable natural-language
    description of what the field represents.

### 1.2  Add `StateMapDeclarations` to `CardDefinition`
  - reads: `openspec/changes/state-map-declarations/requirements.md#R1`,
    `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Core/GameDefinition.cs`
  - Add `IReadOnlyList<StateFieldDecl> StateMapDeclarations` as an optional
    positional parameter on `CardDefinition` (default `null`; treat null as
    empty). Update XML-doc. All existing `CardDefinition` construction sites
    remain valid without change.

### 1.3  Add `StateMapDeclarations` to `ZoneDefinition`
  - reads: `openspec/changes/state-map-declarations/requirements.md#R1`,
    `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Core/GameDefinition.cs`
  - Same pattern as 1.2 for `ZoneDefinition`. Existing callers unaffected.

### 1.4  Add `StateMapDeclarations` to `PlayerDefinition`
  - reads: `openspec/changes/state-map-declarations/requirements.md#R1`,
    `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Core/GameDefinition.cs`
  - Same pattern as 1.2 for `PlayerDefinition`. Existing callers unaffected.

### 1.5  Add `SessionStateMapDeclarations` to `GameDefinition`
  - reads: `openspec/changes/state-map-declarations/requirements.md#R1`,
    `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Core/GameDefinition.cs`
  - Add `IReadOnlyList<StateFieldDecl>? SessionStateMapDeclarations = null`
    as an optional parameter on `GameDefinition`. Treat null as empty. Update
    XML-doc to note that the two engine-reserved session fields (`turn-number`,
    `phase-index`) are implicitly declared and must not appear here.

---

## Group 2 — Builder API extensions

Depends on Group 1.

### 2.1  `GameDefinitionBuilder.AddZone` — `stateMapDeclarations` overload
  - reads: `openspec/changes/state-map-declarations/requirements.md#R3`,
    `src/Archetype.Build/GameDefinitionBuilder.cs`,
    `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Build/GameDefinitionBuilder.cs`
  - Add overload `AddZone(string name, IReadOnlyDictionary<string, object> staticProperties, IReadOnlyList<StateFieldDecl>? stateMapDeclarations = null)`.
    Existing single-argument call sites compile without change. Store declarations
    on the `ZoneDefinition` record.

### 2.2  `GameDefinitionBuilder.AddPlayer` — `stateMapDeclarations` overload
  - reads: `openspec/changes/state-map-declarations/requirements.md#R3`,
    `src/Archetype.Build/GameDefinitionBuilder.cs`,
    `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Build/GameDefinitionBuilder.cs`
  - Same pattern as 2.1 for `AddPlayer`.

### 2.3  `GameDefinitionBuilder.WithSessionStateMap`
  - reads: `openspec/changes/state-map-declarations/requirements.md#R3`,
    `src/Archetype.Build/GameDefinitionBuilder.cs`
  - writes: `src/Archetype.Build/GameDefinitionBuilder.cs`
  - Add `WithSessionStateMap(IReadOnlyList<StateFieldDecl> declarations)`
    fluent method. Stored and passed through to `GameDefinition` in `Build()`.

---

## Group 3 — Builder validation: field name checks

Depends on Group 2 (builder must carry declarations into `Build()`).

### 3.1  Add `StateMapValidator` helper
  - reads: `openspec/changes/state-map-declarations/requirements.md#R2`,
    `src/Archetype.Build/GameDefinitionBuilder.cs`,
    `src/Archetype.Core/Keywords.cs`,
    `src/Archetype.Core/BuiltInKeywords.cs`,
    `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Build/StateMapValidator.cs` (new file)
  - Static class with one public entry point:
    `Validate(GameDefinition definition)` — iterates all keyword bodies, card
    effect blocks (primary, additional, cost), and static effect blocks. For
    each `Invocation` node whose `KeywordName` is one of the six built-in
    field-name keywords (`modify-accumulator`, `clear-accumulator`,
    `apply-condition`, `remove-condition`, `apply-modifier`, `get-state`):
      1. Resolve the static atom kind of the first argument.
      2. If resolvable, look up the declaration set for that kind.
      3. If the field-name argument is a `Str` literal node, check it against
         the declaration set with the required `FieldType` for the operation.
      4. Throw `DefinitionException` on mismatch, identifying the keyword,
         field name, and atom kind.
    Skip checks when: the atom kind cannot be resolved; the field-name argument
    is not a `Str` literal; the field is one of the two implicitly declared
    session reserved fields.

### 3.2  Wire `StateMapValidator` into `GameDefinitionBuilder.Build()`
  - reads: `openspec/changes/state-map-declarations/requirements.md#R2`,
    `src/Archetype.Build/GameDefinitionBuilder.cs`,
    `src/Archetype.Build/StateMapValidator.cs`
  - writes: `src/Archetype.Build/GameDefinitionBuilder.cs`
  - Call `StateMapValidator.Validate(definition)` in `Build()` after the
    existing keyword-shadow and parameter-ref checks, before returning the
    definition. The call takes the fully-assembled `GameDefinition` (with merged
    built-ins) so all keyword bodies are reachable.

---

## Group 4 — Tests: model and validation

Depends on Group 3.

### 4.1  Tests for `StateFieldDecl` model
  - reads: `src/Archetype.Core/StateFieldDecl.cs`
  - writes: `tests/Archetype.Tests/StateMap/StateFieldDeclTests.cs` (new file)
  - Tests:
    - `StateFieldDecl_DefaultTextTemplate_IsNull`
    - `StateFieldDecl_Equality_SameName_SameType_Equal`
    - `StateFieldDecl_Equality_DifferentType_NotEqual`

### 4.2  Tests for builder API extensions
  - reads: `src/Archetype.Build/GameDefinitionBuilder.cs`,
    `src/Archetype.Core/GameDefinition.cs`
  - writes: `tests/Archetype.Tests/StateMap/StateMapBuilderTests.cs` (new file)
  - Tests:
    - `AddZone_WithDeclarations_StoredOnDefinition`
    - `AddZone_WithoutDeclarations_EmptyList`
    - `AddPlayer_WithDeclarations_StoredOnDefinition`
    - `WithSessionStateMap_StoredOnGameDefinition`
    - `AddCard_WithDeclarations_StoredViaRecord`

### 4.3  Tests for builder validation
  - reads: `src/Archetype.Build/StateMapValidator.cs`,
    `src/Archetype.Build/GameDefinitionBuilder.cs`,
    `src/Archetype.Core/BuiltInKeywords.cs`
  - writes: `tests/Archetype.Tests/StateMap/StateMapValidatorTests.cs` (new file)
  - Tests:
    - `ModifyAccumulator_UndeclaredField_ThrowsDefinitionException`
    - `ModifyAccumulator_DeclaredNumberField_Passes`
    - `ApplyCondition_UndeclaredField_ThrowsDefinitionException`
    - `ApplyCondition_DeclaredBoolField_Passes`
    - `ApplyCondition_WrongType_NumberField_ThrowsDefinitionException`
    - `ClearAccumulator_UndeclaredField_ThrowsDefinitionException`
    - `GetState_DeclaredNumberField_Passes`
    - `GetState_DeclaredBoolField_Passes`
    - `GetState_UndeclaredField_ThrowsDefinitionException`
    - `ApplyModifier_UndeclaredField_ThrowsDefinitionException`
    - `RemoveCondition_UndeclaredField_ThrowsDefinitionException`
    - `Validation_GenericAtomParam_Skipped` (no false positive for Atom-typed params)
    - `Validation_NonLiteralFieldArg_Skipped` (Kw.Param reference not checked)
    - `Validation_SessionReservedField_TurnNumber_Passes` (implicit declaration)
    - `Validation_SessionReservedField_PhaseIndex_Passes`

---

## Group 5 — GDScript atom view generation (D40)

Depends on Group 1 (declarations on definitions). Does not depend on Groups 2–4.

### 5.1  `GodotEmitter.EmitAtomViews` — four atom view files
  - reads: `openspec/changes/state-map-declarations/requirements.md#R4`,
    `src/Archetype.Build/GodotEmitter.cs`,
    `src/Archetype.Core/GameDefinition.cs`,
    `src/Archetype.Core/StateFieldDecl.cs`
  - writes: `src/Archetype.Build/GodotEmitter.cs`
  - Add `public static void EmitAtomViews(GameDefinition definition, string outputDir)`.
    For each atom kind:
      1. Compute the union of `StateMapDeclarations` across all definitions of
         that kind (collision on same name + different type raises
         `InvalidOperationException` — should have been caught by `Build()`).
      2. Emit the corresponding `*_atom.gd` file per the template in R4:
         `extends RefCounted`, private `_atom_id`, `_create` factory, typed
         getters for `Number` fields (`get_<snake>() -> float`), typed predicates
         for `Bool` fields (`has_<snake>() -> bool`), structural getters.
      3. Field names are converted to `snake_case` via the existing
         `ToSnakeCase`/`ToScreamingSnakeCase` helper (or a new private helper if
         that exact method does not exist).
    Output files: `card_atom.gd`, `zone_atom.gd`, `player_atom.gd`,
    `session_atom.gd`.

### 5.2  Extend `archetype_interop.gd` emission with factory methods
  - reads: `openspec/changes/state-map-declarations/requirements.md#R4`,
    `src/Archetype.Build/GodotEmitter.cs`
  - writes: `src/Archetype.Build/GodotEmitter.cs`
  - In the method that emits `archetype_interop.gd` (`EmitInteropScripts`),
    append the four factory methods after the existing state-query forwarding
    methods:
      - `get_card(atom_id: int) -> CardAtom`
      - `get_zone(atom_id: int) -> ZoneAtom`
      - `get_player(atom_id: int) -> PlayerAtom`
      - `get_session() -> SessionAtom` (singleton; uses a `var _session_atom`
        field initialized to `null`; first call resolves via `get_atoms` +
        `AtomKind.SESSION` constant)
    Existing methods are unchanged.

### 5.3  Wire `EmitAtomViews` into `BuildRunner.Run`
  - reads: `openspec/changes/state-map-declarations/requirements.md#R4`,
    `src/Archetype.Build/BuildRunner.cs`,
    `src/Archetype.Build/GodotEmitter.cs`
  - writes: `src/Archetype.Build/BuildRunner.cs`
  - Add `GodotEmitter.EmitAtomViews(fullDefinition, outputDir)` call in
    `BuildRunner.Run` alongside the existing `EmitKeywordConstants`,
    `EmitSignals`, `EmitArchetypeNode`, and `EmitInteropScripts` calls.

### 5.4  Tests for atom view emission
  - reads: `src/Archetype.Build/GodotEmitter.cs`,
    `src/Archetype.Core/StateFieldDecl.cs`
  - writes: `tests/Archetype.Tests/GodotEmitter/AtomViewEmitterTests.cs` (new file)
  - Tests:
    - `EmitAtomViews_NumberField_GeneratesGetterMethod`
    - `EmitAtomViews_BoolField_GeneratesPredicateMethod`
    - `EmitAtomViews_CardAtom_StructuralGetters_AlwaysPresent` (zone_id, owner_id)
    - `EmitAtomViews_ZoneAtom_OwnerId_Present_ZoneId_Absent`
    - `EmitAtomViews_SessionAtom_NoOwnerId_NoZoneId`
    - `EmitAtomViews_UnionAcrossDefinitions_DeduplicatesMatchingFields`
    - `EmitAtomViews_FactoryMethods_AppendedToInterop`
    - `EmitAtomViews_GetSession_SingletonPattern_InEmittedCode`
    - `EmitAtomViews_EmptyDeclarations_OnlyStructuralGetters`
    - `EmitAtomViews_FieldNameToSnakeCase_HyphenatedName`

---

## Completion criteria

- All existing tests continue to pass without modification.
- New tests in Groups 4 and 5 all pass.
- `GameDefinitionBuilder.Build()` throws `DefinitionException` for any
  `modify-accumulator`, `clear-accumulator`, `apply-condition`,
  `remove-condition`, `apply-modifier`, or `get-state` invocation that
  references an undeclared field on a resolvable atom type.
- `GodotEmitter.EmitAtomViews` produces four syntactically valid GDScript files
  containing the correct typed getters and structural fields.
- `archetype_interop.gd` includes all four factory methods including the
  `get_session()` singleton pattern.
- `BuildRunner.Run` calls `EmitAtomViews` as part of the standard emission step.
- No changes to `Archetype.Engine` or any runtime execution path.
