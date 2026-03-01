# Archetype — Domain Model

## Status
**Complete. Signed off 2026-03-01.**

This document is the canonical vocabulary for the Archetype card game engine. It is the source of truth for the architect and implementer roles. It is implementation-agnostic: no data structures, programming languages, or frameworks are specified here.

All seven open items from the requirements have been resolved and are incorporated below.

---

## Resolved Open Items

| # | Item | Resolution |
|---|------|------------|
| 1 | Effect terminology overloading | "Effect" in *activated effect* / *static effect* carries the broad sense ("outcome-producing construct"). The keyword subtype that mutates state is called a **mutation keyword**. The subtype that reads state is called a **property keyword**. "Effect keyword" is not used. |
| 2 | Owner vs. controller | **Owner** is a first-class, immutable engine concept. **Controller** is not an engine concept; game creators who need it model it via conditions/tags. |
| 3 | Built-in keyword signatures | Defined precisely in §9. |
| 4 | Zone grouping mechanism | Not a first-class concept. Game creators compose `in-zone` and logical operators into named boolean property keywords (e.g. `is-in-play`). No separate zone-group entity exists in the engine. |
| 5 | Lifetime composition model | A lifetime specification is a set of zero or more primitive conditions combined as OR. Zero conditions = permanent. The three primitive types are turn-timer, trigger-count, and while-condition. Any number of conditions may be OR'd. |
| 6 | Mid-effect prompt binding | A prompt binds the player's choice to a named variable in the block's local execution scope. The block pauses; the event log is unmodified; the player responds; the variable is bound; the block resumes from the next keyword. |
| 7 | State-based rule convergence | The engine makes no attempt to detect or terminate infinite loops. Convergence is entirely the game creator's responsibility. |

---

## 1. Keyword System

### 1.1 Keyword

**Definition.** A keyword is a named, parameterized function that is the primitive unit of expression in the engine. Every rule, cost, condition check, and state change is ultimately expressed as a keyword invocation or a composition of keyword invocations.

**Dual-use invariant.** Every keyword definition must support two modes without a separate definition for each:
1. **Execution** — resolving game state changes or reading values at runtime.
2. **Text rendering** — generating human-readable card text. The full composition tree of a keyword must be available at runtime so the game layer can decide what level of detail to present to the player.

**Subtypes.** Keywords are partitioned into exactly two mutually exclusive subtypes:
- **Mutation keyword** — changes game state when invoked (§1.2).
- **Property keyword** — queries and returns a value; never changes state (§1.3).

**Composition.** A keyword of either subtype may invoke other keywords of either subtype as part of its definition. Composed keywords are called *composite*; keywords that invoke engine primitives directly are called *primitive*. All game-creator-defined keywords are ultimately composed on top of the engine's built-in primitives (§9).

**Invariants.**
- A keyword has a fixed name and a fixed parameter list with declared parameter types.
- A keyword definition is immutable after authoring (it is design-time data, not runtime state).
- A composite keyword's composition tree is finite and acyclic (no keyword may directly or transitively invoke itself).

---

### 1.2 Mutation Keyword

**Definition.** A mutation keyword is a keyword that changes game state when invoked. Mutation keywords are the only mechanism by which game state changes.

**Varieties.**

- **Direct mutation** — immediately changes a state value upon invocation (e.g. adds N to an accumulator, applies a modifier with a permanent or inline-specified lifetime, applies a condition).
- **Standing mutation** — instantiates a static effect entity (§5) upon invocation. The state contribution and optional trigger are managed by that static effect for its lifetime.

**Event logging.** Every mutation keyword invocation appends one or more structured events to the event log (§7). This is how other keywords and triggers observe the outcomes of mutation.

**Invariant.** A mutation keyword may not be used as a property expression (it may not appear where a return value is expected). Mutation keywords are invoked for their side effects only.

---

### 1.3 Property Keyword

**Definition.** A property keyword is a keyword that queries game state and returns a value. Property keywords do not mutate state and do not append events to the event log.

**Return types.** A property keyword returns exactly one value. Valid return types include: boolean, number, entity reference, collection of entity references, or any other value the engine's type system supports.

**Usage.** Property keywords appear as arguments within effect blocks, as conditions in activation conditions, as criteria in target declarations, and as lifetime while-conditions. They are not standalone steps in an effect block.

