## Review: state-map-declarations — branch `impl/d38-d39-state-query-atomkind`

Review date: 2026-03-24
Scope: `StateFieldDecl.cs`, `GameDefinition.cs`, `GameDefinitionBuilder.cs`, `StateMapValidator.cs`, `GodotEmitter.cs` (EmitAtomViews), `BuildRunner.cs`, and all new tests.
Architecture decisions checked: D1, D3, D40 (ref only — see Defect 1), domain model §2.6.

---

### Defects

All defects resolved.

- [RESOLVED — BLOCKER] D40 is now recorded in `docs/architecture.md` with a complete `### D40` section and a checked `[x] D40` entry in the Open Items checklist. Verified 2026-03-24.

- [RESOLVED — MINOR] Zone atom doc-comment factory method name corrected in `GodotEmitter.EmitAtomViewFile`. Fixed directly by reviewer.

- [RESOLVED — MINOR] `clear-accumulator` dead entry removed from `StateMapValidator.FieldKeywords`; test updated to reflect actual behavior. Fixed directly by reviewer.

---

### Observations

- `StateFieldDecl` and `StateFieldType` exactly match §2.6 and R1 in every detail: field names, default-null `TextTemplate`, equality semantics, and the `sealed record` representation.
- All four definition types carry `StateMapDeclarations`/`SessionStateMapDeclarations` with correct null-as-empty semantics and backward-compatible defaulting. No existing callers are broken.
- `GameDefinitionBuilder` overload structure is correct: the no-declarations overload delegates to the declarations overload passing `null`; `WithSessionStateMap` is fluent and stored.
- `StateMapValidator` opt-in semantics are correctly implemented: a kind is unconstrained when no definition of that kind supplies non-null `StateMapDeclarations`. Engine-reserved session fields (`turn-number`, `phase-index`) are implicitly valid and correctly handled via an early-return guard before the declaration lookup.
- `UnionKindDeclarations` in both `StateMapValidator` and `GodotEmitter` correctly deduplicates matching fields (same name + same type = keep one) and raises on conflicting types.
- `EmitAtomViews` structurally correct: `RefCounted`, `_atom_id`, `_create` factory, typed getters for `Number` fields (`get_<snake>() -> float`) and predicates for `Bool` fields (`has_<snake>() -> bool`), structural getters per kind spec.
- `SessionAtom` singleton pattern is correctly implemented: private `_session_atom` field, null guard, `get_atoms(ArchetypeAtomKinds.SESSION)`, cache on first call.
- `ToSnakeCase` handles kebab-case correctly (`-` → `_`). `camelCase` names are not addressed but are not required by spec.
- `BuildRunner.Run` calls `GodotEmitter.EmitAtomViews(fullDefinition, archetypeDir)` after `EmitInteropScripts`, ensuring the base interop file exists before the append step.
- Test coverage is thorough: all 15 required validator tests from tasks.md are present; all 10 required emitter tests are present. Tests are not trivially true — they construct real builders and check generated content.
- The `Validation_NonLiteralFieldArg_Skipped` and `Validation_GenericAtomParam_Skipped` tests correctly exercise the two skip conditions required by R2.
- 262/262 tests pass.

---

### Verdict

PASS WITH MINOR FIXES

All defects resolved. D40 added to `docs/architecture.md` (BLOCKER resolved). Two MINORs fixed directly by reviewer. 262/262 tests passing.

---

## Review: KeywordNodeConverter — DeserializeLiteral bug fix (commit c560f27)

Review date: 2026-03-22
Scope: `src/Archetype.Core/GameDefinitionJsonOptions.cs` (one-line fix), `tests/Archetype.Tests/Serialization/KeywordNodeSerializationTests.cs` (new file, 8 tests).
Architecture decisions checked: D1, D2.

---

### Question answers

**1. Is the fix architecturally sound?**

Yes. `LiteralConverter.Read` (`Keywords.cs:65–66`) opens with an explicit guard:

