# Review: Text Renderer — `Archetype.Text` + `Archetype.Core/RenderNode.cs` (impl/text-renderer)

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
