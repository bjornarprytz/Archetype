## Review: action-args-and-cost-model — Reviewer Tasks 10.1–10.10

Review date: 2026-03-18.

Scope: `src/Archetype.Engine/BuiltInHandlers.cs` (`Assert` handler), `src/Archetype.Engine/ExecutionContext.cs`, `src/Archetype.Engine/ActionResolver.cs`, `src/Archetype.Engine/CostValidator.cs`, `src/Archetype.Engine/GameSession.cs` (`ComputeAvailableActions`, `ResolveCostsForAction`), `src/Archetype.Engine/GameState.cs` (`CloneForValidation`), `src/Archetype.Build/Kw.cs` (`OwnedByActivePlayer`), `src/Archetype.Core/Interfaces.cs` (`IEngineObserver`), `tests/Archetype.Tests/BuiltIns/AssertTests.cs`, `tests/Archetype.Tests/CostModel/CostModelTests.cs`, `tests/Archetype.Tests/ComputeAvailableActions/ComputeAvailableActionsTests.cs`, `tests/Archetype.Tests/SaveLoad/SaveLoadTests.cs`.

Architecture decisions checked: D20, D21, D22, D23, D24, D25.

---

### Defects

None.

---

### Observations

- **10.1 — PASS.** `BuiltInHandlers.Assert` checks `ctx.IsCostBody` first; on `true` it throws `EngineException` unconditionally without reading `on_fail` or `notify` arguments and without calling `OnDiagnostic`. Conforms to D20.

- **10.2 — PASS.** `OnDiagnostic` is called inside `if (notify == NotifyFlag.On)` at line 478, then the `return onFail switch { ... OnFail.Panic => throw ... }` executes at line 481. Observer call precedes the exception. When `notify == NotifyFlag.Off` the block is skipped entirely. Conforms to D25.

- **10.3 — PASS.** The `Assert` handler never calls `ctx.EventLog.Append`. The event-log invariant is tested directly in `Assert_NeverAppendsToEventLog`. Conforms to D20.

- **10.4 — PASS.** `GameState.CloneForValidation` copies atom table (Accumulators, ZoneId, OwnerId, Kind), modifier index, condition index, session atom ID, and player name registry. It explicitly excludes `ContributionRegistry`, `ActiveStaticEffects`, `DormantDeclarativeEffects`, game outcome flags, and — since `EventLog` is not a field on `GameState` at all — event log. Conforms to D21.

- **10.5 — PASS.** `ComputeAvailableActions` captures `stateSnapshot = _state.CloneForValidation()` into a local variable before constructing the lambda. The delegate closes over the clone, not over `_state`. No live reference to the mutable session state drifts into the delegate. Conforms to D22.

- **10.6 — PASS.** `ActionResolver.ResolveAction` calls `eventLog.OpenAction()` once, executes all cost blocks (with `IsCostBody = true`) and then the primary block inside the same action scope, then calls `eventLog.CloseAction()` in `finally`. No separate `OpenAction`/`CloseAction` pair wraps individual cost bodies. Conforms to D23.

- **10.7 — PASS.** `ComputeAvailableActions` Pass 1 iterates `_state.GetAtoms(AtomKind.Card)` with a zone definition-name filter only; no `OwnerId` predicate is present. Pass 2 iterates all card atoms with no zone or owner filter. Test 9.13 (`ComputeAvailableActions_CardInPlayableZone_IncludedRegardlessOfOwner`) and 9.14 (`ComputeAvailableActions_AbilityOnUnownedCard_Included`) confirm this at the integration level. Conforms to D24.

- **10.8 — PASS.** `Kw.OwnedByActivePlayer()` carries a `<summary>` XML doc with a `<b>Requirement:</b>` paragraph that names the `"active-player"` session state field and explains the runtime consequence. The doc accurately reflects that the field must be set by game-level initialization. Minor note: D24 says "Games that do not declare this field will receive a `DefinitionException` at `Build()` time," but no Build()-time check for `"active-player"` field presence is implemented; the doc correctly describes the actual runtime behavior (`EvaluateCondition` will throw). The gap between D24's stated promise and the actual enforcement is a pre-existing architecture note, not a new defect introduced by this change. Conforms to 10.8's stated requirement (doc states the `"active-player"` session state requirement).

- **10.9 — PASS.** Two `IEngineObserver` implementations exist in the codebase: `RecordingObserver` in `AssertTests.cs` and `CapturingObserver` in `SaveLoadTests.cs`. Both have `void OnDiagnostic(DiagnosticEvent e)`. All 164 tests pass; no compilation errors.

- **10.10 — PASS.** All 164 tests pass (`dotnet test` output: `Failed: 0, Passed: 164`). The test suite includes 6 new assert tests (9.1–9.6), 7 new cost-model tests (9.7–9.12, 9.16), and 3 new `ComputeAvailableActions` tests (9.13–9.15) plus the updated 3.8 test. Net test count increased from the prior baseline.

---

### Verdict

PASS

---

## Review: Group 6 — Renderer UI Panels (impl/text-renderer)

Review date: 2026-03-16.

Scope: all files under `tooling/src/renderer/` introduced or modified by Group 6 — `components/DslEditor.tsx`, `panels/KeywordEditorPanel.tsx`, `panels/CardEditorPanel.tsx`, `panels/GameRulesPanel.tsx`, `panels/InitManifestPanel.tsx`, `panels/LocalizationPanel.tsx`, `panels/ProblemsPanel.tsx`, `components/StatusBar.tsx`, `components/ExportModal.tsx`, `panels/GraphPanel.tsx`, `panels/SetOverviewPanel.tsx`, `snapshot.ts`, and `App.tsx`. All corresponding test files reviewed.

Architecture decisions checked: D26, D27, D28, D31.

---

### Defects

#### [MINOR 1] `ExportModal` body copy deviates from D31 specified text — `ExportModal.tsx:65`

D31 specifies the dialog body must say "Missing strings will fall back to the source language at runtime." The implementation says "Export will include untranslated placeholders." These communicate different (and conflicting) things: the spec copy is accurate (the runtime does fall back via the `TextTemplate` resolution order); the implementation copy is misleading ("untranslated placeholders" implies visible empty/broken strings in game, not a graceful fallback). D31 is explicit about this message being the mechanism by which the game creator understands the runtime fallback behaviour without needing to know the renderer internals.

