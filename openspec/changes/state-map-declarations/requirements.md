---
status: draft
owner: requirements-analyst
last-updated: 2026-03-24
depends-on:
  - docs/domain-model.md
  - docs/architecture.md
  - src/Archetype.Core/GameDefinition.cs
---

# State Map Declarations — Requirements

## Purpose

The domain model (§2.6–2.7 of `docs/domain-model.md`) specifies that every atom
type definition carries explicit **state map declarations** — `(name, type)` pairs
declaring mutable runtime fields: accumulators (`Number`) and conditions
(`Boolean`). The built-in keywords `modify-accumulator`, `clear-accumulator`,
`apply-condition`, `remove-condition`, `apply-modifier`, and `get-state` are all
specified to validate against declared fields at authoring time, with access to
an undeclared field name being an authoring-time error.

The C# implementation is missing this entirely. `CardDefinition`, `ZoneDefinition`,
and `PlayerDefinition` carry `StaticProperties` but no `StateMapDeclarations`.
The engine stores accumulators and conditions via string-keyed dictionaries with
zero validation at build time. The `archetype_interop.gd` queries
(`get_accumulator`, `has_condition`) are untyped: any string key is accepted at
all layers.

This change closes the gap in three coordinated steps: model, validation, and
generated GDScript atom view classes.

---

## Scope

### R1 — Core Model: `StateFieldDecl` and `StateMapDeclarations`

A new value type `StateFieldDecl` carries a field name, its type, and an optional
localizable text template. It is added to `CardDefinition`, `ZoneDefinition`,
`PlayerDefinition`, and `GameDefinition` (session-level extension), completing
the domain model's atom type definition contract.

**Specifics:**

- `StateFieldType` — an enum with two members: `Number` (accumulator or
  modifier-adjusted property) and `Bool` (condition/tag).
- `StateFieldDecl(Name: string, FieldType: StateFieldType, TextTemplate: string? = null)`
  — a record carrying one declaration. `TextTemplate` is the localizable
  natural-language description of what this state field represents (e.g.
  `"current health"`, `"is shielded"`). It follows the same convention as
  `KeywordDefinition.TextTemplate` and `CostDef.TextTemplate` — optional, used
  for card text rendering and tooling display; null means no rules text is shown
  for this field.
- `CardDefinition` gains `StateMapDeclarations: IReadOnlyList<StateFieldDecl>`.
- `ZoneDefinition` gains `StateMapDeclarations: IReadOnlyList<StateFieldDecl>`.
- `PlayerDefinition` gains `StateMapDeclarations: IReadOnlyList<StateFieldDecl>`.
- `GameDefinition` gains `SessionStateMapDeclarations: IReadOnlyList<StateFieldDecl>`
  for session atom extension (per §2.4: "game creators may declare additional
  state fields on the session atom type definition"). The two engine-reserved
  session fields (`turn-number`, `phase-index`) are implicitly declared and are
  not included in `SessionStateMapDeclarations`.

**Field name rules (per §2.6):**

- Field names within a single `StateMapDeclarations` list must be unique.
- A `Number`-typed state field that shares its name with a `Number`-typed static
  property denotes the modifier-adjusted computed value of that property. This
  is valid; no duplication error is raised for the shared name, but the static
  property must also be `Number`-typed.

**Constraints:**

- `StateMapDeclarations` is optional in the sense that existing callers who do
  not supply it should not be broken. The field defaults to an empty list.
- `StaticProperties` on all three definition types is unchanged — these are
  design-time read-only values. `StateMapDeclarations` is strictly separate.

---

### R2 — Builder Validation: Field Name Checks at `Build()` Time

`GameDefinitionBuilder.Build()` gains a build-time validation pass that checks
all `modify-accumulator`, `clear-accumulator`, `apply-condition`,
`remove-condition`, `apply-modifier`, and `get-state` invocations across every
keyword body, card effect block, cost body, and static effect block against the
declared state map fields of their target atom type.

**What is validated:**

For each invocation of a field-name-requiring built-in keyword, the validator:

1. Resolves the target atom argument's static type (the declared type of the
   parameter or the resolved return type of an invocation). If the static type
   cannot be resolved to a specific atom kind (`Card`, `Zone`, `Player`, or
   `Session`), the check is skipped — no false positives for generic `Atom`-typed
   parameters.

2. Looks up the effective declaration set for that atom kind (the
   `StateMapDeclarations` on the relevant definition, or
   `SessionStateMapDeclarations` for `Session`).

3. Checks whether the field-name string literal argument appears in the
   declaration set with the correct `FieldType` for the operation:
   - `modify-accumulator`, `clear-accumulator`, `apply-modifier` → `Number`
   - `apply-condition`, `remove-condition` → `Bool`
   - `get-state` → either `Number` or `Bool` (any declared field is valid)

4. If the field name is not declared, throws `DefinitionException` with a message
   identifying the keyword invocation and the undeclared field name.

**What is not validated:**

- Field name arguments that are not string literals (e.g. `Kw.Param` references)
  cannot be checked statically and are silently skipped.
- Engine-reserved session fields (`turn-number`, `phase-index`) are always
  treated as implicitly declared for `get-state` when the target type is `Session`
  — the validator does not require them to appear in `SessionStateMapDeclarations`.
- `InitManifest` accumulator/condition keys are not validated against declarations
  in this change. That is deferred.

**Validation is build-time only** (consistent with §2.6: "authoring-time error").
No runtime checks are added.

---

### R3 — `GameDefinitionBuilder` API: `stateMapDeclarations` Parameters