**Invariants.**
- A property keyword invocation produces no state changes.
- A property keyword invocation appends nothing to the event log.
- A property keyword may be composed of other property keywords and read primitives, but may not invoke mutation keywords.

---

## 2. Game Entities

The engine defines three first-class game entities: **Player**, **Card**, and **Zone**. All three share the same state model (§3).

---

### 2.1 Player

**Definition.** A player is a first-class engine entity representing one of the two participants in a game. Players are the agents who take actions.

**Relationships.**
- A player is the owner of themselves (for ownership-model consistency).
- A player is the owner of zero or more cards and zero or more zones.
- The engine targets exactly two players: one human, one AI.

**Static properties.** Defined at game setup; read-only during play. Examples: player name, starting hand size, starting accumulator values.

**State.** Players carry the same three mutable state types as cards: modifiers, accumulators, and conditions/tags (§3). All are contribution-tracked by the engine.

**Lifecycle.**
- Created at game setup.
- Exist for the duration of the game.
- Not destroyed mid-game by engine mechanisms (win/loss are state-based rules defined by the game creator).

**Invariants.**
- Exactly two players exist per game.
- Both players exist for the full duration of the game (the engine does not remove players).

---

### 2.2 Card

**Definition.** A card is a first-class engine entity defined at design time (as part of a card definition in a card set) and instantiated at game setup or during play. Cards are the primary objects that move between zones and carry effect blocks.

**Relationships.**
- A card belongs to exactly one card definition.
- A card has exactly one owner (a Player). Set at game setup. Immutable.
- A card occupies at most one zone at any given time.

**Static properties.** Defined at design time on the card definition; read-only during play. Examples: name, base cost, base attack, base health, color, rarity, card art reference. The engine does not prescribe which static properties a card must have beyond what is required for engine mechanics.

**State.** Mutable runtime data. Three types: modifiers, accumulators, conditions/tags (§3). All are contribution-tracked by the engine.

**Effect blocks.** A card may have one or more effect blocks (§4). Exactly one is designated the **primary effect block** — the one that fires when the card is played.

**Lifecycle.**
- A card instance is created (typically at game setup, but game creators may create cards during play).
- A card occupies exactly one zone at all times while it exists; it begins in a zone designated at creation.
- A card moves between zones via mutation keywords defined by the game creator (the engine provides zone membership tracking; movement semantics are game-defined).
- A card may be destroyed (removed from the game) via a game-creator-defined mutation keyword.

**Invariants.**
- Every card has exactly one owner. Owner is set at game setup and never changes.
- Every card occupies at most one zone. A card not in any zone is considered destroyed or removed from the game.
- Card type (creature, spell, etc.) is not an engine concept. Games define their own type taxonomy via static properties and conditions.

---

### 2.3 Zone

**Definition.** A zone is a named container that holds cards. Zones are pure containers — the engine prescribes no inherent behavior. Meaning is assigned to zones by the game creator through rules and property definitions.

**Relationships.**
- A zone has exactly one owner (a Player). Set at game setup. Immutable.
- A zone holds zero or more cards at any given time.
- A card belongs to at most one zone at a time.

**Static properties.** Defined at design time; read-only during play. Example: `max-size` on a hand zone.

**State.** Zones carry the same three mutable state types as cards: modifiers, accumulators, conditions/tags (§3). All are contribution-tracked by the engine.

**Zone grouping.** Zone groups are not a first-class engine concept. When a game creator needs to express "while in play" (meaning "in any of several zones"), they define a named boolean property keyword using `in-zone` and logical operators. Example: `is-in-play(card)` = `or(in-zone(card, battlefield), in-zone(card, structure-zone))`. No separate zone-group entity is defined or tracked by the engine.

**Zone membership as a criterion.** Zone membership — including via composed property keywords — is a valid criterion for:
1. **Lifetime scope** — a static effect's while-condition may reference zone membership.
2. **Effect scope** — an effect may target or apply to cards based on their current zone.

**Lifecycle.**
- Created at game setup.
- Exist for the duration of the game (zones are not created or destroyed mid-game by engine mechanisms).

**Invariants.**
- Every zone has exactly one owner. Owner is set at game setup and never changes.
- A card cannot occupy more than one zone simultaneously.

