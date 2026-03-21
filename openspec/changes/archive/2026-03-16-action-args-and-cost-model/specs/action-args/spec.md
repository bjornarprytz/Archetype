## ADDED Requirements

### Requirement: PlayerAction carries cost arguments and target arguments
`PlayCard` and `ActivateAbility` subtypes of `PlayerAction` SHALL carry `CostArgs: IReadOnlyDictionary<string, object>` (player-supplied values for cost parameters, keyed by `ParameterDecl.Name`) and `Targets: IReadOnlyList<AtomId>` (player-selected target atoms). `Pass` carries neither.

#### Scenario: PlayCard with a discard cost supplies the card to discard
- **WHEN** a card has a `CostDef` with parameter `discarded-card: Card`
- **THEN** the host supplies `CostArgs = { "discarded-card": <atomId> }` in the `PlayCard` action
- **AND** the engine binds that value when executing the cost `Body`

#### Scenario: PlayCard with no cost supplies empty CostArgs
- **WHEN** a `CardDefinition` has an empty `Cost` list
- **THEN** `CostArgs` is an empty dictionary and the engine performs no cost execution

### Requirement: CostArgs are bound into the cost body execution context by parameter name
During cost body execution (both real and clone-based validation), each `CostDef`'s `CostArgs` entries SHALL be bound into the `ExecutionContext` by `ParameterDecl.Name`, alongside `source` (the card or ability atom). Missing required cost args SHALL cause `EngineException` to be raised during execution or validation.

#### Scenario: CostArgs correctly bound during body execution
- **WHEN** a `CostDef.Body` references a parameter `discarded-card`
- **AND** `PlayerAction.CostArgs` contains `{ "discarded-card": <atomId> }`
- **THEN** the parameter resolves to the specified atom during body execution

#### Scenario: Missing required cost arg raises EngineException
- **WHEN** a `CostDef` declares a parameter `discarded-card`
- **AND** `PlayerAction.CostArgs` does not contain `"discarded-card"`
- **THEN** `EngineException` is raised and `ValidationResult.IsValid` is false

### Requirement: ValidationResult describes overall affordability and per-cost display text
`ValidateActionArgs` SHALL return a `ValidationResult` containing `IsValid: bool` and `CostTexts: IReadOnlyList<string>` (one resolved text per `CostDef`, always populated). The `CostTexts` are resolved from each `CostDef.TextTemplate` using the active locale (or the raw template if no locale is provided).

#### Scenario: All costs pass — IsValid true
- **WHEN** the combined cost block executes on the clone without exception
- **THEN** `ValidationResult.IsValid` is true

#### Scenario: Any cost fails — IsValid false
- **WHEN** the combined cost block raises `EngineException` on the clone
- **THEN** `ValidationResult.IsValid` is false

#### Scenario: CostTexts always populated regardless of outcome
- **WHEN** validation fails
- **THEN** `ValidationResult.CostTexts` still contains all resolved cost text strings for display
