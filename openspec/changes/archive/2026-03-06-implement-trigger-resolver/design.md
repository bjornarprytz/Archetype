## Context

Tier 2 execution is complete: `BlockExecutor`, `KeywordEvaluator`, `MutationDispatch`, `PropertyDispatch`, and `BuiltInHandlers` are all implemented and tested. `LifetimeChecker` implements the two-phase while-condition lifecycle (Phase 1 expire, Phase 2 dormant re-activation). `ExecutionContext.CreateChildActionContext` exists for spawning new action scopes.

What does not yet exist:
- `TriggerResolver` — collects candidate trigger firings from the event log using per-effect high-water marks, evaluates conditions, orders results, and fires triggered blocks.
- `ActionResolver` — orchestrates the full per-action sequence: primary block → state-based rules → cascade loop (trigger collection → trigger firing → state-based rules → repeat until quiescent).
- `RunStateBasedRules` — fixpoint loop evaluating `StateBasedRule` conditions and executing their blocks.

Layer 2 test 6.7 manually simulates what `ActionResolver` should do automatically. It will be updated to use the real `ActionResolver` as part of this change.

Architecture decisions D7 and D8 are fully specified. This design translates them into the exact class and method structure the implementer will write.

## Goals / Non-Goals

**Goals:**

- Implement `TriggerResolver` with high-water mark collection, condition evaluation via `TriggerEvaluationContext`, and `OldestFirst` ordering.
- Implement `ActionResolver` with the full post-action sequence from D7.
- Implement `RunStateBasedRules` (fixpoint loop) as a functional first pass — no tests in this change, but it must not be a no-op stub since `ActionResolver` calls it unconditionally.
- Update Layer 2 test 6.7 to use real `ActionResolver` instead of manual simulation.
- Add `TriggerResolution/` tests covering: single trigger fires, trigger does not fire when condition false, high-water mark prevents double-firing, trigger chain (trigger → event → another trigger fires in next cascade batch), multiple triggers ordered correctly.

**Non-Goals:**

- `PromptPlayer` trigger ordering — `TriggerOrderPrompt` path not implemented. `OldestFirst` only in this change.
- State-based rule tests — a follow-up change will add dedicated SBR tests.
- `GameSessionBuilder` / `GameSession` — depends on this change.
- Text renderer — independent.
- `IEngineObserver.OnTriggerCascade` — the interface is defined; the cascade loop will call it but tests will pass `null` (always `Continue`).

## Decisions

### Decision 1: `TriggerResolver` as a separate class vs. folding into `ActionResolver`

**Chosen:** `TriggerResolver` is a separate `internal` class, injected into `ActionResolver`.

**Rationale:** `CollectSatisfiedTriggers` has its own distinct responsibility (event log scan, condition evaluation, ordering) that warrants isolation. Keeping it separate makes unit-testing the collection logic possible without exercising the full action lifecycle. `ActionResolver` stays focused on the sequence orchestration.

**Alternatives considered:** A single `ActionResolver` class with private methods — simpler but makes the collection path harder to test in isolation.

### Decision 2: `TriggerEvaluationContext` vs. reusing `ExecutionContext`

**Chosen:** `TriggerEvaluationContext` is a distinct lightweight record (struct or sealed class), as specified in D8. It carries only what a trigger condition needs: the candidate event's bound args (via `EventParams` mapping), read-only `GameState`, and the declared log scope (`TriggerScope`). It does not carry `Bindings`, `PromptChannel`, or mutable state.

**Rationale:** Trigger conditions are pure read-only evaluations — they must not fire mutations and must not access block-scoped variables. Using `ExecutionContext` would expose these paths. A separate type makes the contract explicit and prevents accidental misuse.

**Implementation note:** `KeywordEvaluator.EvaluateCondition` already exists and accepts a `GameState` + `Dictionary<string, object>`. `TriggerEvaluationContext` provides the bindings dictionary to that method.

### Decision 3: High-water mark advancement strategy

**Chosen:** Advance past ALL candidate events seen in a collection pass (whether condition matched or not), as specified in D8. The high-water mark on each `StaticEffect` is updated to `max(seen candidate SequenceNumbers)` after the scan, unconditionally.

**Rationale:** This matches the domain model's "fires at most once per event" rule (§5.3). Events from trigger-fired blocks get new `SequenceNumber`s above the current high-water mark, so they are visible in the next cascade batch. Events from the primary block are never re-evaluated.

### Decision 4: `RunStateBasedRules` — full implementation now, tests later

**Chosen:** Implement the full fixpoint loop as specified in D7. `ActionResolver.RunStateBasedRules` is not a stub — it evaluates `GameDefinition.StateBasedRules` and executes triggered blocks. No dedicated SBR tests in this change; the trigger chain tests will exercise the method indirectly (with no SBRs registered, it returns immediately — confirming the no-SBR path is safe).

**Rationale:** `ActionResolver` calls `RunStateBasedRules` at multiple points in the post-action sequence. A stub (always returning immediately) would silently pass tests even if the loop is wrong. A real implementation fails correctly when broken. The implementation is simple given that `EvaluateCondition` and `ExecuteBlock` are already available.

### Decision 5: `PromptPlayer` ordering — declaration-order fallback

**Chosen:** If `GameDefinition.TriggerResolutionOrder == PromptPlayer`, fall back to `OldestFirst` in this change and log a warning via `IEngineObserver` (or no-op if null). `TriggerOrderPrompt` support is deferred.

**Rationale:** The `PromptPlayer` path requires the full `IPromptChannel` suspension pattern and is not exercised by any existing or planned test in this change. Deferring it keeps scope tight.

## Risks / Trade-offs

- **Cascade termination** → Mitigation: `IEngineObserver` halt mechanism is wired into the loop. Tests that create trigger chains must be written carefully to ensure they terminate.
- **High-water mark correctness for trigger chains** → Mitigation: A dedicated test verifies that a trigger-fired block's events are seen in the NEXT cascade batch, not the current one.
- **`RunStateBasedRules` not tested in this change** → Accepted risk. Follow-up change adds dedicated SBR tests. The fixpoint loop is simple enough to review-verify without tests initially.
- **`TriggerFireCount` increment ordering** → D8 specifies increment BEFORE `ExecuteBlock` so `CheckLifetimes` sees the updated count. This is easy to get wrong. The implementer must follow the exact sequence in D8's `FireTrigger` pseudocode.

## Migration Plan

No persistent state changes. All changes are engine-internal classes. Layer 2 test 6.7 is updated in-place to use `ActionResolver` instead of manual simulation. New tests are added in a new `TriggerResolution/` directory.

## Open Questions

None. D7 and D8 fully specify the trigger and state-based rule semantics. The implementer should follow the pseudocode in those decisions directly.
