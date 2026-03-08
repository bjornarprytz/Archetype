## Why

Tier 2 execution (block executor, keyword evaluator, built-in dispatch) is complete, but triggers never actually fire between actions — Layer 2 test 6.7 manually simulates what an `ActionResolver`/`TriggerResolver` would do. Without a real trigger resolver, the engine cannot evaluate any game rule that reacts to events (which is nearly every interesting game rule). This is the highest-priority unblocked item on the critical path to `GameSession`.

## What Changes

- Introduce `TriggerResolver`: collects pending trigger firings from `ActiveStaticEffects` after each action, orders them, and dispatches the resulting effect blocks.
- Introduce `ActionResolver`: orchestrates the full action lifecycle — execute the player's action, run `LifetimeChecker`, collect and resolve triggers, repeat until quiescence.
- Replace the manual trigger simulation in Layer 2 test 6.7 with the real `ActionResolver`.
- Add tests covering trigger ordering, multiple triggers on the same event, and trigger chains (trigger fires → event → another trigger).

## Capabilities

### New Capabilities

- `trigger-resolution`: Collecting, ordering, and firing triggered effects after each action; quiescence detection for trigger chains.
- `action-lifecycle`: The full per-action loop: player action → effect block → lifetime check → trigger collection → trigger firing → repeat until quiescent.

### Modified Capabilities

- `zone-movement`: Layer 2 test 6.7 currently manually simulates trigger firing. Once `ActionResolver` exists, that test should exercise the real path. The spec requirements are unchanged; only the test implementation changes.

## Impact

- **New classes**: `TriggerResolver` and `ActionResolver` in `Archetype.Engine` (Tier 3).
- **`EventLog`**: `TriggerResolver` reads `ThisAction` scope (already implemented) to find new events since the last high-water mark. `StaticEffect.TriggerHighWaterMark` and `TriggerFireCount` fields (already defined) are updated by `TriggerResolver`.
- **`LifetimeChecker`**: Called by `ActionResolver` after each action; interface unchanged.
- **`ExecutionContext`**: `ActionResolver` uses `CreateChildActionContext` (already implemented) to spawn execution contexts for trigger-fired effect blocks.
- **Tests**: `MoveCard/MoveCardLayer2Tests.cs` test 6.7 updated. New `TriggerResolution/` test file added.
- **Non-goals**: Trigger ordering with player choice (the `TriggerOrderPrompt` path) is defined in the domain model and architecture but is not a goal of this change — ordering will use declaration order initially.

## Non-goals

- Player-controlled trigger ordering (`TriggerOrderPrompt`) — will default to declaration order for now.
- State-based rule runner (separate change).
- `GameSession` / `GameSessionBuilder` — these depend on this change completing first.
- Text renderer — independent; separate change.

## Owners

- **Implementer** — writes `TriggerResolver`, `ActionResolver`, and tests.
- **Reviewer** — verifies correctness of quiescence loop, event scoping, and that no mid-block triggers can fire.
