# Review: Text Renderer — `Archetype.Text` + `Archetype.Core/RenderNode.cs` (impl/text-renderer)

Reviewed 2026-03-05. Implementation commit: `47b10e8 Implement Tier 4 text renderer: RenderNode tree + TextRenderer (70/70 tests)`.

Changeset: `src/Archetype.Core/RenderNode.cs` (new), `src/Archetype.Text/TextRenderer.cs` (new), `src/Archetype.Core/BuiltInKeywords.cs` (TextTemplate values added), `tests/Archetype.Tests/TextRenderer/TextRendererTests.cs` (new), `docs/implementation-status.md` (updated).

---

## Defects

### [BLOCKER 1] `RenderBlock` returns the step node directly for single-step blocks — deviates from D11 API contract — `TextRenderer.cs:169` — violates D11

D11 states explicitly:

> `RenderBlock` produces a `SequenceNode` of one `RenderNode` per step in the block.

The implementation special-cases the single-step scenario:

```csharp
return items.Count == 1 ? items[0] : new SequenceNode(items);
```

For a one-step block this returns the inner `RenderNode` directly (e.g., a `CompositeNode`) rather than a `SequenceNode` wrapping it. Test T7 `RenderBlock_SingleStep_ReturnsSingleNodeDirectly` explicitly asserts `Assert.IsType<CompositeNode>(result)`, confirming the deviation.

This is an API contract violation. A host that calls `RenderBlock` and pattern-matches or casts to `SequenceNode` will fail on any single-step card ability. The D11 architecture's stated purpose for `RenderBlock` returning `SequenceNode` is that the host can iterate steps uniformly — the single-step unwrapping breaks that invariant.

**Fix (choose one):**
- Remove the special case: always return `new SequenceNode(items)` regardless of count (one-line change in `TextRenderer.cs`; update test T7 to assert `SequenceNode` with one item).
- Update `docs/architecture.md` D11 to ratify the unwrapping optimization and document the resulting invariant the host must handle.

Either path is acceptable; spec and implementation must agree before the module is marked complete.

---

## Minor Fixes Applied Directly

### [MINOR 1] `RegexOptions.Compiled` is WASM-unsafe — `TextRenderer.cs` — D1

`TemplateTokenRegex` was declared with `RegexOptions.Compiled`, which uses `Reflection.Emit` internally. D1 states: "Minimize reflection. Trim-unfriendly code inflates binary size and may fail at runtime under the WASM IL stripper." `Reflection.Emit` is not available in Godot's WebAssembly export context; while .NET 10 may degrade gracefully, the flag adds binary-size risk and is unnecessary for a template parser that is never on the hot execution path.

**Fixed in place:** `RegexOptions.Compiled` removed; `RegexOptions.CultureInvariant` retained. A comment explaining the rationale has been added.

---

## Observations

- **`RenderStaticEffect` and permanent lifetime**: D11 says the method produces a `SequenceNode` containing "the rendered lifetime spec" (no "(if any)" qualifier), but the implementation skips adding the node when it is the empty `TextSpan("")` returned by a permanent `LifetimeSpec`. In practice this is harmless — an empty span in the sequence adds no information — but it technically deviates from the stated schema. If the architect agrees the omission is correct, D11 should be updated to add "(if non-permanent)" to the lifetime entry.

- **Invocation-time composite body propagation edge case**: When `Render` is called with `bindings` on an `Invocation` whose keyword is composite, the method passes the *outer* bindings directly into the body render (`Render(def.Body, locale, bindings)`). This works correctly when all of the composite's parameter names appear as keys in `bindings` (the expected pattern for event-log rendering from an `ExecutionContext`). It silently degrades to label rendering if an argument was passed as a `Literal` at the call site and its name therefore isn't in `bindings`. Not a defect in the current usage model, but worth a comment on `RenderInvocation` so the limitation is visible to maintainers.

- **Test gaps**: Three scenarios are absent from `TextRendererTests.cs` that could harden the `RenderStaticEffect` contract:
  - A `StaticEffectDef` with a `StateContributionBlock` present.
  - A `StaticEffectDef` with a non-permanent `LifetimeSpec` (the conditional lifetime-inclusion path inside `RenderStaticEffect` is untested).
  - `Resolve` called with a non-null locale dictionary.
  None represent gaps in core invariants — the underlying primitives are tested — but they would close the `RenderStaticEffect` contract fully.

- **Overall quality**: `RenderNode` discriminated union, `TextRenderer` architecture, locale support, `ConditionalWeakTable` definition-time caching, D18 `RulesRef` tag parsing (both long and short form), and `Resolve` are all correctly implemented and well-documented against D11/D18. TextTemplate values are present on all 34 built-in definitions. XML summaries are complete on every public type and member. The dual-use invariant test T11 is the most architecturally significant test and passes correctly.

---

## Verdict

**NEEDS REWORK**

One blocker must be resolved: `RenderBlock`'s single-step behavior deviates from D11's stated API contract. The fix is a one-line change to `TextRenderer.cs` (and the corresponding test assertion in T7), or a co-ordinated update to `docs/architecture.md` D11.

The MINOR (`RegexOptions.Compiled`) has been fixed directly in this review.