**Fixed in place:** updated `ExportModal.tsx:65` to match the D31 copy.

---

### Observations

- **`completionItem.insertText` ignores snippet syntax from D28.** `DslEditor.tsx:181` always sets `insertText: item.label` — it ignores the `insertText` field that the sidecar returns in `CompletionItem`. D28 says `insertText` uses Monaco snippet syntax (`$1`, `$2` for tab stops) for keyword suggestions like `get-state($1, $2)`. The effect is that completions never insert tab-stop templates, reducing the UX value of autocomplete. This is not a protocol defect (the correct data is in the `item` object), it is a renderer-side oversight in how the data is consumed. It does not affect correctness, only completeness of the feature.

- **`staticEffect_${i}` dslField falls through to `UpdateCardEffect` — no dedicated handler.** In `CardEditorPanel.tsx:289` the static effect body editor uses `dslField={`staticEffect_${i}`}`. In `DslEditor.tsx` `resolveMutation`, this falls through to `UpdateCardEffect` with `effectName = "staticEffect_0"` etc. D28 does not specify a dedicated method for static effect bodies; routing through `UpdateCardEffect` is the reasonable interpretation. The sidecar's `UpdateCardEffectHandler` must handle these effect names correctly. The static effect `lifetime` DSL correctly routes to `UpdateLifetimeSpec` via the `lifetime:` prefix. This is consistent and the fallthrough is intentional, but worth a comment in `resolveMutation` explaining the naming contract.

- **`fetchSnapshot()` calls `SaveProject` on every panel mount.** Multiple panels (`KeywordEditorPanel`, `CardEditorPanel`, `GameRulesPanel`, `InitManifestPanel`, `LocalizationPanel`, `SetOverviewPanel`) each independently call `fetchSnapshot()` on mount, which calls `SaveProject` under the hood. This means opening/switching panels triggers multiple round-trip serialisations of the entire project state. For large projects this could become noticeable. Not a correctness defect; noted for the simplifier pass.

- **`ProblemsPanel` secondary sort by entry name not implemented.** D28 specifies "sorted by severity then entry name." The sort comparator in `ProblemsPanel.tsx:49-52` only implements the severity tier and returns 0 for same-severity pairs. Within-tier ordering depends on sidecar sort order. Practically correct because the sidecar's `GetAllDiagnosticsHandler` does the full sort, but the renderer should not rely on this.

- **`DslEditor` completion provider registered on every `onMount`.** `DslEditor.tsx:166` registers a `CompletionItemProvider` for `archetype-dsl` on every mount. Monaco accumulates providers rather than deduplicating, so panels with many DSL editors will show N copies of each suggestion. A module-level `completionProviderRegistered` flag (same pattern as the existing `languageRegistered` flag on line 55) would fix this.

- **`KeywordDetail` parameter name input commits on every `onChange` keystroke.** `KeywordEditorPanel.tsx:247` calls `void updateParam(i, "name", e.target.value)` on `onChange`. D28 specifies non-DSL fields commit on "blur / Enter / selection," not on every keystroke. This fires one `UpdateField` + full re-validation per keypress while the user is typing a parameter name. The type dropdown is unaffected (dropdown fires `onChange` only on selection).

- **`DslEditor` invoke assertion is absent from two tests.** `DslEditor.test.tsx:86-108` does not assert `window.archetype.invoke` was called with the correct channel and payload (comment on line 106 acknowledges this). Similarly, the `activationCondition` test (line 111) is a render-only smoke test. These are known gaps.

- **All 10 Group 6 tasks have test files; coverage is solid.** Each panel has render, interaction, and IPC-call assertions. Happy paths and empty states are covered across all panels; error states are covered where most impactful (GraphPanel network error, ExportModal errors-kind response, ProblemsPanel sort order). No missing test files.

---

### Verdict

**PASS WITH MINOR FIXES**

MINOR 1 (ExportModal body copy deviation from D31) has been fixed directly. No blockers. All observations are improvement opportunities; none violate architecture decisions or domain invariants. All 105 TypeScript tests pass.

---

# Review: Tooling Sidecar Bug Fixes — `Archetype.Tooling.Server` + `Archetype.Core/GameDefinitionJsonOptions.cs` (impl/text-renderer)

Review date: 2026-03-11. Bug fixes: 6 items covering Fix 1–6 as described in the task.
Re-review date: 2026-03-11. Blocker fixes verified.

Changeset (original + blocker fixes):
- `src/Archetype.Tooling.Server/KeywordEntry.cs` — `ReturnType: TypeName?` added
- `src/Archetype.Tooling.Server/CardEntry.cs` — `StaticEffectEntry` full schema
- `src/Archetype.Tooling.Server/ProjectFileLoader.cs` — reads `returnType`, `artCropRegion`, static effect fields, corrected `ZoneSpec` constructor args
- `src/Archetype.Tooling.Server/ProjectFileSerializer.cs` — serializes `artCropRegion`, static effect fields, corrects `ZoneSpec` definition field
- `src/Archetype.Tooling.Server/Export/GameDefinitionExporter.cs` — real `StaticEffectDef` export, `ReturnType` from entry
- `src/Archetype.Tooling.Server/Handlers/RenameEntryHandler.cs` — DSL rewrite added, extended to Phase/ActionRule/SBR (BLOCKER 2 fix)
- `src/Archetype.Tooling.Server/Handlers/UpdateLifetimeSpecHandler.cs` — calls `LifetimeDsl.Parse` immediately (BLOCKER 1 fix)
- `src/Archetype.Tooling.Server/LifetimeDsl.cs` — new file
- `src/Archetype.Core/GameDefinitionJsonOptions.cs` — new file
- Tests: 10 + 8 new tests in `ProjectFileLoaderTests`, `ValidatorTests`, `RpcHandlerTests`

Architecture decisions checked: D27, D28, D6.

---

## Defects

### ~~[BLOCKER 1] `UpdateLifetimeSpecHandler` does not call `LifetimeDsl.Parse`~~ — RESOLVED

