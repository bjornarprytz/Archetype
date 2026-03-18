## 1. Core Types — CostDef, ValidationResult, DiagnosticEvent (Archetype.Core)

- [x] 1.1 Add `CostDef` record to `Archetype.Core` with fields `Body: EffectBlockDef`, `Parameters: ParameterDecl[]`, `TextTemplate: string?` (no `EvaluationFunction`)
  - reads: `docs/architecture.md#D20`, `src/Archetype.Core/Keywords.cs`
  - writes: `src/Archetype.Core/CostDef.cs`
- [x] 1.2 Add `ValidationResult` record to `Archetype.Core` with fields `IsValid: bool`, `CostTexts: IReadOnlyList<string>`
  - reads: `docs/architecture.md#D21`, `docs/architecture.md#D22`
  - writes: `src/Archetype.Core/ActionArgs.cs`
- [x] 1.3 Add `Cost: IReadOnlyList<CostDef>` to `CardDefinition`; default empty list; update `CardDefinitionBuilder` with `WithCost(CostDef cost)` method
  - reads: `docs/architecture.md#D20`, `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Core/GameDefinition.cs`
- [x] 1.4 Replace `Cost: EffectBlockDef?` with `Cost: IReadOnlyList<CostDef>` on `NamedEffectBlockDef`; update all construction and pattern-match sites (breaking change — see D25)
  - reads: `docs/architecture.md#D20`, `docs/architecture.md#D25`, `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Core/GameDefinition.cs`
- [x] 1.5 Add `ValidateActionArgs: Func<PlayerAction, ValidationResult>` to `AvailableActions` struct (breaking change — see D25)
  - reads: `docs/architecture.md#D22`, `docs/architecture.md#D25`, `src/Archetype.Core/Interfaces.cs`
  - writes: `src/Archetype.Core/Interfaces.cs`
- [x] 1.6 Add `OnFail` enum (`continue | stop | panic`) and `NotifyFlag` enum (`on | off`) to `Archetype.Core`; these are NOT `ParameterType` values
  - reads: `docs/architecture.md#D20`
  - writes: `src/Archetype.Core/BuiltIns.cs` (or a new `src/Archetype.Core/AssertTypes.cs`)
- [x] 1.7 Add `DiagnosticKind` enum (int-backed; `AssertionFailed = 0`) and `DiagnosticEvent` record (`Kind`, `Message`, `ConditionNode: KeywordNode?`, `OnFail`, `Location: string`) to `Archetype.Core`
  - reads: `docs/architecture.md#D25`
  - writes: `src/Archetype.Core/Diagnostics.cs`
- [x] 1.8 Add `void OnDiagnostic(DiagnosticEvent e)` to `IEngineObserver` interface (breaking change — all implementations must add method; add default no-op to any base/adapter class)
  - reads: `docs/architecture.md#D25`, `src/Archetype.Core/Interfaces.cs`
  - writes: `src/Archetype.Core/Interfaces.cs`

## 2. PlayerAction — CostArgs (Archetype.Core)

- [x] 2.1 Verify `PlayCard` and `ActivateAbility` already carry `CostArgs: IReadOnlyDictionary<string, object>` and `Targets: IReadOnlyList<AtomId>`; add if missing; ensure `Pass` carries neither
  - reads: `docs/architecture.md#D21`, `src/Archetype.Core/Interfaces.cs`
  - writes: `src/Archetype.Core/Interfaces.cs`

## 3. Build Helpers (Archetype.Build)

- [x] 3.1 Add `CostDefBuilder` or factory methods in `Archetype.Build` for constructing `CostDef` instances
  - reads: `src/Archetype.Build/Kw.cs`, `openspec/changes/action-args-and-cost-model/design.md`
  - writes: `src/Archetype.Build/CostDef.cs`
- [x] 3.2 Add `Kw.Assert(condition: KeywordNode, onFail: OnFail = OnFail.Continue, notify: NotifyFlag = NotifyFlag.On) → Invocation` to `Kw`
  - reads: `docs/architecture.md#D20`, `src/Archetype.Build/Kw.cs`
  - writes: `src/Archetype.Build/Kw.cs`
- [x] 3.3 Add `Kw.OwnedByActivePlayer() → KeywordNode` shorthand to `Archetype.Build`; expands to `Kw.Eq(Kw.OwnerOf(Kw.Param("source")), Kw.GetState(Kw.Session(), "active-player"))`; add XML doc noting the `"active-player"` session state requirement
  - reads: `src/Archetype.Build/Kw.cs`, `docs/architecture.md#D24`
  - writes: `src/Archetype.Build/Kw.cs`