---

### 2.4 Ownership

**Definition.** Ownership is the relationship between a game entity (card or zone) and a player. It is a first-class engine concept.

**Invariants.**
- Every card has exactly one owner.
- Every zone has exactly one owner.
- Ownership is set at game setup and is immutable for the life of the entity.
- Controller is **not** an engine concept. If a game requires the notion of temporary control by a non-owner, the game creator models it via a condition/tag on the entity.

---

## 3. State Model

All three entity types (Player, Card, Zone) carry the same three kinds of mutable runtime state. The engine tracks the **source** of each state contribution and automatically removes contributions when their source expires. Game creators do not write cleanup logic.

---

### 3.1 Accumulators

**Definition.** An accumulator is an independently tracked numeric value on an entity that is not tied to any static property. It accumulates permanent deltas over the course of the game.

**Examples.** Damage taken, resources spent, times an ability has been activated.

**Contribution model.** Accumulators are modified by permanent delta operations. There is no per-delta contribution tracking — once a delta is applied, it merges into the accumulator's total. The engine does not auto-remove accumulator values (no lifetime). Explicit reset/removal is performed via a dedicated mutation keyword (§9).

**Evaluation.** The current value of an accumulator is its total accumulated delta.

**Invariants.**
- An accumulator starts at zero unless initialized otherwise at entity creation.
- Accumulator values are permanent until explicitly modified or cleared.
- Accumulators have no associated lifetime.

---

### 3.2 Modifiers

**Definition.** A modifier is an adjustment to a static property on an entity. Modifiers do not change the static property's base value; they adjust the computed current value of that property.

**Kinds.**
- **Additive** — adds N to the base value.
- **Multiplicative** — multiplies the sum of the base and all additives by N.

**Evaluation order.** For a given property on a given entity:
```
computed value = (base + Σ additives) × Π multiplicatives
```
All active additive modifiers are summed first; then all active multiplicative modifiers are multiplied together and applied.

**Contribution tracking.** Each modifier contribution is tracked with its source (the entity or effect block that created it) and an optional lifetime specification (§5.1). The engine removes a modifier contribution automatically when its lifetime expires.

**Lifecycle of a contribution.**
- Created when a mutation keyword applies a modifier (directly or via a static effect).
- Removed when: its lifetime expires, or an explicit removal keyword is invoked with its contribution ID.
- Multiple contributions to the same property may coexist simultaneously.

**Invariants.**
- A modifier contribution has exactly one kind: additive or multiplicative.
- A modifier contribution targets exactly one static property on exactly one entity.
- The base value of a static property is never modified; only the computed value changes.

---

### 3.3 Conditions / Tags

**Definition.** A condition (also called a tag) is a categorical or boolean state on an entity. Conditions may represent states such as sleeping, bleeding, flying, or any game-defined category.

**Contribution tracking.** Each condition contribution is tracked with its source and an optional lifetime specification (§5.1). Multiple independent contributions to the same condition name may coexist (from different sources). A condition is **present** on an entity as long as at least one contribution to that condition name exists. A condition is **absent** when all contributions have been removed.

**Lifecycle of a contribution.**
- Created when a mutation keyword applies a condition (directly or via a static effect).
- Removed when: its lifetime expires, or an explicit removal keyword is invoked.
- Removing all contributions of a given name is performed via a dedicated removal keyword. Removing a specific contribution is performed via its contribution ID.

**Invariants.**
- A condition's presence is the logical OR of all its active contributions.
- Removing one contribution does not affect other contributions to the same condition name.
- A condition name with zero contributions is considered absent (not the same as a condition with value false — it simply does not exist on the entity).

---

## 4. Effect Blocks

**Definition.** An effect block is an ordered sequence of mutation keyword invocations that executes as an atomic unit. No triggers or state-based rules resolve while a block is executing, including while the block is paused waiting for a mid-effect prompt.

**Local scope.** An effect block has a local execution scope: a set of named variable bindings available to keywords within the block. Variables are local to the block and cease to exist when the block completes.

**Event log scope.** An effect block's events are a subset of the parent action's event log scope. Keywords within the block may query `events.this_block` to see only the events from the current block, or broader scopes for context from the enclosing action or turn.

---

### 4.1 Variable Binding

