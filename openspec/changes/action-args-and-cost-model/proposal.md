## Why

`ComputeAvailableActions` hard-codes an ownership assumption that the engine has no business making, and there is no model for action costs or target arguments — meaning hosts cannot validate player choices before committing to them, and the engine cannot correctly sequence cost payment ahead of effect execution.

## What Changes

- Remove ownership as a hard-coded filter from `ComputeAvailableActions`; zone membership and `ActivationCondition` fully express playability.
- Introduce `CostDef` — a new type modelling a cost as a state-mutating payment with an independent pure evaluation function, optional player-provided parameters, and localized text.
- Add `Cost: CostDef[]` to `CardDefinition` (PlayCard) and `NamedEffectBlockDef` (abilities); costs are sequenced and paid before the main effect executes.
- Extend `PlayerAction` subtypes to carry cost arguments and target arguments supplied by the player.
- Add `ValidateActionArgs: Func<PlayerAction, ValidationResult>` callback inside `AvailableActions`, allowing the host to validate cost affordability (including multi-cost combination) and target legality before committing to an action — callable as many times as needed.
- Sequential cost validation uses a lightweight `GameState` clone: each cost's `EvaluationFunction` is evaluated against the state after all prior costs' bodies have been applied to the clone, catching conflicts that individual evaluation functions cannot detect.

**BREAKING**: `AvailableActions` gains a new required field. `PlayerAction` subtypes gain cost-arg and target-arg fields. `ComputeAvailableActions` iteration basis changes from "owned cards" to "cards in playable zones (or all cards)".

## Capabilities

### New Capabilities

- `cost-model`: `CostDef` type; `Cost: CostDef[]` on `CardDefinition` and `NamedEffectBlockDef`; cost sequencing and payment execution ahead of main effect; sequential clone-based multi-cost validation.
- `action-args`: Player-supplied cost arguments and target arguments on `PlayerAction` subtypes; `ValidateActionArgs` callback in `AvailableActions`; `ValidationResult` type with per-cost text and affordability flags.

### Modified Capabilities

- `available-actions-contract`: Remove ownership filter; iteration basis shifts to zone-based (or all-cards); note outcome telegraphing as a deferred future capability.

## Non-goals

- **Outcome telegraphing / full action dry-run**: Previewing triggered effects and SBR outcomes for the player is a separate, future capability. This change establishes cost validation only.
- **Target enumeration**: `ValidTargets` remains deferred. The host is responsible for enumerating valid targets; the engine validates whether the host-supplied targets are legal (this is the `ValidateActionArgs` contract).
- **Cost pre-flight for ability zone restrictions**: Ability zone restrictions continue to be expressed via `ActivationCondition` on `NamedEffectBlockDef`.

## Impact

- **`Archetype.Core`**: New `CostDef` record; `CardDefinition` and `NamedEffectBlockDef` gain `Cost: CostDef[]`; `PlayerAction` subtypes (`PlayCard`, `ActivateAbility`) gain cost-arg and target-arg fields; `AvailableActions` gains `ValidateActionArgs` callback; new `ValidationResult` type.
- **`Archetype.Engine`**: `ComputeAvailableActions` rewrite (ownership filter removal, `ValidateActionArgs` construction); sequential clone-based cost evaluation logic; cost payment sequencing in action execution path.
- **`Archetype.Build`**: `CostDef` builder / factory helpers.
- **Tests**: Existing `ComputeAvailableActions` tests updated for new iteration basis; new tests for `CostDef` validation, multi-cost combination, and `ValidateActionArgs` callback.

**Owner**: Technical Architect (design), Implementer (code), Reviewer (verification).