The builder API is extended to make declaring state map fields ergonomic:

- `AddZone(name, staticProperties)` gains an overload accepting
  `IReadOnlyList<StateFieldDecl>? stateMapDeclarations = null`.
- `AddPlayer(name, staticProperties)` gains the same overload.
- `AddCard(CardDefinition card)` is unchanged (callers construct `CardDefinition`
  directly and add `StateMapDeclarations` to the record constructor). No new
  overload is needed.
- `WithSessionStateMap(IReadOnlyList<StateFieldDecl> declarations)` is added to
  set `GameDefinition.SessionStateMapDeclarations`.

Existing callers that omit `stateMapDeclarations` continue to work — the
parameter defaults to null (empty list semantics).

---

### R4 — Atom View Generation (D40): Typed GDScript Atom Classes

`GodotEmitter` gains a new method `EmitAtomViews(GameDefinition, string)` that
generates four typed GDScript `RefCounted` classes, one per atom kind, using
`StateMapDeclarations` as the source of truth for typed property getters.

**Generated files:**

| File | Class | Source of declarations |
|---|---|---|
| `card_atom.gd` | `CardAtom` | Union of `CardDefinition.StateMapDeclarations` across all card definitions |
| `zone_atom.gd` | `ZoneAtom` | Union of `ZoneDefinition.StateMapDeclarations` across all zone definitions |
| `player_atom.gd` | `PlayerAtom` | Union of `PlayerDefinition.StateMapDeclarations` across all player definitions |
| `session_atom.gd` | `SessionAtom` | `GameDefinition.SessionStateMapDeclarations` |

**Declaration union rule:** When multiple card (or zone, or player) definitions
declare the same field name with the same `FieldType`, the field appears once in
the generated class. When the same field name is declared with conflicting
`FieldType` values across definitions of the same kind, `Build()` raises a
`DefinitionException` before emission is reached.

**Per-class structure:**

Each generated class:
- Extends `RefCounted`.
- Holds a private `_atom_id: int` set at construction.
- Has a static factory method `_create(atom_id: int) -> <ClassName>`.
- For each `Number`-typed declared field: a getter method
  `get_<snake_name>() -> float` that calls
  `ArchetypeInterop.get_accumulator(_atom_id, "<field-name>")`.
- For each `Bool`-typed declared field: a predicate method
  `has_<snake_name>() -> bool` that calls
  `ArchetypeInterop.has_condition(_atom_id, "<field-name>")`.
- **Structural fields** always present (not from state map):
  - `CardAtom`: `get_atom_id() -> int`, `get_zone_id() -> int`, `get_owner_id() -> int`
  - `ZoneAtom`: `get_atom_id() -> int`, `get_owner_id() -> int`
  - `PlayerAtom`: `get_atom_id() -> int`, `get_owner_id() -> int`
  - `SessionAtom`: `get_atom_id() -> int`

The structural getters (`get_zone_id`, `get_owner_id`) call the existing
`ArchetypeInterop.get_zone(atom_id)` and `ArchetypeInterop.get_owner(atom_id)`
methods respectively.

**`ArchetypeInterop` factory methods:**

`archetype_interop.gd` gains four factory methods appended to the existing file:
- `get_card(atom_id: int) -> CardAtom`
- `get_zone(atom_id: int) -> ZoneAtom`
- `get_player(atom_id: int) -> PlayerAtom`
- `get_session() -> SessionAtom` (singleton; caches on first call using a private
  `_session_atom` field initialized to `null`)

These methods are additive. The existing untyped `get_accumulator`,
`has_condition`, `get_zone`, and `get_owner` methods are unchanged.

**`SessionAtom` singleton invariant:**

`get_session()` returns a single cached `SessionAtom` instance. The session atom
has exactly one engine atom ID for its lifetime; the cache is valid for the
entire game session. The ID is obtained via `GetAtoms(AtomKind.SESSION)` on
first call.

**`BuildRunner.Run` integration:**

`BuildRunner.Run` calls `GodotEmitter.EmitAtomViews(fullDefinition, outputDir)`
as part of the standard artifact emission step, alongside the existing emitter
calls. `EmitAtomViews` writes all four `*_atom.gd` files and appends the four
factory methods to the `archetype_interop.gd` content.

---

## Out of Scope

- **Shared schemas (§2.7)**: universal schemas and per-kind schema declarations
  at the game-definition level are not addressed. The implementation uses
  per-definition declarations only. Schema support is deferred.
- **`InitManifest` key validation**: validating that accumulator/condition keys in
  `ZoneSpec`, `CardSpec`, and `PlayerStateSpec` match the declared state map for
  the target definition is explicitly deferred.
- **Pure property keywords**: computed-value keywords (`get-property`, `owner-of`,
  `in-zone`) are not affected.
- **Runtime validation**: no engine execution-path changes. Build-time only.
- **Modifier-adjusted property cross-check**: the cross-check between a static
  property and its matching state field declaration (per §2.6 invariant) is
  deferred to a later change.

---

## Constraints

- **Target branch**: `impl/d38-d39-state-query-atomkind`. All work merges here.
- **No runtime overhead**: `StateMapDeclarations` on definition records carries
  no cost after build. Engine execution paths are unchanged.
- **Backward compatibility**: adding `StateMapDeclarations` to existing records
  must not break existing callers. The field must default to an empty list.
- **`SessionAtom` is a singleton**: `get_session()` returns one view object.
  `GetAtoms(AtomKind.Session)` returns exactly one ID.