**Definition.** A variable is a named reference in the block's local execution scope, bound to a value. Variables allow later keywords in the block to reference the results of earlier keywords or player choices.

**Binding time.**
- **Binding-time variables** — bound before the block begins executing, from targets, cost payment choices, or explicit variable values (e.g. X) supplied by the player as action inputs.
- **Mid-execution variables** — bound during execution via a mid-effect prompt (§4.2).

**Invariants.**
- A variable is local to the block it is declared in.
- A variable may be referenced by any keyword that appears after its binding point in the block.
- A variable may not be referenced before it is bound.
- Variables cease to exist when the block completes.

---

### 4.2 Mid-Effect Prompts

**Definition.** A mid-effect prompt is a mechanism by which an executing effect block pauses to request a player choice that cannot be known at binding time.

**Semantics.**
1. The block reaches a keyword that requires a mid-effect prompt.
2. Execution pauses. The event log is not modified during the pause. No state changes occur during the pause.
3. A choice context is delivered to the player (the set of valid candidates and the number of choices required).
4. The player responds. The response is bound to a named variable in the block's local scope.
5. Execution resumes from the next keyword in the block.

**Short-circuit rule.** If the number of valid candidates is less than or equal to the number of choices required by the prompt, the prompt is skipped: all candidates are automatically selected and bound without player input.

**Atomicity invariant.** No triggers, state-based rules, or other effect blocks resolve while a block is paused for a mid-effect prompt. The block's atomic execution guarantee holds across the pause.

---

## 5. Static Effects

**Definition.** A static effect is a persistent engine entity with a lifetime, an optional state contribution, and an optional trigger. Static effects are the mechanism by which temporary state changes and conditional triggers are expressed.

**Origins.** A static effect is created in one of two ways:

- **Declarative static effect** — defined directly on a card's schema. Becomes active according to the conditions defined in its lifetime specification. Does not require invocation; the engine manages it based on the card's existence and the lifetime condition.
- **Dynamic static effect** — created at runtime by a standing mutation keyword invocation within an effect block. The invocation is what brings the static effect into existence.

Both origins produce entities of the same kind; the distinction is only in how they come to exist.

**Lifecycle.**
- Created: either at game setup (declarative) or when a standing mutation keyword is invoked (dynamic).
- Active: while its lifetime specification is satisfied.
- Expired: when the first of its lifetime conditions is met (see §5.1).
- On expiry: all state contributions from this static effect are automatically removed by the engine.

---

### 5.1 Lifetime Specification

**Definition.** A lifetime specification defines when a static effect expires. It is a set of zero or more primitive lifetime conditions combined as **OR**: the static effect expires as soon as any one of its conditions is satisfied.

**Zero conditions = permanent.** A static effect with an empty lifetime specification is permanent — it persists until explicitly removed by a mutation keyword.

**Primitive lifetime condition types.**

| Type | Description |
|---|---|
| **Turn timer** | Expires after N turns have elapsed since the static effect became active. |
| **Trigger count** | Expires after the static effect's attached trigger has fired N times. (Only valid when the static effect has a trigger.) |
| **While-condition** | Active while a boolean property expression evaluates to true; checked after each effect block resolves. Expires the first time the expression evaluates to false after a check. |

**Composition.** Any number of primitive conditions may be OR'd in a single lifetime specification. The static effect expires on the first satisfied condition.

**While-condition evaluation timing.** The while-condition is not checked continuously; it is checked after each effect block resolves. A static effect whose while-condition becomes false mid-block does not expire until the block completes.

**Invariants.**
- A lifetime specification is immutable after the static effect is created.
- A turn-timer condition has N ≥ 1.
- A trigger-count condition has N ≥ 1 and requires the static effect to have a trigger.
- A while-condition accepts any boolean-valued property expression.

---

### 5.2 State Contribution

**Definition.** The state contribution is the modifier, accumulator delta, or condition/tag that a static effect contributes to an entity while the static effect is active.

**Optionality.** A static effect may have zero or one state contribution. A static effect without a state contribution exists solely to hold a trigger.

**Engine management.** The engine tracks the static effect as the source of its contribution. When the static effect expires, the engine automatically removes the contribution without requiring any game-creator-defined cleanup logic.

---

### 5.3 Trigger