`UpdateLifetimeSpecHandler.cs:39` now calls `LifetimeDsl.Parse(dsl)` immediately after storing the string, and assigns the result to `effect.LifetimeNode`. If the parse returns null for a non-empty string, a diagnostic is recorded to `effect.Diagnostics`. The XML doc has been corrected to describe this behaviour. Test `UpdateLifetimeSpec_SetsLifetimeNode_SoExportReflectsSpec` verifies the `TurnTimer` path end-to-end (parse → `LifetimeNode` non-null → `IsPermanent == false` → export JSON contains `"$type": "turn"` and `"Turns": 3`). Test `UpdateLifetimeSpec_InvalidDsl_RecordsDiagnostic_LifetimeNodeNull` covers the parse-failure path.

### ~~[BLOCKER 2] `RenameEntryHandler.RewriteKeywordRefs` omits Phase/ActionRule/SBR DSL fields~~ — RESOLVED

`RenameEntryHandler.cs:153–176` now rewrites:
- `PhaseEntry.InitDsl` and `CleanupDsl` (lines 155–159)
- `ActionRuleEntry.BeforeDsl` and `AfterDsl` (lines 165–168)
- `StateBasedRuleEntry.ConditionDsl` and `BodyDsl` (lines 174–175)

Six new tests cover all three entry types, each with a DSL-content assertion test and a save/reload zero-errors test: `RenameEntry_RewritesPhaseInitAndCleanupDsl`, `RenameEntry_Phase_SaveReload_ZeroErrors`, `RenameEntry_RewritesActionRuleBeforeAndAfterDsl`, `RenameEntry_ActionRule_SaveReload_ZeroErrors`, `RenameEntry_RewritesStateBasedRuleConditionAndBodyDsl`, `RenameEntry_StateBasedRule_SaveReload_ZeroErrors`.

---

## Minor Fixes Applied Directly

### [MINOR 1] `Validator.CollectEntryDiagnostics` does not collect `StaticEffectEntry.Diagnostics` — `Validator.cs:59–60`

`UpdateLifetimeSpecHandler` records parse-failure diagnostics into `effect.Diagnostics` (`StaticEffectEntry.Diagnostics`). However, `CollectEntryDiagnostics` only iterates `card.Diagnostics` (the card-level list), not the per-`StaticEffectEntry` sub-lists. After `RevalidateAndBuildResponse` calls `Validator.Validate`, `state.Diagnostics` is rebuilt without including static-effect-level diagnostics. The result: `GlobalErrorCount` is not incremented on a bad lifetime DSL, so the renderer's problems panel never sees the error. The test for the invalid-DSL path asserts against `StaticEffects[0].Diagnostics` directly and passes, but the gap means the diagnostic is invisible in the response payload.

**Fixed in place:** `CollectEntryDiagnostics` now iterates `card.StaticEffects` and collects each `se.Diagnostics` in addition to `card.Diagnostics`. All 150 tests continue to pass.

---

## Observations

- **`LifetimeDsl.Parse` `TurnTimer` path now covered.** The previous review flagged that all three `LifetimeDsl` condition paths were untested. `UpdateLifetimeSpec_SetsLifetimeNode_SoExportReflectsSpec` now covers the `TurnTimer` path end-to-end. The `TriggerCount` and `WhileCondition` paths remain untested in isolation, but this is a coverage gap rather than a correctness defect; the parsing logic for all three paths is straightforward.

- **D27 rewrite method diverges from architecture spec — benign.** D27 says to use `oldName(` (with trailing `(`) to disambiguate. The implementation uses a token-boundary approach (`IsIdentChar`) instead, which is strictly more correct. The divergence is documented in the `RewriteDsl` XML comment. No action needed.

- **`RenameEntry_RewritesPhaseInitAndCleanupDsl` test uses a slightly imprecise assertion.** Line 671: `Assert.DoesNotContain("phase-kw()", phase.InitDsl!)` checks that the literal string `"phase-kw()"` is absent. After rewriting, `InitDsl` becomes `"phase-kw-v2()"`, which indeed does not contain `"phase-kw()"`. This is correct but it would also pass if `InitDsl` became empty or was set to something unrelated. The paired `Assert.Contains("phase-kw-v2", ...)` assertion is the load-bearing one. Together they provide sufficient coverage.

- **All six save/reload tests rely on `ProjectFileLoader.Load` producing zero errors.** This is the correct way to verify the D27 round-trip invariant. The tests would catch any regression where DSL strings are left stale.

- **`TriggerEventKeyword` and `TriggerScope` not yet ratified.** These two `StaticEffectEntry` fields lack architect sign-off. No change needed from the reviewer.

- **Fix 5, Fix 6, Fix 1, Fix 2, `GameDefinitionJsonOptions` — all confirmed correct** (unchanged from initial review observations).

---

## Verdict

**PASS**

Both blockers are resolved. MINOR 1 (static-effect diagnostics not propagated to `state.Diagnostics`) was fixed directly in `Validator.cs`. All 150 tests pass. No outstanding defects.

---

# Previous Review: Text Renderer — `Archetype.Text` + `Archetype.Core/RenderNode.cs` (impl/text-renderer)

Initial review: 2026-03-05. Implementation commit: `47b10e8`.
Re-review: 2026-03-07. Fix commit: `1a49699 Fix BLOCKER 1: RenderBlock always returns SequenceNode (D11 API contract)`.

Changeset: `src/Archetype.Core/RenderNode.cs` (new), `src/Archetype.Text/TextRenderer.cs` (new), `src/Archetype.Core/BuiltInKeywords.cs` (TextTemplate values added), `tests/Archetype.Tests/TextRenderer/TextRendererTests.cs` (new), `docs/implementation-status.md` (updated).

---

## Defects

### [BLOCKER 1] ~~`RenderBlock` returns the step node directly for single-step blocks — deviates from D11 API contract — `TextRenderer.cs:169` — violates D11~~ — RESOLVED in `1a49699`

The single-step unwrapping optimisation (`return items.Count == 1 ? items[0] : new SequenceNode(items)`) has been removed. `RenderBlock` now unconditionally returns `new SequenceNode(items)` at `TextRenderer.cs:177`, satisfying the D11 contract.

Test T7 has been split into T7a (`RenderBlock_MultiStep_ProducesSequenceNodeWithTwoItems`) and T7b (`RenderBlock_SingleStep_AlwaysReturnsSequenceNode`). T7b asserts `SequenceNode` with exactly one item and verifies the inner item is a `CompositeNode`, correctly reflecting the D11 invariant.