- [x] 3.4 Verify `Kw.OwnerOf` and `Kw.GetState(Kw.Session(), ...)` exist in `Archetype.Build`; add if missing
  - reads: `src/Archetype.Build/Kw.cs`, `docs/architecture.md#D24`
  - writes: `src/Archetype.Build/Kw.cs`

## 4. Engine — assert built-in and diagnostics (Archetype.Engine)

- [x] 4.1 Register `assert` in `BuiltInKeywords` with three parameters: `condition: Boolean`, `on_fail: OnFail`, `notify: NotifyFlag`; add handler in `BlockExecutor` that reads `ExecutionContext.IsCostBody` to override `on_fail`/`notify` to `panic`/`off` regardless of call-site arguments
  - reads: `docs/architecture.md#D20`, `src/Archetype.Engine/BlockExecutor.cs`, `src/Archetype.Engine/BuiltInKeywords.cs`
  - writes: `src/Archetype.Engine/BuiltInKeywords.cs`, `src/Archetype.Engine/BlockExecutor.cs`
- [x] 4.2 Add `bool IsCostBody` flag to `ExecutionContext`; set it `true` before each `CostDef.Body` execution and reset after
  - reads: `docs/architecture.md#D20`, `src/Archetype.Engine/ExecutionContext.cs`
  - writes: `src/Archetype.Engine/ExecutionContext.cs`
- [x] 4.3 Implement `OnDiagnostic` call path in `BlockExecutor`: when `assert` fails and `notify == on` (and `IsCostBody` is false), construct `DiagnosticEvent` and call `IEngineObserver.OnDiagnostic`; when `on_fail == panic`, call `OnDiagnostic` first, then raise `EngineException`; when `on_fail == stop`, call `OnDiagnostic` (if notify on) then halt block; when `on_fail == continue`, call `OnDiagnostic` (if notify on) then proceed
  - reads: `docs/architecture.md#D20`, `docs/architecture.md#D25`, `src/Archetype.Engine/BlockExecutor.cs`
  - writes: `src/Archetype.Engine/BlockExecutor.cs`

## 5. Engine — Sequential Cost Validation (Archetype.Engine)

- [x] 5.1 Implement lightweight `GameState` clone: shallow copy of atom table and accumulator maps, zone membership, and condition presence; no event log, no contribution registries, no observer reference; add `GameState.CloneForValidation()` internal method
  - reads: `docs/architecture.md#D21`, `src/Archetype.Engine/GameState.cs`
  - writes: `src/Archetype.Engine/GameState.cs`
- [x] 5.2 Implement `CostValidator.Validate(IReadOnlyList<CostDef> costs, PlayerAction action, GameState state, GameDefinition def) → ValidationResult`; concatenate all cost bodies into a single composite `EffectBlockDef`; execute against clone with `IsCostBody = true`; if `EngineException` is thrown set `IsValid = false`; always resolve all `CostTexts` from `TextTemplate` + `CostChoices` regardless of outcome
  - reads: `docs/architecture.md#D21`, `openspec/changes/action-args-and-cost-model/specs/cost-model/spec.md`
  - writes: `src/Archetype.Engine/CostValidator.cs`
- [x] 5.3 Wire `ValidateActionArgs` delegate construction in `ComputeAvailableActions`: create a closure over current `GameState` snapshot and `GameDefinition`; call `CostValidator.Validate` with the relevant `CostDef` list for the action type; when action has no costs return `ValidationResult { IsValid = true, CostTexts = [] }` immediately
  - reads: `src/Archetype.Engine/GameSession.cs`, `docs/architecture.md#D22`
  - writes: `src/Archetype.Engine/GameSession.cs`

## 6. Engine — Ownership Filter Removal (Archetype.Engine)

- [x] 6.1 Rewrite `ComputeAvailableActions` Step 1 (PlayCard): iterate all cards in playable zones regardless of owner; remove any `zone.OwnerId == activePlayer` predicate
  - reads: `docs/architecture.md#D24`, `src/Archetype.Engine/GameSession.cs`, `openspec/changes/action-args-and-cost-model/specs/available-actions-contract/spec.md`
  - writes: `src/Archetype.Engine/GameSession.cs`
- [x] 6.2 Rewrite `ComputeAvailableActions` Step 2 (Abilities): iterate all card atoms regardless of zone or owner; remove any ownership guard
  - reads: `docs/architecture.md#D24`, `src/Archetype.Engine/GameSession.cs`
  - writes: `src/Archetype.Engine/GameSession.cs`

## 7. Engine — Cost Payment at Execution Time (Archetype.Engine)