**Definition.** A trigger is a condition on the event log that, when satisfied, causes the static effect to fire an activated effect (an effect block).

**Optionality.** A static effect may have zero or one trigger.

**Trigger resolution order.** When multiple static effects trigger simultaneously (i.e. the same event log condition satisfies multiple triggers), they resolve in **source-lifetime order — the oldest active static effect fires first**. This gives the engine a deterministic resolution order without requiring player input.

**Trigger timing.** Triggers resolve between actions, never mid-block.

**Invariants.**
- A trigger fires at most once per event that satisfies its condition (it does not fire multiple times for the same event).
- Trigger resolution occurs between actions in the scope hierarchy.

---

## 6. Activated Effects

**Definition.** An activated effect is an effect block that executes when a player takes an action (plays a card, activates an ability). It is the direct, player-initiated counterpart to trigger-fired blocks on static effects.

**Activation types.** Every activated effect has exactly one activation type:
- **Directly activatable** — the player may activate this effect block as an action. Subject to an activation condition and a cost.
- **Triggered-only** — fires only via a trigger on a static effect. Cannot be activated directly by the player.

**Activation condition** *(directly activatable only).* A boolean property expression that must evaluate to true for the player to activate the effect. Evaluated at binding time.

**Cost** *(directly activatable only).* An effect block with all-or-nothing semantics. Validated via dry run at binding time; if validation fails, the action cannot be taken. Cost resolves before the main effect block. Events generated by cost resolution are in scope for the main block.

**Primary effect block.** One effect block per card is designated the **primary effect block** — the one that fires when the card is played. A card may have additional effect blocks (activated abilities, modal choices); those are the game creator's responsibility to present and route to.

**Invariants.**
- Every card has exactly one primary effect block.
- A directly activatable effect may not be activated unless its activation condition evaluates to true.
- A cost must fully resolve (all-or-nothing) or the action does not proceed.

---

## 7. Events and Event Log

**Definition.** The event log is an append-only record of everything that happens in a game. It is the primary mechanism for inter-keyword communication within a scope and for trigger conditions on static effects.

**What is logged.** Every mutation keyword invocation appends one or more structured events. Property keyword invocations do not append events.

**Queryable scopes.**

| Scope | Contents |
|---|---|
| `events.this_block` | Events from the current effect block |
| `events.this_action` | Events from the current action (including all its effect blocks and costs) |
| `events.this_turn` | Events from the current turn |
| `events.this_game` | All events in the game |

**Scope hierarchy.**
```
game
  └── turn
        └── phase
              └── action
                    └── effect block
```

Each scope is a strict subset of all scopes above it.

**Trigger resolution timing.** Triggers resolve between actions, never mid-block. A trigger condition is evaluated against `events.this_action` or broader scopes; `events.this_block` is not a valid scope for trigger conditions (it ceases to be meaningful once the block completes).

**Invariants.**
- The event log is append-only. Events are never modified or removed.
- Events are structured (they carry enough information for the trigger condition system to evaluate against them).
- An event is produced by exactly the block that invoked the mutation keyword, not by any containing scope.

---

## 8. Actions and Turn Structure

### 8.1 Actions

**Definition.** An action is the unit of player agency. Taking an action creates an action scope in the event log and executes one or more effect blocks within it. "Playing a card" and "activating an ability" are both actions.

**Binding time.** Before an action resolves, the player (or AI) supplies all **binding-time inputs** declared by the effect block's signature:
- **Targets** — one or more game entities meeting declared criteria.
- **Cost payment choices** — how to pay when multiple payment options exist.
- **Variable values** — e.g. choosing the value of X.

Binding-time inputs are validated before execution begins. If validation fails (no valid targets, cost cannot be paid, etc.), the action cannot be taken.

**Resolution semantics.** Once binding succeeds, resolution does not fail. If a keyword cannot fully execute (e.g. "discard 2 cards" with only 1 in hand), it does as much as it can. There is no rollback. Partial execution is acceptable.

---

### 8.2 Turn Structure and Phases

**Phases.** A turn is composed of phases. The specific phases, their order, and what each phase does are defined by the game creator. The engine prescribes only the internal structure of a phase:

```
phase
  ├── init     (effect block — set up the phase)
  ├── wait     (player action window)
  └── cleanup  (effect block — teardown)
```

