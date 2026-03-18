## ADDED Requirements

### Requirement: move-card primitive exists
The engine SHALL provide a built-in mutation primitive `move-card(card: Card, destination: Zone) → void` that changes the zone a card occupies.

#### Scenario: Card moves to a different zone
- **WHEN** `move-card(card, destination)` is invoked where `destination` differs from the card's current zone
- **THEN** the card's zone membership is updated to `destination`
- **THEN** the card is no longer a member of its previous zone

#### Scenario: Self-move is valid
- **WHEN** `move-card(card, destination)` is invoked where `destination` equals the card's current zone
- **THEN** the invocation completes without error and the card remains in the same zone

#### Scenario: Invalid destination raises a runtime error
- **WHEN** `move-card(card, destination)` is invoked and `destination` does not correspond to an active zone atom in the current game state
- **THEN** the engine raises a runtime `EngineException` and does not mutate game state

### Requirement: move-card logs an event with origin and destination
The engine SHALL append a `move-card` event to the event log whenever `move-card` executes. The event SHALL include `card`, `origin` (the zone the card occupied before the move), and `destination` as bound arguments.

#### Scenario: Event captures pre-move origin zone
- **WHEN** `move-card(card, destination)` executes and the card was in zone A
- **THEN** the logged event contains `{ card: <card-id>, origin: <zone-A-id>, destination: <destination-id> }`
- **THEN** `origin` reflects the zone the card was in at the moment of invocation, regardless of any subsequent moves in the same block

#### Scenario: Self-move event is still logged
- **WHEN** `move-card(card, destination)` executes with `origin == destination`
- **THEN** a `move-card` event is appended with `origin` and `destination` equal to the same zone id

### Requirement: Zone-based lifetime conditions re-evaluate after move
A static effect whose lifetime includes a `WhileCondition` referencing zone membership SHALL be evaluated by the engine's post-block `CheckLifetimes` sweep after any block that contains `move-card`.

#### Scenario: Static effect expires when card leaves its while-condition zone
- **WHEN** a card has an active static effect with `WhileCondition: in-zone(source, zone-X)`
- **THEN** after a block executes `move-card(card, zone-Y)`, the effect expires on the next `CheckLifetimes` call
- **THEN** all contributions owned by that static effect are removed

#### Scenario: Dormant static effect re-activates when card enters its while-condition zone
- **WHEN** a card has a dormant declarative static effect with `WhileCondition: in-zone(source, zone-X)`
- **THEN** after a block executes `move-card(card, zone-X)`, the effect instantiates on the next `CheckLifetimes` Phase 2 sweep
- **THEN** the new instance has `TriggerFireCount = 0` and `TriggerHighWaterMark = 0`

### Requirement: move-card is composable as a building block for game-creator keywords
Game creators SHALL be able to define composite mutation keywords (e.g., `draw-card`, `discard`, `play-to-battlefield`) that call `move-card` as their underlying zone-transition mechanism.

#### Scenario: Game creator defines draw-card using move-card
- **WHEN** a game creator defines `draw-card(player)` as a composite that calls `move-card(top-of-deck, hand-zone-of(player))`
- **THEN** executing `draw-card(player)` produces a `move-card` event nested under the `draw-card` event in the event tree
- **THEN** a trigger watching for `EventKeyword: "draw-card"` fires; a trigger watching for `EventKeyword: "move-card"` also fires, because both events exist in the tree

### Requirement: move-card is available in BuiltInKeywords and Kw factory
`move-card` SHALL be registered in `BuiltInKeywords` in `Archetype.Core` with its full `ParameterDecl[]` signature. A corresponding `Kw.MoveCard(card, destination)` shorthand SHALL exist in `Archetype.Build`.

#### Scenario: Authoring validation accepts move-card references
- **WHEN** a game creator's keyword definition contains an `Invocation("move-card", ...)` node
- **THEN** the `GameDefinitionBuilder.Build()` type-checker resolves it against `BuiltInKeywords` without error

#### Scenario: Kw.MoveCard produces a valid Invocation node
- **WHEN** a game creator calls `Kw.MoveCard(Kw.Param("card"), Kw.Param("dest"))`
- **THEN** an `Invocation("move-card", [ParameterRef("card"), ParameterRef("dest")])` node is produced