- [x] 7.1 In `ActionResolver` (or `GameSession.TranslatePlayerAction`), before executing the primary `EffectBlockDef`, execute each `CostDef.Body` in declaration order within the same action scope with `IsCostBody = true`; bind cost args from `PlayerAction.CostChoices` into the execution context per `CostDef.Parameters`
  - reads: `docs/architecture.md#D23`, `src/Archetype.Engine/ActionResolver.cs`, `src/Archetype.Engine/GameSession.cs`
  - writes: `src/Archetype.Engine/ActionResolver.cs`

## 8. Breaking Change Migrations

- [x] 8.1 Search all `IEngineObserver` implementations (including test fakes and any base/adapter class); add `void OnDiagnostic(DiagnosticEvent e)` as a no-op or logging stub to each
  - reads: `docs/architecture.md#D25`, `src/Archetype.Core/Interfaces.cs`
  - writes: any file implementing `IEngineObserver`
- [x] 8.2 Search all `new AvailableActions {` struct literals; add `ValidateActionArgs = ...` field to each; use `(_) => ValidationResult.Empty` for any test or stub that does not exercise cost validation
  - reads: `docs/architecture.md#D25`, `src/Archetype.Core/Interfaces.cs`
  - writes: any file constructing `AvailableActions`
- [x] 8.3 Search all `NamedEffectBlockDef` construction sites with a `Cost:` field; migrate from `Cost: EffectBlockDef?` to `Cost: IReadOnlyList<CostDef>`; wrap existing `EffectBlockDef` in a single `CostDef` if needed or replace with empty list
  - reads: `docs/architecture.md#D25`, `src/Archetype.Core/GameDefinition.cs`
  - writes: any file constructing `NamedEffectBlockDef`
- [x] 8.4 Audit all `ComputeAvailableActions` call sites and downstream tests that assumed only the active player's cards appear in results; add `ActivationCondition: Kw.OwnedByActivePlayer()` to affected card/ability definitions, or update test assertions to reflect the new broader result set
  - reads: `docs/architecture.md#D24`, `docs/architecture.md#D25`
  - writes: any test or game definition file relying on implicit ownership filtering

## 9. Tests

- [x] 9.1 `Assert_OutsideCostBody_ContinueNotify_CallsOnDiagnosticNoException` — assert fails with defaults (`continue`/`on`); `OnDiagnostic` is called; no exception; execution continues to next step
  - reads: `docs/architecture.md#D20`
  - writes: `tests/Archetype.Tests/BuiltIns/AssertTests.cs`
- [x] 9.2 `Assert_OutsideCostBody_StopNotify_CallsOnDiagnosticHaltsBlock` — assert fails with `stop`/`on`; `OnDiagnostic` is called; block halts; no exception
  - reads: `docs/architecture.md#D20`
  - writes: `tests/Archetype.Tests/BuiltIns/AssertTests.cs`
- [x] 9.3 `Assert_OutsideCostBody_PanicNotify_CallsOnDiagnosticThenThrows` — assert fails with `panic`/`on`; `OnDiagnostic` is called before `EngineException` is raised
  - reads: `docs/architecture.md#D20`
  - writes: `tests/Archetype.Tests/BuiltIns/AssertTests.cs`
- [x] 9.4 `Assert_OutsideCostBody_PanicNoNotify_NoOnDiagnosticThrows` — assert fails with `panic`/`off`; `OnDiagnostic` is NOT called; `EngineException` is raised
  - reads: `docs/architecture.md#D20`
  - writes: `tests/Archetype.Tests/BuiltIns/AssertTests.cs`
- [x] 9.5 `Assert_InsideCostBody_AlwaysPanicsNoNotify_RegardlessOfArguments` — assert with `continue`/`on` inside a cost body raises `EngineException` and does NOT call `OnDiagnostic`
  - reads: `docs/architecture.md#D20`
  - writes: `tests/Archetype.Tests/BuiltIns/AssertTests.cs`
- [x] 9.6 `Assert_NeverAppendsToEventLog` — assert succeeds and fails; neither outcome produces an event log entry
  - reads: `docs/architecture.md#D20`
  - writes: `tests/Archetype.Tests/BuiltIns/AssertTests.cs`
- [x] 9.7 `CostValidator_SingleCost_Affordable_ReturnsValid` — cost body's assert condition is true; `IsValid` is true; `GameState` unchanged
  - reads: `docs/architecture.md#D21`
  - writes: `tests/Archetype.Tests/CostModel/CostModelTests.cs`
- [x] 9.8 `CostValidator_SingleCost_NotAffordable_ReturnsInvalid` — cost body's assert condition is false; `IsValid` is false; `GameState` unchanged
  - reads: `docs/architecture.md#D21`
  - writes: `tests/Archetype.Tests/CostModel/CostModelTests.cs`
