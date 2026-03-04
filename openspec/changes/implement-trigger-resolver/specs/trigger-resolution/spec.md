## ADDED Requirements

### Requirement: Trigger fires when its condition is satisfied
The engine SHALL fire a static effect's triggered block when, after an action's primary block completes, at least one event in the declared `TriggerScope` matches the trigger's `EventKeyword` and satisfies its `Condition` (if present), and that event's `SequenceNumber` is greater than the effect's current `TriggerHighWaterMark`.

#### Scenario: Single trigger fires on matching event
- **WHEN** a static effect has a trigger with `EventKeyword: "move-card"` and no `Condition`
- **AND** a primary block executes `move-card`
- **THEN** after the block completes, the trigger's `FiredBlock` is executed as a new child action
- **THEN** the `TriggerFireCount` on the static effect is incremented by 1

#### Scenario: Trigger does not fire when condition is false
- **WHEN** a static effect has a trigger with `EventKeyword: "move-card"` and a `Condition` that evaluates to false for the candidate event
- **AND** a primary block executes `move-card`
- **THEN** the trigger's `FiredBlock` is NOT executed
- **THEN** `TriggerFireCount` is NOT incremented

#### Scenario: Trigger does not fire when no matching events exist
- **WHEN** a static effect has a trigger with `EventKeyword: "draw-card"`
- **AND** a primary block executes only `move-card`
- **THEN** the trigger's `FiredBlock` is NOT executed

### Requirement: Each event fires a trigger at most once via high-water mark
The engine SHALL advance a static effect's `TriggerHighWaterMark` past every candidate event evaluated in a collection pass (whether matched or not), ensuring each event fires the trigger at most once across all cascade batches.

#### Scenario: High-water mark prevents double-firing
- **WHEN** a static effect trigger fires on event E1 (SequenceNumber = 5)
- **AND** the cascade loop runs a second collection pass
- **THEN** event E1 (SequenceNumber = 5) is NOT re-evaluated as a candidate
- **THEN** the trigger does NOT fire again for E1

#### Scenario: Events from trigger-fired blocks are visible in next cascade batch
- **WHEN** a trigger fires and its `FiredBlock` produces a new event E2
- **THEN** E2's `SequenceNumber` is greater than the high-water mark set in the current batch
- **THEN** E2 is visible as a candidate in the next cascade batch, enabling trigger chains

### Requirement: Triggered block receives the triggering event as a reserved binding
The engine SHALL pre-populate the fired block's variable bindings with the triggering `GameEvent` under the reserved name `trigger_event` (typed `EventRef`). Declared `EventBindings` on the trigger SHALL also be pre-populated as named bindings.

#### Scenario: trigger_event binding is available in fired block
- **WHEN** a trigger fires on event E with `BoundArgs: { card: X, origin: Y, destination: Z }`
- **THEN** the fired block's bindings contain `trigger_event` set to `EventRef(E)`
- **THEN** `event-arg(trigger_event, "card")` resolves to X within the block

#### Scenario: EventBindings provide convenient named access
- **WHEN** a trigger declares `EventBindings: [{ EventArgName: "card", BlockVarName: "moved_card" }]`
- **AND** the trigger fires on event E with `BoundArgs: { card: X }`
- **THEN** the fired block's bindings contain `moved_card = X` in addition to `trigger_event`

### Requirement: Multiple triggers are ordered before firing
When multiple triggers are satisfied in the same cascade batch, the engine SHALL order them according to `GameDefinition.TriggerResolutionOrder` before executing any triggered block. The default order is `OldestFirst` (lowest `StaticEffectId` first; within a single effect, lowest `SequenceNumber` first).

#### Scenario: OldestFirst ordering fires older effects before newer ones
- **WHEN** two static effects S1 (older `StaticEffectId`) and S2 (newer `StaticEffectId`) both have triggers satisfied by the same event
- **AND** `TriggerResolutionOrder` is `OldestFirst`
- **THEN** S1's triggered block executes before S2's triggered block

#### Scenario: Trigger chain — second trigger fires in a subsequent cascade batch
- **WHEN** trigger T1 fires and its block produces an event that satisfies trigger T2
- **THEN** T2 does NOT fire in the same cascade batch as T1
- **THEN** after T1's block completes, the cascade loop runs another collection pass and T2 fires in the next batch

### Requirement: TriggerFireCount is incremented before block execution
The engine SHALL increment `TriggerFireCount` on a static effect before executing its triggered block, so that the immediately-following `CheckLifetimes` call can evaluate any `TriggerCount` lifetime condition correctly.

#### Scenario: TriggerCount lifetime expires after correct number of firings
- **WHEN** a static effect has `LifetimeSpec: TriggerCount(1)` and its trigger fires once
- **THEN** `TriggerFireCount` is 1 before `CheckLifetimes` runs
- **THEN** `CheckLifetimes` detects the `TriggerCount(1)` condition is satisfied and expires the effect
- **THEN** the static effect is no longer active after that firing
