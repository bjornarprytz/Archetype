## ADDED Requirements

### Requirement: Zone filtering for playable cards
`ComputeAvailableActions` SHALL only include `PlayCard` actions for cards whose current `ZoneId` matches a zone designated as the active player's playable zone in the `GameDefinition`. Cards in other zones (discard, in-play, opponent's hand, etc.) SHALL NOT be included.

#### Scenario: Card in hand is playable
- **WHEN** the active player owns a card whose `ZoneId` matches their designated hand zone
- **THEN** a `PlayCard` action for that card is included in the available actions list

#### Scenario: Card not in hand is not playable
- **WHEN** the active player owns a card whose `ZoneId` does not match their designated hand zone (e.g., it is in the discard or in play)
- **THEN** no `PlayCard` action for that card is included in the available actions list

#### Scenario: Opponent's cards are not playable
- **WHEN** a card is owned by a player other than the active player
- **THEN** no `PlayCard` action for that card is included in the available actions list, regardless of its zone

### Requirement: Activation condition filtering
If a `CardDefinition` declares an `ActivationCondition` keyword node, `ComputeAvailableActions` SHALL evaluate that condition in pure mode (no state mutation, no event logging) and exclude the card's `PlayCard` action if the condition evaluates to false.

#### Scenario: Card with true activation condition is playable
- **WHEN** a card is in the active player's hand zone and its `ActivationCondition` evaluates to true
- **THEN** a `PlayCard` action for that card is included

#### Scenario: Card with false activation condition is not playable
- **WHEN** a card is in the active player's hand zone and its `ActivationCondition` evaluates to false
- **THEN** no `PlayCard` action for that card is included

#### Scenario: Card without activation condition is always playable (if in hand)
- **WHEN** a card is in the active player's hand zone and has no `ActivationCondition`
- **THEN** a `PlayCard` action for that card is included unconditionally

### Requirement: Pass is always available
`ComputeAvailableActions` SHALL always include a `Pass` action, regardless of any other conditions.

#### Scenario: No cards in hand
- **WHEN** the active player has no cards in their hand zone
- **THEN** the available actions list contains exactly one action: `Pass`

#### Scenario: Cards in hand
- **WHEN** the active player has cards in their hand zone
- **THEN** the available actions list contains a `Pass` action in addition to any `PlayCard` actions

### Requirement: Ability activation filtering
For each `AdditionalEffect` declared on a `CardDefinition`, a `ActivateAbility` action SHALL be included only if the card is in a zone where ability activation is permitted (as declared by the `GameDefinition`) and any declared activation condition for that ability evaluates to true.

#### Scenario: Ability with satisfied condition
- **WHEN** a card is in an ability-activatable zone and its ability's activation condition evaluates to true
- **THEN** an `ActivateAbility` action for that ability is included

#### Scenario: Ability with unsatisfied condition
- **WHEN** a card's ability activation condition evaluates to false
- **THEN** no `ActivateAbility` action for that ability is included
