## MODIFIED Requirements

### Requirement: Zone filtering for playable cards
`ComputeAvailableActions` SHALL include `PlayCard` actions for all cards whose current `ZoneId` matches a zone whose definition name appears in `GameDefinition.PlayableZoneNames`, regardless of card ownership. If `PlayableZoneNames` is null or empty, all cards are zone-eligible candidates. Further playability restrictions (including ownership) SHALL be expressed via `CardDefinition.ActivationCondition`.

#### Scenario: Card in a playable zone is a candidate regardless of owner
- **WHEN** a card's current zone's definition name is in `PlayableZoneNames`
- **THEN** that card is a `PlayCard` candidate, irrespective of which player owns it

#### Scenario: Card not in a playable zone is excluded
- **WHEN** a card's current zone's definition name is NOT in `PlayableZoneNames`
- **AND** `PlayableZoneNames` is non-empty
- **THEN** no `PlayCard` action for that card is included

#### Scenario: No zone filter — all cards are candidates
- **WHEN** `PlayableZoneNames` is null or empty
- **THEN** every card in `GameState` is a zone-eligible `PlayCard` candidate (subject to `ActivationCondition`)

#### Scenario: Ownership restriction expressed via ActivationCondition
- **WHEN** a `CardDefinition` declares `ActivationCondition: owned-by(source, active-player)`
- **AND** a card of that definition is in a playable zone but owned by another player
- **THEN** the `ActivationCondition` evaluates to false and the card is excluded from `PlayCard` candidates

### Requirement: Ability activation filtering
For each `AdditionalEffect` declared on a `CardDefinition`, an `ActivateAbility` action SHALL be included for every card of that definition currently in `GameState`, regardless of zone or ownership. Zone or ownership restrictions on ability activation SHALL be expressed via `ActivationCondition` on the `NamedEffectBlockDef`.

#### Scenario: Ability candidate is not zone-restricted by default
- **WHEN** a card has an ability with no `ActivationCondition`
- **AND** the card is in any zone
- **THEN** an `ActivateAbility` action for that ability is included

#### Scenario: Ability with satisfied condition
- **WHEN** a card's ability `ActivationCondition` evaluates to true
- **THEN** an `ActivateAbility` action for that ability is included

#### Scenario: Ability with unsatisfied condition
- **WHEN** a card's ability `ActivationCondition` evaluates to false
- **THEN** no `ActivateAbility` action for that ability is included

## REMOVED Requirements

### Requirement: Opponent's cards are not playable
**Reason**: Ownership is no longer a hard-coded engine constraint. The engine has no business presuming ownership rules — games that require ownership restrictions express them via `ActivationCondition`.
**Migration**: Add `ActivationCondition: Kw.OwnedByActivePlayer()` (or an equivalent keyword tree) to any `CardDefinition` or `NamedEffectBlockDef` that requires the active player to own the card before playing or activating it.

## ADDED Requirements

### Requirement: ValidateActionArgs callback is provided in AvailableActions
`ComputeAvailableActions` SHALL populate `AvailableActions.ValidateActionArgs` with a synchronous `Func<PlayerAction, ValidationResult>` delegate. The host MAY call this delegate as many times as needed before returning a `PlayerAction` from `IPlayerStrategy.SelectActionAsync`. The delegate SHALL NOT mutate `GameState`.

#### Scenario: Host validates a chosen action before returning it
- **WHEN** the host calls `availableActions.ValidateActionArgs(chosenAction)`
- **THEN** the engine evaluates the action's costs sequentially against a state clone and returns a `ValidationResult`
- **AND** `GameState` is unchanged after the call

#### Scenario: ValidateActionArgs called multiple times is safe
- **WHEN** the host calls `ValidateActionArgs` on the same or different actions multiple times
- **THEN** each call produces an independent result with no side effects
