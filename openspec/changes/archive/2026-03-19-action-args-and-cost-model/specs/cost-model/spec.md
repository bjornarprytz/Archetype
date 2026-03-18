## ADDED Requirements

### Requirement: CostDef is a first-class type with a mutating body and optional parameters
The engine SHALL define `CostDef` as a distinct type from `EffectBlockDef`. A `CostDef` SHALL have a `Body` (`EffectBlockDef` — the state-mutating payment), `Parameters` (player-provided args, e.g. which card to discard), and an optional `TextTemplate` for localized cost description. A `CostDef` SHALL NOT itself carry a cost (no recursive cost chains). There is no separate evaluation function — affordability is signalled inside the body via the `assert` built-in.

#### Scenario: Cost body executes and mutates state when paid
- **WHEN** an action with a cost is executed by `ActionResolver`
- **THEN** each `CostDef.Body` runs as part of a combined `EffectBlockDef` and mutates `GameState` (e.g. decrements energy, moves a card to discard)

#### Scenario: Cost body with assert raises EngineException when condition false
- **WHEN** a `CostDef.Body` contains `assert(gte(energy(source), 3))` and the player has 2 energy
- **THEN** `assert` raises `EngineException`
- **AND** the action fails

### Requirement: assert is a built-in mutation keyword that guards cost bodies
The engine SHALL provide an `assert(condition: Boolean) → void` built-in mutation keyword. If `condition` evaluates to true, `assert` completes silently without appending any event to the log. If `condition` evaluates to false, `assert` raises `EngineException`. Game creators use `assert` inside `CostDef.Body` to enforce affordability conditions before payment steps execute.

#### Scenario: assert with true condition — silent success, no event logged
- **WHEN** `assert(gte(energy(source), 3))` is evaluated and the player has 5 energy
- **THEN** execution continues to the next step with no event appended

#### Scenario: assert with false condition — EngineException raised
- **WHEN** `assert(gte(energy(source), 3))` is evaluated and the player has 1 energy
- **THEN** `EngineException` is raised and execution does not proceed to subsequent steps

#### Scenario: Game creator composes a reusable cost keyword using assert
- **WHEN** a game creator defines `energy_cost(x) → [assert(gte(energy(source), x)), modify-accumulator(source, "energy", -x)]`
- **THEN** this keyword is usable as a step in any `CostDef.Body` and correctly enforces and pays the energy cost

### Requirement: CardDefinition and NamedEffectBlockDef carry CostDef lists
`CardDefinition` SHALL have a `Cost: IReadOnlyList<CostDef>` field (empty = no cost). `NamedEffectBlockDef` SHALL replace its existing `Cost: EffectBlockDef?` field with `Cost: IReadOnlyList<CostDef>`. An empty list requires no payment.

#### Scenario: Card with no cost is always affordable
- **WHEN** `CardDefinition.Cost` is an empty list
- **THEN** `ValidationResult.IsValid` is true and no payment executes

#### Scenario: Ability with multiple costs declares them as a list
- **WHEN** a `NamedEffectBlockDef` declares two `CostDef` entries
- **THEN** their bodies are combined into a single block, validated together, and paid before the ability body executes

### Requirement: Costs are paid as a single combined block before the main effect
When `ActionResolver` executes a `PlayCard` or `ActivateAbility` action, it SHALL combine all `CostDef.Body` blocks in declaration order into a single composite `EffectBlockDef` and execute it within the same action scope before executing the main effect. Cost events SHALL appear in `events.this_action` alongside effect events.

#### Scenario: Cost body events appear in the same action scope as effect events
- **WHEN** a card with a cost that moves a card to discard is played
- **THEN** the move-card event (from the cost body) and the play effect events share the same action scope
- **AND** both are visible in `events.this_action`

#### Scenario: Cost failure propagates as EngineException
- **WHEN** a cost body's `assert` raises `EngineException`
- **THEN** the exception propagates to the caller
- **AND** it is the host's responsibility to call `ValidateActionArgs` before submitting the action

### Requirement: Combined cost block validation uses a lightweight state clone
`ValidateActionArgs` SHALL combine all `CostDef.Body` blocks into a single composite `EffectBlockDef` and execute it against a lightweight clone of `GameState`. If the block completes without `EngineException`, `ValidationResult.IsValid` is true. If `EngineException` is raised, `IsValid` is false. The clone covers mutable atom state (accumulators, zone membership, condition presence) but excludes the event log, active static effects, and contribution registries. The real `GameState` SHALL NOT be mutated by `ValidateActionArgs`.

#### Scenario: Two costs combined — conflict detected via clone
- **WHEN** a card has two costs each asserting and consuming 2 energy, and the player has only 3 energy
- **THEN** the combined block: first cost deducts 2 (clone now has 1 energy); second cost's assert fails (1 < 2); `EngineException` raised
- **AND** `ValidationResult.IsValid` is false

#### Scenario: All costs pass — IsValid true, GameState unchanged
- **WHEN** all combined cost steps complete without exception on the clone
- **THEN** `ValidationResult.IsValid` is true and real `GameState` is unchanged

#### Scenario: Clone validation matches real execution semantics
- **WHEN** `ValidateActionArgs` is called and the combined cost block succeeds on the clone
- **AND** the same action is then submitted and executed by `ActionResolver`
- **THEN** both use the same single-block execution path and produce consistent outcomes

### Requirement: CostDef.TextTemplate resolves to localized cost text in ValidationResult
Each `CostDef` MAY declare a `TextTemplate`. `ValidationResult` SHALL include a `CostTexts` list with one resolved string per `CostDef`, always populated regardless of whether validation passed or failed.

#### Scenario: Cost text is present even when validation fails
- **WHEN** the combined cost block fails
- **THEN** `ValidationResult.CostTexts` still contains all resolved cost descriptions for the host to display

#### Scenario: CostTexts count matches CostDef list length
- **WHEN** an action has N costs
- **THEN** `ValidationResult.CostTexts` has exactly N entries