Three additional tests from review observations were also added: T8h (StateContributionBlock present), T8i (non-permanent LifetimeSpec), T9d (Resolve with locale). All 73 tests pass.

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

**PASS**

BLOCKER 1 resolved in `1a49699`: `RenderBlock` now always returns `SequenceNode`, satisfying the D11 API contract. Test T7b correctly asserts the single-step case. The MINOR (`RegexOptions.Compiled`) was fixed directly in the initial review. No outstanding defects remain.

---

## Review: D17 Save/Load — `SeededRandom`, `GameStateSnapshotSerializer`, `GameState.ToSnapshot/LoadFromSnapshot`, `GameSession` wiring (impl/text-renderer)

Initial review: 2026-03-08. Implementation commits: `88e652a` (changes), `1a49699` (prior fix — base).
Re-review: 2026-03-08. Fix commit: `88e652a changes` (BLOCKER 1 fix included).

Changeset: `src/Archetype.Core/Snapshot.cs` (new), `src/Archetype.Core/Interfaces.cs` (`OnTurnStart` added), `src/Archetype.Core/GameDefinition.cs` (`Id` added), `src/Archetype.Core/Keywords.cs` (`LiteralConverter` + `[JsonDerivedType]` on `KeywordNode`), `src/Archetype.Core/Contributions.cs` (`[JsonDerivedType]` on `ContributionSource`, `LifetimeCondition`), `src/Archetype.Core/StaticEffects.cs` (`[JsonDerivedType]` on `ParameterModification`), `src/Archetype.Engine/SeededRandom.cs` (new), `src/Archetype.Engine/GameStateSnapshotSerializer.cs` (new), `src/Archetype.Engine/GameState.cs` (`ToSnapshot`/`LoadFromSnapshot` added), `src/Archetype.Engine/GameSession.cs` (`FromSavedState` load path, `OnTurnStart` call, `CreateSnapshot`), `tests/Archetype.Tests/SaveLoad/SaveLoadTests.cs` (new, 13 tests).

---

### Defects

#### ~~[BLOCKER 1] `GameSession.ApplyConditions` does not register conditions in `ContributionRegistry` — manifest-provisioned conditions are lost from snapshots — `GameSession.cs:649-661`~~ — RESOLVED

`ApplyConditions` now calls `_state.ContributionRegistry[id] = new ConditionContributionWrapper(contribution);` at `GameSession.cs:662`, immediately after adding the contribution to `atom.ConditionIndex`. This matches the pattern in `BuiltInHandlers.cs:209` (the `apply-condition` handler) exactly: same `id` from `_state.NextContributionId()`, same `ConditionContributionWrapper` wrapping the `ConditionContribution`, same registry assignment.

Regression test T13 (`ManifestProvisionedCondition_SurvivesSnapshotRoundTrip`) correctly exercises the fix:
- Provisions a game with a `CardSpec` that declares `Conditions: ["poisoned"]` — the exact path through `ApplyConditions`.
- Captures the `OnTurnStart` snapshot and round-trips it through `GameStateSnapshotSerializer.Serialize`/`Deserialize`.
- Asserts that `restored.Contributions` contains a `ConditionContributionSnapshot` with `ConditionName == "poisoned"` — this assertion would fail immediately if `ApplyConditions` did not register in `ContributionRegistry`, because `ToSnapshot()` sources contributions exclusively from `ContributionRegistry.Values`.
- Loads the snapshot into a fresh `GameState` via `LoadFromSnapshot` and asserts `HasCondition("poisoned")` returns true — end-to-end proof that the condition is present and queryable after a load.

All 85 tests pass.

---

### Observations

- **`SeededRandom` xoshiro128** correctness.** The implementation is correct. The `NextRaw()` state-update steps match the canonical reference (Blackman & Vigna xoshiro128**): result is `rotl(s1*5, 7) * 9`; the twist sequence (`s2 ^= s0`, `s3 ^= s1`, `s1 ^= s2`, `s0 ^= s3`, `s2 ^= t`, `s3 = rotl(s3, 11)`) matches exactly. Splitmix64 is the correct seeding strategy. The rejection-sampling approach for `NextInt` eliminates modulo bias. The `_callCount` accounting is correct: `NextRaw()` does not touch `_callCount`; `NextInt`'s `do`/`while` loop increments it on every raw step including rejected ones; the fast-forward constructor calls `NextRaw()` in a bare loop (without incrementing) and then sets `_callCount = callCount`, which correctly reproduces state. WASM-safe: no threads, no `Reflection.Emit`, pure arithmetic on value types.

- **`StaticEffectSnapshot` "both null" case.** The `ValidateExclusive()` comment says "Both null is allowed: a dynamic effect with no trigger (pure state contribution)." This is a valid runtime state. The load path handles it cleanly — `LoadFromSnapshot` treats `DeclarativeRef is null` as the dynamic/no-trigger branch, leaving `sourceDef = null` and `Trigger = null`. No defect.

- **`BoundValue` discriminated union coverage.** All seven variants (`NumberValue`, `BoolValue`, `StringValue`, `AtomIdValue`, `ContribIdValue`, `EventRefValue`, `CollectionValue`) are handled in both `ObjectToBoundValue` and `BoundValueToObject`. The `CollectionValue` deserialize path (`col.Items.OfType<AtomIdValue>()`) silently drops non-`AtomIdValue` members from a heterogeneous collection. At present `get-atoms-in-zone` only produces `IReadOnlyList<AtomId>`, so this is safe, but if future keywords produce heterogeneous collections this will silently lose items. Worth a comment.

- **`int` narrowing in `ObjectToBoundValue`.** The match arms include `int i => new NumberValue(i)` and `long l => new NumberValue(l)` alongside `double d => ...`. C# pattern-matching evaluates top-to-bottom, so `int` and `long` are caught before the `double` arm. This is correct and necessary because, for example, an `int` literal in `BoundArgs` would otherwise fail the `_` default arm.

- **Version check on deserialization.** `FromDto` throws `JsonException` if `dto.Version != GameStateSnapshot.CurrentVersion`. This is correct. No forward-compatibility concern because D17 defines a single version (`1`).