Init and cleanup are effect blocks defined by the game creator. Either may be empty.

**Phase prompts.** Prompts generated by phase init/cleanup are discrete actions initiated by the engine between player action windows (e.g. "discard to hand size at end of turn"). These are not mid-effect prompts; they are full actions.

---

### 8.3 Game Rules Mechanisms

The engine provides three mechanisms for game creators to define game-level rules:

**1. Phase init/cleanup.** Effect blocks attached to a phase's lifecycle. Used for things like drawing a card at the start of a turn or triggering a hand-size check at end of turn.

**2. Action rules.** Middleware that wraps a named action type with before/after effect blocks. The action's own effect block resolves in the middle. Multiple action rules may apply to the same action type. Game creators address action rules by action name. Example: a "play card" action rule might move the card to a staging zone before resolution, then to the discard pile after.

**3. State-based rules.** Effect blocks that run automatically after every effect block resolves. The engine re-runs all state-based rules repeatedly until none trigger (i.e. until game state is stable). State-based rules are how win conditions, loss conditions, and mandatory game state corrections (e.g. "destroy units with damage ≥ health") are implemented.

**Convergence.** The engine makes no attempt to detect or terminate infinite loops in state-based rules. Convergence is entirely the game creator's responsibility. Game creators must write state-based rules that, once applied, do not immediately re-trigger themselves.

---

## 9. Built-in Keywords

The engine provides a minimal set of primitive keywords. All game-creator-defined keywords are ultimately composed on top of these. Game creators do not typically invoke primitives directly; they define named keywords (e.g. `take-damage`, `current-health`, `mana-value`) that compose them.

---

### 9.1 Mutation Primitives

| Keyword | Parameters | Description |
|---|---|---|
| `modify-accumulator` | entity, name, delta | Adds delta (positive or negative) to the named accumulator on the entity. Permanent — no lifetime. |
| `clear-accumulator` | entity, name | Resets the named accumulator on the entity to zero. |
| `apply-modifier` | entity, property-name, kind, value, lifetime? | Adds a modifier contribution to the named static property on the entity. kind is additive or multiplicative. lifetime is an optional lifetime specification (§5.1); if omitted, the modifier is permanent. Returns a contribution-ID. |
| `remove-modifier` | contribution-ID | Removes the specific modifier contribution identified by the given ID, regardless of its remaining lifetime. |
| `apply-condition` | entity, condition-name, lifetime? | Applies a condition/tag contribution to the entity. lifetime is optional; if omitted, the condition is permanent. Returns a contribution-ID. |
| `remove-condition` | entity, condition-name | Removes all contributions of the named condition on the entity, regardless of remaining lifetimes. |

**Notes.**
- `apply-modifier` and `apply-condition` each accept an optional inline lifetime, so game creators can express "give +2 attack until end of turn" in a single keyword invocation without separately spawning a static effect. The engine manages cleanup automatically.
- `remove-modifier` removes exactly one contribution (by ID). `remove-condition` removes all contributions of a given name on a given entity. There is no built-in "remove one contribution of a named condition by ID" — if a game needs that granularity, the game creator uses the contribution-ID returned by `apply-condition` and defines their own removal logic around `remove-modifier`'s model.
- `modify-accumulator` produces no contribution-ID; accumulator deltas merge permanently into the total.

---

### 9.2 Read Primitives

| Keyword | Parameters | Returns |
|---|---|---|
| `get-state` | entity, field | The computed current value of a mutable state field on the entity (modifier-adjusted property value, accumulator total, or condition presence). |
| `get-property` | entity, field | The design-time static property value of the entity (unmodified base value). |
| `in-zone` | entity, zone | Boolean: true if the entity is currently in the named zone. |

---

### 9.3 Logical and Comparison Primitives

These are property keywords (stateless, no side effects, no event log entries). They are used to compose conditions, activation conditions, and lifetime while-conditions.

| Keyword | Arity | Returns |
|---|---|---|
| `less-than(a, b)` | binary numeric | true if a < b |
| `greater-than(a, b)` | binary numeric | true if a > b |
| `at-least(a, b)` | binary numeric | true if a ≥ b |
| `at-most(a, b)` | binary numeric | true if a ≤ b |
| `equal-to(a, b)` | binary | true if a = b |
| `not(p)` | unary boolean | true if p is false |
| `and(p, q)` | binary boolean | true if both p and q are true |
| `or(p, q)` | binary boolean | true if at least one of p, q is true |