- [x] 9.9 `CostValidator_MultipleCosts_CombinedBodyFails_ReturnsInvalid` — two costs: first drains resource, second asserts resource remains; combined block fails; `IsValid` is false; `GameState` unchanged
  - reads: `docs/architecture.md#D21`
  - writes: `tests/Archetype.Tests/CostModel/CostModelTests.cs`
- [x] 9.10 `CostValidator_AllCostsPass_GameStateUnchanged` — validation succeeds; original `GameState` not mutated
  - reads: `docs/architecture.md#D21`
  - writes: `tests/Archetype.Tests/CostModel/CostModelTests.cs`
- [x] 9.11 `CostValidator_MissingCostArg_ReturnsInvalid` — `CostDef` declares a parameter; `CostChoices` is empty; `IsValid` is false
  - reads: `docs/architecture.md#D21`
  - writes: `tests/Archetype.Tests/CostModel/CostModelTests.cs`
- [x] 9.12 `CostValidator_CostTexts_PopulatedRegardlessOfOutcome` — validation fails; `CostTexts` still contains one resolved string per `CostDef`
  - reads: `docs/architecture.md#D21`
  - writes: `tests/Archetype.Tests/CostModel/CostModelTests.cs`
- [x] 9.13 `ComputeAvailableActions_CardInPlayableZone_IncludedRegardlessOfOwner` — card owned by opponent is in a playable zone; `PlayCard` action is included
  - reads: `docs/architecture.md#D24`
  - writes: `tests/Archetype.Tests/ComputeAvailableActions/ComputeAvailableActionsTests.cs`
- [x] 9.14 `ComputeAvailableActions_AbilityOnUnownedCard_Included` — card owned by opponent has an ability with no `ActivationCondition`; `ActivateAbility` action is included
  - reads: `docs/architecture.md#D24`
  - writes: `tests/Archetype.Tests/ComputeAvailableActions/ComputeAvailableActionsTests.cs`
- [x] 9.15 `ComputeAvailableActions_ValidateActionArgs_DoesNotMutateState` — call `ValidateActionArgs` on a valid action; assert `GameState` is unchanged
  - reads: `docs/architecture.md#D22`
  - writes: `tests/Archetype.Tests/ComputeAvailableActions/ComputeAvailableActionsTests.cs`
- [x] 9.16 `PlayCard_WithCost_CostBodyExecutesBeforeEffect` — card has a cost; event log shows cost events before primary effect events in `events.this_action`
  - reads: `docs/architecture.md#D23`
  - writes: `tests/Archetype.Tests/CostModel/CostModelTests.cs`
- [x] 9.17 Update existing `ComputeAvailableActions` tests that relied on implicit ownership filtering: add `ActivationCondition: Kw.OwnedByActivePlayer()` or update assertions for the new broader result set
  - reads: `tests/Archetype.Tests/ComputeAvailableActions/ComputeAvailableActionsTests.cs`
  - writes: `tests/Archetype.Tests/ComputeAvailableActions/ComputeAvailableActionsTests.cs`

## 10. Reviewer Checks

- [x] 10.1 Reviewer: verify `assert` in cost body always panics silently regardless of `on_fail`/`notify` arguments at the call site; inspect `BlockExecutor` for the `IsCostBody` guard
- [x] 10.2 Reviewer: verify `OnDiagnostic` is called BEFORE `EngineException` is raised when `on_fail: panic, notify: on`; verify it is NOT called when `notify: off`
- [x] 10.3 Reviewer: verify `assert` never appends to `EventLog` under any outcome (success, continue, stop, panic)
- [x] 10.4 Reviewer: verify `GameState` clone in `CostValidator` excludes `EventLog`, contribution registries, and active static effects; includes atom table, accumulators, zone membership, condition presence
- [x] 10.5 Reviewer: verify `ValidateActionArgs` delegate captures a snapshot of state at `ComputeAvailableActions` time; no live reference that can drift
- [x] 10.6 Reviewer: verify cost bodies execute within the existing action scope (no separate `OpenAction`/`CloseAction` pair) and cost events appear in `events.this_action`
- [x] 10.7 Reviewer: verify no ownership predicate remains in `ComputeAvailableActions` steps 1 or 2
- [x] 10.8 Reviewer: verify `Kw.OwnedByActivePlayer()` XML doc states the `"active-player"` session state requirement
- [x] 10.9 Reviewer: verify all `IEngineObserver` implementations have `OnDiagnostic`; no compilation errors
- [x] 10.10 Reviewer: verify all new tests pass and existing `ComputeAvailableActions` tests are updated; net test count increases
