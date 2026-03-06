## 1. TriggerEvaluationContext and Collection

- [x] 1.1 Create `TriggerEvaluationContext` record in `Archetype.Engine` — carries event param bindings (from `EventParams` mapping + candidate `BoundArgs`), read-only `GameState`, and `TriggerScope`
- [x] 1.2 Implement `CollectSatisfiedTriggers(GameState, EventLog) → List<TriggerFiring>` in `TriggerResolver` — scans active static effects with triggers, filters candidates by `EventKeyword` and `SequenceNumber > TriggerHighWaterMark`, evaluates `Condition` via `KeywordEvaluator.EvaluateCondition` with the `TriggerEvaluationContext` bindings, and advances `TriggerHighWaterMark` past all seen candidates
- [x] 1.3 Implement `OrderTriggerFirings(List<TriggerFiring>, TriggerResolutionOrder) → List<TriggerFiring>` — `OldestFirst`: sort by `(StaticEffectId ASC, SequenceNumber ASC)`; `OldestLast`: sort by `(StaticEffectId DESC, SequenceNumber ASC)`; `PromptPlayer`: fall back to `OldestFirst` with a no-op warning (deferred per design Decision 5)

## 2. FireTrigger

- [x] 2.1 Implement `FireTrigger(TriggerFiring, ExecutionContext) → Task` in `TriggerResolver` — populate `bindings` with `trigger_event = EventRef(e)` and any `EventBindings` convenience names; increment `se.TriggerFireCount` BEFORE calling `ExecuteBlock`; call `ExecuteBlock(se.Trigger.FiredBlock, ctx)` using `ExecutionContext.CreateChildActionContext(bindings)`

## 3. RunStateBasedRules

- [x] 3.1 Implement `RunStateBasedRules(ExecutionContext) → Task` in `ActionResolver` — fixpoint loop: evaluate all `GameDefinition.StateBasedRules` conditions via `KeywordEvaluator.EvaluateCondition`; collect all satisfied rules; if empty, return; execute each satisfied rule's block via `BlockExecutor.ExecuteBlock` (which calls `CheckLifetimes`); repeat until no rules trigger in a pass

## 4. ActionResolver

- [x] 4.1 Create `ActionResolver` class in `Archetype.Engine` with constructor accepting `IReadOnlyDictionary<string, IPlayerStrategy>`, `IRandomSource`, `IEngineObserver?`, `TriggerResolver`, `GameDefinition`
- [x] 4.2 Implement `ResolveAction(PlayerAction, GameState, EventLog) → Task` — full post-action sequence per D7: (1) `ExecuteBlock` primary block → `CheckLifetimes`; (2) `RunStateBasedRules`; (3) cascade loop: increment `triggerBatchCount`, call `IEngineObserver?.OnTriggerCascade(triggerBatchCount)` → break on `Halt`; collect triggers → break if empty; fire each trigger → `CheckLifetimes` → `RunStateBasedRules`; repeat cascade

## 5. Update Existing Tests

- [x] 5.1 Refactor Layer 2 test 6.7 (`CompositeKeyword_CallingMoveCard_ProducesNestedEventTree_AndTriggerFires`) to use the real `ActionResolver` instead of manually simulating trigger resolution — the test must pass with the identical assertions it has now

## 6. New TriggerResolution Tests

- [x] 6.1 `TriggerFires_WhenEventMatchesKeywordAndNoCondition` — primary block executes `move-card`; assert trigger's `FiredBlock` executes and `TriggerFireCount == 1`
- [x] 6.2 `TriggerDoesNotFire_WhenConditionIsFalse` — trigger has a condition that is false for the candidate event; assert `FiredBlock` does not execute
- [x] 6.3 `TriggerDoesNotFire_WhenNoMatchingEvents` — trigger listens for `draw-card`; primary block only executes `move-card`; assert `FiredBlock` does not execute
- [x] 6.4 `HighWaterMark_PreventsTriggerDouble_Firing` — trigger fires once on event E1; cascade runs a second pass; assert trigger does not fire again for E1
- [x] 6.5 `TriggerChain_SecondTriggerFiresInNextBatch` — trigger T1 fires and its block produces event E2 that satisfies trigger T2; assert T2 fires in the next cascade batch (after T1 completes), not the same batch
- [x] 6.6 `TriggerFiring_FireCount_CausesExpiry_ViaTriggerCount` — static effect with `TriggerCount(1)` fires once; assert effect is no longer active after the action resolves
- [x] 6.7 `OldestFirst_OrdersMultipleTriggers` — two static effects both triggered; assert the one with the lower `StaticEffectId` fires first
- [x] 6.8 `TriggerEvent_Binding_IsAvailableInFiredBlock` — triggered block reads `trigger_event` and uses `event-arg` to extract a value; assert the correct value is available
- [x] 6.9 `NullObserver_DoesNotHaltCascade` — `ActionResolver` constructed with `null` observer; trigger chain runs to quiescence without error

## 7. Review

- [x] 7.1 Reviewer: verify `TriggerFireCount` is incremented before `ExecuteBlock` in `FireTrigger` (D8 requirement — easy to get wrong)
- [x] 7.2 Reviewer: verify `TriggerHighWaterMark` advances past all candidates (matched or not) in a single collection pass
- [x] 7.3 Reviewer: verify `RunStateBasedRules` re-evaluates ALL conditions after each full pass (not incrementally)
- [x] 7.4 Reviewer: verify all new tests pass and Layer 2 test 6.7 still passes with real `ActionResolver`