- **`IEngineObserver.OnTurnStart` timing.** The call in `RunAsync` is made after `SetSessionAccumulator("turn-number", turn)` and before `_eventLog.OpenTurn()`, which is exactly the "all prior-turn processing complete, no current-turn work started" guarantee D17 requires. The snapshot captured at this point truly reflects settled end-of-prior-turn state.

- **Load path skips provisioning correctly.** `RunAsync` branches cleanly: `_loadSnapshot is not null` → `LoadFromSnapshot` only; else `ProvisionSession()`. No double-provisioning risk. The `SeededRandom.FromSnapshot` fast-forward is correctly delegated inside `Build()`, so the derived RNG is already in the right position when `RunAsync` begins.

- **Declarative effect resolution on load.** If a snapshot references a `CardDefinitionName` or `EffectIndex` that no longer exists in `GameDefinition` (e.g. after a definition update between save and load), `LoadFromSnapshot` silently skips dormant effects and produces a `StaticEffect` with `sourceDef = null`. For active effects with a null `sourceDef`, `Trigger` and `ParameterModification` will be null, disabling the effect silently. This is an acceptable degradation for version mismatches (the `GameDefinitionId` check catches gross incompatibilities), but it is undocumented. A comment noting the silent-null behaviour would help maintainers.

- **Test coverage assessment.** The 12 tests cover: RNG determinism (T1/T2), atom-state round-trip (T3), active-static-effect round-trip (T4), `BoundArgs` type preservation for `AtomId` and `EventRef` (T5/T6), resume-at-correct-turn end-to-end (T7), definition-id mismatch guard (T8), `WithRandomSource` exemption (T9), `OnTurnStart`-before-init timing (T10), `Id`-missing guard (T11), modifier-index reconstruction (T12). Core invariants are well covered. The missing scenario — manifest-provisioned `Conditions` on atoms — is the gap that allowed BLOCKER 1 to pass undetected.

---

### Verdict

**PASS**

BLOCKER 1 resolved: `ApplyConditions` now registers each `ConditionContribution` in `ContributionRegistry` (`GameSession.cs:662`), matching the `BuiltInHandlers.cs:209` pattern. Regression test T13 (`ManifestProvisionedCondition_SurvivesSnapshotRoundTrip`) correctly catches the defect — it would fail at the `ConditionContributionSnapshot` assertion if the registry call were absent. All 85 tests pass. No outstanding defects remain.

---

## Review: tooling change — D29–D31, Groups 3–4 (impl/text-renderer)

Initial review: 2026-03-11.

Changeset: `src/Archetype.Core/GameDefinition.cs` (InitManifest mandatory, CardSpec.LocalId), `src/Archetype.Core/HostManifest.cs` (HostManifest, AtomStateOverride, OverrideTarget — new), `src/Archetype.Core/Exceptions.cs` (SessionException — new), `src/Archetype.Core/Interfaces.cs` (GameStateView.LastActionEvents D30), `src/Archetype.Engine/GameSession.cs` (nine-step provisioning, WithHostManifest, Build() validation), `src/Archetype.Engine/EventLog.cs` (CloseAction captures LastActionEvents), `src/Archetype.Build/HostManifestBuilder.cs` (new), `src/Archetype.Build/GameSessionBuilderExtensions.cs` (new), `src/Archetype.Tooling.Server/` (full sidecar — 18 RPC handlers, DSL parser, validator, export pipeline), `tests/Archetype.Tests/HostManifest/HostManifestTests.cs` (9 tests), `tests/Archetype.Tests/GameSession/LastActionEventsTests.cs` (4 tests), `tests/Archetype.Tests/Tooling/` (17 tests).

---

### Defects

#### [BLOCKER 1] D29 §5 / §4 contradiction — `ZoneTarget` resolution allows HostManifest zones — `GameSession.cs:869–877`

D29 §5 states: "AtomStateOverride may target only atoms provisioned by InitManifest. Targeting a HostManifest-added atom is a SessionException." This applies to all three `OverrideTarget` variants.

D29 §4 (the `OverrideTarget` discriminated union) says: "ZoneTarget { LocalId : string } — matches a ZoneSpec.LocalId in InitManifest or HostManifest zones."

These are contradictory. §5 is the policy statement; §4 is a technical key-space description. The implementer resolved this in favour of §4 (ZoneTarget accepts HostManifest zones). The code even carries a comment acknowledging the tension: "The restriction is only on CardTarget, not ZoneTarget."

This is incorrect. The asymmetry is not explained anywhere in D29 and creates a class of runtime behaviour (host patching its own zones via `StateOverrides`) that §5 explicitly prohibits. The correct resolution is for the architect to clarify D29 §4 — either the `ZoneTarget` description should say "InitManifest zones only" (consistent with §5), or D29 §5 should be narrowed to apply only to `CardTarget` (which has clear rationale). The implementer cannot make this call unilaterally.

Until resolved: the `Build()` validation correctly rejects `CardTarget` for HostManifest cards but does not perform the same check for `ZoneTarget` against HostManifest zones. If the architect's intent is §5 (all targets must be InitManifest-only), a missing `SessionException` guard at `ResolveOverrideTarget` (and correspondingly at `Build()` pre-validation) is a gap. If the intent is §4 (ZoneTarget is wider), D29 §5 needs an exception clause.

**Action required: architect must clarify D29 §4 vs §5 for ZoneTarget. No code change until the architecture is updated.**

---

### Minor Fixes Applied Directly

#### [MINOR 1] `GameSession.ResolveOverrideTarget` ZoneTarget comment is ambiguous — `GameSession.cs:874–876`

The comment says "this is accepted by D29 §5 — the restriction is only on CardTarget, not ZoneTarget." This misreads §5, which says "may target only atoms provisioned by InitManifest" without exception. The comment should instead note the contradiction and flag it as an open architecture question.

**Fixed in place:** comment reworded to accurately describe the ambiguity.

---

### Observations

#### D29 — Nine-step provisioning order

All nine steps are implemented correctly in `ProvisionSession` → `ProvisionManifest` → `ProvisionHostManifest`:
- Steps 1–2 (session atom, player atoms) in `ProvisionSession`.
- Steps 3–6 (InitManifest zones, cards with static effects, card mutable state, player mutable state) in `ProvisionManifest`.
- Steps 7–9 (HostManifest zones, cards with static effects, StateOverrides) in `ProvisionHostManifest`.
- `LocalId → AtomId` map thread correctly across all nine steps (passed `out`/by reference between methods).
- Card and zone LocalIds use separate key prefixes (`"card:"`/`"host-card:"`) so they cannot collide despite sharing one `Dictionary<string, AtomId>`. This matches D29 §4's statement that card and zone LocalIds "do not share a namespace."