```csharp
if (reader.TokenType != JsonTokenType.StartObject)
    throw new JsonException("Expected StartObject for Literal.");
```

A `Utf8JsonReader` constructed over a byte array always starts at `TokenType.None`. The single `reader.Read()` call added on line 190 advances to `StartObject` before control is passed to `LiteralConverter.Read`. The bytes written by `DeserializeLiteral` are a complete, valid JSON object, so `StartObject` is always the first token — the precondition is guaranteed structurally.

The fix is the minimum correct change. It does not add allocation or change the public contract of any type. D2 (keyword representation as serialisable expression trees) is fully satisfied.

**2. Are there other callers of DeserializeLiteral or similar patterns?**

`new Utf8JsonReader(...)` appears exactly once in `src/` — the fixed site. No other code in the repository constructs a reader over manually assembled bytes and delegates to a converter. The `DeserializeInvocation` path takes a different approach (it calls back through `JsonSerializer.Deserialize<KeywordNode>`, which creates its own reader internally) and is unaffected. There is no analogous risk elsewhere.

**3. Do the tests adequately cover the regression?**

Mostly yes, with one gap (see MINOR below). Coverage delivered:

- `Literal` round-trip for all three non-AtomId value types (`string`, `double`, `bool`) via both builder-serialize→deserialize and raw JSON paths.
- `Invocation` with a `Literal` arg — the exact regression shape.
- `Invocation` with mixed arg types (`ParameterRef` + multiple `Literal`s).

Missing: `AtomId` literal round-trip. `LiteralConverter` explicitly handles `"atom"` tag (`Keywords.cs:98`), and `Literal(AtomId)` is a legitimate value the domain model allows. It is not covered by any test in the new file or in pre-existing tests (confirmed by grep — no test file constructs `new Literal(new AtomId(...))`).

Missing: deeply nested `Invocation` inside `Invocation` args (e.g. `attack(take-damage(target, 5), bonus)`). The existing `Invocation_WithMixedArgs_RoundTrips` test nests one level but does not verify recursive `Invocation` args. This is lower priority because the recursive path is exercised indirectly by the existing `Invocation` tests, but an explicit two-level nesting test would give complete confidence.

Missing: error path — no test verifies that an unknown discriminator (`$type: "bad"`) throws a `JsonException`.

**4. Does docs/architecture.md need updating?**

No. The bug was an implementation defect in the serialization plumbing beneath D2, not a new architectural decision or a change to any stated constraint. D2 already specifies that `KeywordNode` trees must round-trip through JSON. The fix correctly implements that existing constraint; no amendment is warranted.

---

### Defects

- [MINOR] `AtomId` literal round-trip is not tested. `LiteralConverter` handles the `"atom"` discriminator tag, but no test in `KeywordNodeSerializationTests.cs` or anywhere else exercises `new Literal(new AtomId(...))` through the `KeywordNodeConverter` path. — `tests/Archetype.Tests/Serialization/KeywordNodeSerializationTests.cs` — gap in coverage for a supported literal type.

### Minor fixes applied directly

None — the MINOR above requires a new test and is left for the implementer (test additions are not MINOR text fixes).

### Observations

- The comment on line 190 (`// advance to StartObject before delegating`) is accurate and sufficient. No change needed.
- `DeserializeInvocation` and `DeserializeLiteral` are the only two non-trivial deserialization paths. Their structural asymmetry (one uses `JsonSerializer.Deserialize<>`, the other manually constructs a reader) is intentional and documented in the class-level summary. The asymmetry is correct but worth a brief inline comment on `DeserializeInvocation` noting why it does not need the same advance step — future readers will wonder.
- The test class XML summary at line 9–12 accurately describes the regression; no changes needed.

---

### Verdict

PASS WITH MINOR FIXES

The one-line fix is correct and the regression is covered. The `AtomId` literal round-trip gap is a MINOR that should be addressed but does not block this change. No architecture doc update is required.