---

## 10. Card Sets and Pool

### 10.1 Card Set

**Definition.** A card set is a named collection of card definitions. Card sets may be grouped thematically or mechanically, but that is a game-level concern — the engine imposes no structure beyond a name and membership.

**Invariants.**
- A card set has exactly one name.
- A card set contains zero or more card definitions.

---

### 10.2 Card Pool

**Definition.** The card pool is the union of all card sets available to a given game. It is the complete set of card definitions from which a game's cards may be drawn.

**Invariants.**
- The card pool is the union of one or more card sets.
- The card pool is determined at game setup and does not change during a game.

---

## Glossary

| Term | Brief definition |
|---|---|
| **Accumulator** | A permanently tracked numeric value on an entity, not tied to any static property (§3.1). |
| **Action** | The unit of player agency; creates an action scope and executes effect blocks (§8.1). |
| **Activated effect** | An effect block that executes when a player takes an action (§6). |
| **Binding time** | The moment before an action executes when inputs are validated and bound (§8.1). |
| **Card** | A first-class entity with static properties, state, and effect blocks (§2.2). |
| **Card pool** | The union of all card sets available to a game (§10.2). |
| **Card set** | A named collection of card definitions (§10.1). |
| **Composite keyword** | A keyword that invokes other keywords as part of its definition (§1.1). |
| **Condition / Tag** | A categorical or boolean state on an entity, contribution-tracked (§3.3). |
| **Contribution** | A single source-tracked instance of a modifier or condition on an entity (§3). |
| **Contribution-ID** | An opaque identifier returned by `apply-modifier` or `apply-condition`, used to remove that specific contribution (§9.1). |
| **Controller** | Not an engine concept. Game creators model it via conditions/tags (§2.4). |
| **Declarative static effect** | A static effect defined directly on a card's schema (§5). |
| **Direct mutation** | A mutation keyword that immediately changes state upon invocation (§1.2). |
| **Directly activatable** | An activation type: the player may activate the effect block as an action (§6). |
| **Dynamic static effect** | A static effect created at runtime by a standing mutation keyword (§5). |
| **Effect block** | An ordered, atomic sequence of mutation keyword invocations (§4). |
| **Event log** | The append-only record of all mutation keyword invocations in a game (§7). |
| **Keyword** | The primitive unit of expression: a named, parameterized, dual-use function (§1.1). |
| **Lifetime specification** | A set of OR'd primitive conditions defining when a static effect expires (§5.1). |
| **Mid-effect prompt** | A mechanism for binding a player choice to a variable during block execution (§4.2). |
| **Modifier** | An adjustment to a static property, tracked by source and optional lifetime (§3.2). |
| **Mutation keyword** | A keyword subtype that changes game state when invoked (§1.2). |
| **Owner** | The player who owns a card or zone; immutable after game setup (§2.4). |
| **Phase** | A segment of a turn with init, wait, and cleanup sub-stages (§8.2). |
| **Player** | A first-class engine entity representing one of the two game participants (§2.1). |
| **Primary effect block** | The designated effect block that fires when a card is played (§6). |
| **Primitive keyword** | A keyword that invokes engine built-ins directly (§1.1). |
| **Property keyword** | A keyword subtype that queries state and returns a value; no side effects (§1.3). |
| **Standing mutation** | A mutation keyword that instantiates a static effect entity (§1.2). |
| **State-based rule** | An effect block that runs automatically after every effect block until game state is stable (§8.3). |
| **Static effect** | A persistent engine entity with a lifetime, optional state contribution, and optional trigger (§5). |
| **Triggered-only** | An activation type: the effect block fires only via a trigger, never directly (§6). |
| **Turn timer** | A lifetime condition that expires after N turns (§5.1). |
| **Trigger** | A condition on the event log that fires an effect block when satisfied (§5.3). |
| **Trigger count** | A lifetime condition that expires after the trigger has fired N times (§5.1). |
| **While-condition** | A lifetime condition that holds while a boolean property expression is true (§5.1). |
| **Zone** | A named container for cards; has the same state model as cards (§2.3). |