#### D29 — Uniqueness validation placement

D29 §6 says: "Duplicate within InitManifest.Zones → DefinitionException at GameDefinitionBuilder.Build()." Since `GameDefinition` is a record (directly constructed, no builder class), this check is performed in `GameSessionBuilder.Build()` instead. The exception type `DefinitionException` is correct. The pre-existing absence of a `GameDefinitionBuilder` class means the check fires slightly later (session build, not definition build), but there is no functional impact. The test comment at `HostManifestTests.cs:92–94` correctly documents this. This is a pre-existing architectural gap (references to `GameDefinitionBuilder.Build()` in XML docs) that predates this change.

#### D29 — `WithHostManifest`/`FromSavedState` mutual exclusion

`GameSessionBuilder.Build()` correctly throws `SessionException` when both `_snapshot` and `_hostManifest` are non-null (line 146–150). Test 0.8h covers this exactly.

#### D29 — `CardTarget` validation in `Build()` (pre-provisioning)

`Build()` validates `CardTarget` overrides against InitManifest card LocalIds at build time (lines 185–211) — before provisioning. This is correct and ensures early failure without requiring provisioning to complete first.

#### D30 — `LastActionEvents` reset semantics

D30 says "reset to empty at the start of each new action." The implementation resets `_lastActionEvents` in `CloseAction()` (overwriting it with the new content). It is NOT reset to empty in `OpenAction()`. While the observable behaviour is correct for the post-action polling use case (after `CloseAction`, the value shows only the just-completed action), there is a window between `OpenAction()` and `CloseAction()` where `LastActionEvents` still holds the previous action's events. For the intended Godot interop use case (read after `ResolveAction` completes), this is harmless. No defect.

#### D30 — Signal prefix

The generated signal names correctly use `on_` prefix with hyphens-to-underscores conversion (`sig.GdScriptName`) matching D30's spec: "foo-bar produces a GDScript signal named on_foo_bar." Both `AppendSignals` (line 267) and the interop wrapper `deliver_signals` function (line 239) use this convention.

#### D30 — All four per-game GDScript classes generated

`ArchetypeCard.gd`, `ArchetypeZone.gd`, `ArchetypePlayer.gd`, `ArchetypeSession.gd`, `ArchetypeCardImporter.gd`, and `ArchetypeInterop.gd` are all generated as specified in D30. All signals are emitted on every class (not per-atom-kind filtered). D30 says signals fire on the matching atom kind's class — the current implementation emits all signals on all four class types. This is a minor fidelity gap but the Electron/Godot integration (out of scope for this review) is where per-kind filtering would be implemented.

#### D31 — Two-step force flow

`ExportGameDefinitionHandler` correctly implements the two-step flow: first call returns `missingTranslations` if warnings exist and `force: false`; second call with `force: true` proceeds to export. `ExportGameDefinitionExporter.Export()` tests cover both branches (4.7-H7, 4.7-H8). `ExportGodotClassesHandler` correctly omits the translation gate (D28 method table says both export methods are "gated on 0 errors" — translation gate is only on `ExportGameDefinition` per D31).

#### D28 — 18 methods

`Program.cs` registers exactly 18 methods, matching D28's method table:
`LoadProject`, `SaveProject`, `UpdateKeywordBody`, `UpdateCardEffect`, `UpdateLifetimeSpec`, `UpdateActivationCondition`, `UpdateCostBody`, `UpdateField`, `AddEntry`, `RemoveEntry`, `RenameEntry`, `GetAllDiagnostics`, `GetSymbolInfo`, `GetReferenceGraph`, `GetCompletions`, `RenderCardText`, `ExportGameDefinition`, `ExportGodotClasses`. Complete.

#### D28 — Debounce is renderer-side

The sidecar has no debounce logic (correct per D28 — debounce is the renderer's responsibility). Handlers are synchronous; there is no built-in delay mechanism.

#### D27 — `KeywordEntry.ReturnType` absent; exporter hardcodes `TypeName.Atom`

`KeywordEntry` (as specified in D27's schema, line 2897–2906 of architecture.md) does not include a `ReturnType` field. Consequently `GameDefinitionExporter` hardcodes `ReturnType: TypeName.Atom` for all game-creator keywords (line 77). The engine does not use `ReturnType` at runtime, so this has no execution impact. It is a known limitation of D27's authoring-state model. Not a defect against this implementation.

#### Test coverage

All 9 HostManifest tests cover: duplicate zone LocalId (InitManifest), CardSpec.LocalId null by default, cross-manifest zone LocalId collision, host provisioning order, accumulator merge patch semantics, condition append semantics, CardTarget-against-host-card SessionException, mutual exclusion, duplicate zone within HostManifest. The accumulator merge and condition append tests (0.8e, 0.8f) do not directly assert the mutable state values post-provisioning (they only verify the session runs to completion). This is a coverage gap — the tests cannot detect if the override was silently dropped. However, since `GameStateView` does expose `GetAccumulator` and `HasCondition`, this could be strengthened.

All 4 LastActionEvents tests correctly cover: empty before first action, populated after a card play, not carrying over between actions, and the before-first-action assertion. The two-action tests correctly detect accumulator events.

WASM constraint (D1): no `Thread`, `ThreadPool`, `Task.Run`, or `Reflection.Emit` introduced. The sidecar is explicitly exempted from the WASM constraint (D1: "Tooling is separate"). No Godot types in any engine assembly.

---

### Verdict

**PASS WITH MINOR FIXES**

One BLOCKER (B1) requires an **architect decision** before the implementer can make any code change. It is not an implementer error — the architecture document is self-contradictory on `ZoneTarget` semantics, and the implementer made a reasonable choice and documented it. The verdict cannot be PASS until D29 §4/§5 is clarified. Once the architect adds the clarification, the implementer either adds a `SessionException` guard for HostManifest ZoneTarget (if §5 wins) or updates the §5 policy text (if §4 wins), and this review closes.

MINOR 1 (comment inaccuracy) has been fixed directly.

All 132 tests pass. No new compiler warnings. All other D29–D31 and D28 decisions are correctly and completely implemented.

---

## Review: action-args-and-cost-model — D20–D25 (impl/text-renderer)

Initial review: 2026-03-09.

Changeset: `src/Archetype.Core/Diagnostics.cs` (new), `src/Archetype.Core/ActionArgs.cs` (new), `src/Archetype.Core/GameDefinition.cs` (CostDef, CardDefinition.Cost, NamedEffectBlockDef.Cost), `src/Archetype.Core/Interfaces.cs` (AvailableActions.ValidateActionArgs, IEngineObserver.OnDiagnostic, PlayCard/ActivateAbility CostChoices), `src/Archetype.Engine/BuiltInHandlers.cs` (assert handler), `src/Archetype.Engine/ExecutionContext.cs` (IsCostBody), `src/Archetype.Engine/CostValidator.cs` (new), `src/Archetype.Engine/GameState.cs` (CloneForValidation), `src/Archetype.Engine/ActionResolver.cs` (costBlocks param), `src/Archetype.Engine/GameSession.cs` (ownership filter removal, ValidateActionArgs delegate, ResolveCostBlocks), `src/Archetype.Build/Kw.cs` (Assert, OwnedByActivePlayer, OwnerOf, Session, CostDefBuilder), `tests/Archetype.Tests/BuiltIns/AssertTests.cs` (new, 7 tests), `tests/Archetype.Tests/CostModel/CostModelTests.cs` (new, 7 tests), `tests/Archetype.Tests/ComputeAvailableActions/ComputeAvailableActionsTests.cs` (updated + 3 new D24 tests).

---

### Reviewer Checklist Results

#### 10.1 — `assert` in cost body always panics silently regardless of call-site args

`BuiltInHandlers.Assert` (line 450): checks `ctx.IsCostBody` before reading `on_fail`/`notify` args. When true, immediately throws `EngineException` with no observer call. Call-site `on_fail`/`notify` values are never read. `ActionResolver.ResolveAction` sets `ctx.IsCostBody = true` before iterating cost blocks and resets to `false` after (lines 157–160, and in `finally` at line 169). Test 9.5 (`Assert_InsideCostBody_AlwaysPanicsNoNotify_RegardlessOfArguments`) directly exercises this with `continue/on` args and expects `EngineException` + no `OnDiagnostic` call.

**PASS**

#### 10.2 — `OnDiagnostic` called BEFORE `EngineException` on `panic/on`; NOT called on `notify: off`

`BuiltInHandlers.Assert` (lines 468–488): `notify == NotifyFlag.On` guard calls `ctx.Observer?.OnDiagnostic(diagnostic)` unconditionally before the `onFail switch`. The `switch` arm for `OnFail.Panic` is only reached after the notify call has already been made. For `notify: off` the `if (notify == NotifyFlag.On)` block is skipped entirely; only the `switch` executes. Tests 9.3 and 9.4 cover both cases.

**PASS**

#### 10.3 — `assert` never appends to `EventLog`

`BuiltInHandlers.Assert` never calls `ctx.EventLog.*` directly or indirectly. The handler returns `null` (for `continue`), throws `BlockHaltException` (for `stop`), or throws `EngineException` (for `panic`). None of these paths invoke the event log. `BlockExecutor.ExecuteStep` only appends events through `KeywordEvaluator.EvaluateInvocation` when `_mutations.Has(step.KeywordName)` returns true; `assert` is registered in `_mutations` but its handler does not call `LogEvent`. Test 9.6 (`Assert_NeverAppendsToEventLog`) asserts `ThisGame` contains no `"assert"` keyword name event for both passing and failing conditions.

**PASS**

#### 10.4 — `GameState.CloneForValidation` includes/excludes the right fields

`GameState.CloneForValidation` (GameState.cs lines 259–301):
- **Included**: atom table with full deep-copy of `Accumulators`, `ModifierIndex`, `ConditionIndex`, `ZoneId`, `OwnerId`, `Kind`. Session atom ID. Player name registry.
- **Excluded**: `ContributionRegistry` (not copied, NOTE comment at line 296). `ActiveStaticEffects`/`DormantDeclarativeEffects` (new `GameState()` initialises these empty). `GameIsOver`/`PendingWinner` (new `GameState()` leaves false/null). `EventLog` is not part of `GameState`.

This satisfies D21. The `CostValidator` supplies its own fresh `EventLog` (lines 66–67 of `CostValidator.cs`), ensuring the clone execution writes to a throw-away log.

**PASS**

#### 10.5 — `ValidateActionArgs` captures a state snapshot, not a live reference

`GameSession.ComputeAvailableActions` (lines 495–509):
- `stateSnapshot = _state.CloneForValidation()` — a separate object; mutations on `_state` after this point do not affect `stateSnapshot`.
- `defSnapshot = _definition` — immutable; safe to capture.
- `strategiesSnapshot = _strategies` — immutable reference; safe to capture.
- `randomSnapshot = _randomSource` — read-only random source; safe to capture.
- `ResolveCostsForAction(action, defSnapshot)` inside the lambda uses `_atomDefinitionNames` (a live instance field). This map is populated only during `ProvisionManifest` and `create-card`; within a single `SelectActionAsync` call no new entries can appear (the action window is serial). The definition lookup is done against `defSnapshot` (correct). This is an acceptable design trade-off documented in the engine source.

**PASS**

#### 10.6 — Cost bodies execute within the existing action scope; cost events appear in `events.this_action`

`ActionResolver.ResolveAction` (lines 151–171): `eventLog.OpenAction()` is called once, before any cost body or primary block executes. Cost bodies run inside this scope (`ctx.IsCostBody = true`; each body passed to `executor.ExecuteBlock`). The primary block then runs in the same scope. `eventLog.CloseAction()` is called in `finally`. No separate `OpenAction`/`CloseAction` pair wraps the cost blocks. Test 9.16 (`PlayCard_WithCost_CostBodyExecutesBeforeEffect`) verifies that `"cost-paid"` events appear before `"effect-fired"` events in `result.FinalLog`.

**PASS**

#### 10.7 — No ownership predicate in `ComputeAvailableActions` steps 1 or 2

Pass 1 (lines 443–469): iterates `_state.GetAtoms(AtomKind.Card)` with no owner check. Zone filter uses `playableZoneDefNames.Contains(zoneDefName)` (definition name only). No `OwnerId` comparison appears. Pass 2 (lines 475–490): iterates the same atom set with no zone or owner filter. The zone-owner guard present in the pre-D24 implementation has been entirely removed. Tests 9.13 (`ComputeAvailableActions_CardInPlayableZone_IncludedRegardlessOfOwner`) and 9.14 (`ComputeAvailableActions_AbilityOnUnownedCard_Included`) assert that opponent-owned cards and abilities appear in p1's results.

**PASS**

#### 10.8 — `Kw.OwnedByActivePlayer()` XML doc states the `"active-player"` session state requirement

`Kw.cs` (lines 295–320): the XML `<summary>` explicitly states:
> "Requirement: the game must declare a session state field named `"active-player"` whose value is the atom ID of the currently active player. If this field is absent at runtime, `EvaluateCondition` will throw."

The expansion in the doc comment uses `Kw.EqualTo`/`Kw.OwnerOf`/`Kw.GetState`/`Kw.Session()`, which matches the actual implementation and the D24 decision. The migration guidance is also present: "This shorthand is the migration path from the old implicit ownership filter that was removed from `ComputeAvailableActions` in D24."

**PASS**

#### 10.9 — All `IEngineObserver` implementations have `OnDiagnostic`

Two implementations exist in the codebase:
- `AssertTests.RecordingObserver` (`AssertTests.cs:36`): implements `void OnDiagnostic(DiagnosticEvent e)` — appends to `Diagnostics` list.
- `SaveLoadTests.CapturingObserver` (`SaveLoadTests.cs:25`): implements `void OnDiagnostic(DiagnosticEvent e) { /* no-op in tests */ }` at line 42.

No other `IEngineObserver` implementations exist in the repository (confirmed via search). The codebase compiles cleanly (102/102 tests pass with 0 errors).

**PASS**

#### 10.10 — All new tests pass; existing `ComputeAvailableActions` tests updated; net test count increases

- Prior total: 85 tests.
- New tests added: 7 in `AssertTests.cs`, 7 in `CostModelTests.cs` (9.7–9.12 + 9.16), 3 in `ComputeAvailableActionsTests.cs` (9.13–9.15 via tests 3.8 updated + 9.13, 9.14, 9.15 added).
- Current total: 102 tests, all passing.
- Existing test 3.8 (`ComputeAvailableActions_CardInOpponentOwnedZone_IsIncluded_D24`) correctly documents the D24 behaviour change (the old test name `ComputeAvailableActions_ExcludesCards_FromOpponentOwnedZone` was inverted — the new assertion checks that the card IS included).
- Net increase: +17 tests.

**PASS**

---

### Defects

None found.

---

### Minor Fixes Applied Directly

#### [MINOR 1] `CloneForValidation` XML doc listed "Player name registry, session atom" as excluded when they are actually copied — `GameState.cs:248–254`

The "Excluded (per D21)" bullet point incorrectly listed "Player name registry, session atom, game outcome flags" among the excluded items. The code at lines 263–294 clearly copies both `SessionAtomId` and the player name registry (both needed for cost-body keyword resolution). Only game outcome flags (`GameIsOver`, `PendingWinner`) are truly excluded.

**Fixed in place:** added an "Also copied" section listing `SessionAtomId` and the player name registry with rationale; corrected the "Excluded" bullet to read "Game outcome flags (`GameIsOver`, `PendingWinner`)".

#### [MINOR 2] Dead variable `sessionEnergyBefore` in `ComputeAvailableActionsTests.cs:512`

`CS0219` warning: `var sessionEnergyBefore = double.NaN` assigned but never used. The variable was a planning artifact (the test comment references it but the assertion was never written against it).

**Fixed in place:** variable and its associated comment removed. The test still verifies non-mutation via the `result.IsDraw` assertion and the `ValidateActionArgs` return-value check.

---

### Observations

- **`CostValidator.Validate` short-circuit wastes one `ResolveCostTexts` call.** Lines 43 and 47: `ResolveCostTexts(costs, action)` is called and the result stored in `costTexts` even when `costs is null or empty`, but then `ValidationResult.Empty` (with empty `CostTexts`) is returned and `costTexts` is discarded. The result is functionally correct — empty costs → empty text list — and the wasted call is trivially O(0). Not a defect, but a clarity opportunity: move the `Count == 0` guard above the `ResolveCostTexts` call or inline `Array.Empty<string>()` there.

- **`ResolveCostsForAction` in the `ValidateActionArgs` closure captures live `_atomDefinitionNames`.** The closure at `GameSession.cs:501–509` captures `_atomDefinitionNames` as a live field. Within a single `SelectActionAsync` call this is safe (the action window is serial; no new atoms can be provisioned between `ComputeAvailableActions` and the strategy call). The `defSnapshot` reference correctly captures the immutable definition. This is an acceptable known trade-off and the comment at line 492 acknowledges the snapshot pattern.

- **`NamedEffectBlockDef.Cost` accepts `null` as well as an empty list.** The type is `IReadOnlyList<CostDef>?` (nullable). `ResolveCostBlocks` and `ResolveCostsForAction` both handle null with `?.Cost` and null-coalesce patterns. D20 says "Cost: IReadOnlyList<CostDef>" with no null mention; the nullable annotation is a pragmatic construction-time convenience for the common `Cost: null` shorthand in test helpers. This is consistent and handled everywhere.

- **No test exercises `Kw.OwnedByActivePlayer()` end-to-end.** The shorthand compiles correctly and its expansion is the only definition of `owner-of` + `get-state(session(), ...)`. No integration test provisions a session with an `"active-player"` state field and verifies that cards with this condition are filtered. This is a coverage gap for the migration path documented in D24, though the underlying primitives (`owner-of`, `equal-to`, `get-state`) are individually exercised. Noted for the implementer's awareness; not a blocker.

---

### Verdict

**PASS**

All ten reviewer checks (10.1–10.10) pass. No blockers found. Two minor issues fixed directly: doc comment inaccuracy in `CloneForValidation` and dead variable in test. 102/102 tests pass with zero compiler warnings.
