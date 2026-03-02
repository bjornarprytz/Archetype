# Archetype — Domain Model

## Status
**Complete. Signed off 2026-03-02.**

All seven requirements-phase open items and all thirteen architecture-phase additions (A1–A13) are resolved and incorporated below.

This document is the canonical vocabulary for the Archetype card game engine. It is the source of truth for the architect and implementer roles. It is implementation-agnostic: no data structures, programming languages, or frameworks are specified here.

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

**Architecture-phase additions** (gaps flagged by D4, D6, D8, D9, D12, D13):

| # | Item | Resolution |
|---|------|------------|
| A1 | Declarative static effect re-activation | Declarative while-conditions re-instantiate: each time a while-condition transitions from false to true, a new instance is created with fresh identity and counters. Dynamic effects expire permanently. Defined in §5. |
| A2 | `events-matching` primitive | Added to §9.2 with `EventScope` type, optional `candidate`-scoped predicate, and collection primitives `count`, `any`, `sum-arg` in new §9.4. |
| A3 | `EventRef` type and `event-arg` primitive | `EventRef` defined as a first-class read-only value type in §7.1. `event-arg` added to §9.2. `trigger_event` reserved name documented in §5.3. |
| A4 | `random-int` and `shuffle` primitives | Added to §9.2 as restricted property keywords. Valid only in effect block bodies; prohibited in all deterministic evaluation contexts. RNG non-game-state status documented. |
| A5 | `create-card`, `copy-card`, `create-zone` primitives | Added to §9.1 with Returns column. `copy-card` copies no runtime state; declarative static effects activate fresh. Both card-creation primitives log the same event type. |
| A6 | `CardDefinitionName` and `ZoneDefinitionName` types | Defined as string-valued types in §9.1 notes; validated at authoring time, resolved at load time. |
| A7 | Ownership timing clarification (dynamic creation) | §2.2, §2.3, §2.4 updated: "set at game setup" → "set at the moment of creation." Immutability is the invariant, not the timing. |
| A8 | Zone destruction (runtime-created zones) | §2.3 updated: zones may be created during play via `create-zone`; once created, a zone is never destroyed. Game creators model inactive zones via conditions. |
| A9 | `ParameterModification` on static effects | Added as §5.4: `ParameterAdjustment` (additive/multiplicative/replace) and `Disable` variants; filter condition; interception at every dispatch point; `Disable` precedence. |
| A10 | Reserved binding names (`source`, `original`) | All four reserved names consolidated in new §4.3: `trigger_event`, `candidate`, `source`, `original`. |
| A11 | `keyword-disabled` engine event | Defined in §5.4 and tabulated in §7 built-in engine events. Bound args: `"keyword"` plus one entry per suppressed invocation arg. |
| A12 | Arithmetic primitives | `add`, `subtract`, `multiply`, `max`, `min` added to §9.3. |
| A13 | Trigger resolution order as a game-level setting | §5.3 updated: fixed "oldest first" replaced by game-level setting with three options — oldest first (default), newest first, player choice. |

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
- Every card has exactly one owner. Owner is set at the moment of creation and never changes.
- Every card occupies at most one zone. A card not in any zone is considered destroyed or removed from the game.
- Card type (creature, spell, etc.) is not an engine concept. Games define their own type taxonomy via static properties and conditions.

---

### 2.3 Zone

**Definition.** A zone is a named container that holds cards. Zones are pure containers — the engine prescribes no inherent behavior. Meaning is assigned to zones by the game creator through rules and property definitions.

**Relationships.**
- A zone has exactly one owner (a Player). Set at the moment of creation. Immutable.
- A zone holds zero or more cards at any given time.
- A card belongs to at most one zone at a time.

**Static properties.** Defined at design time; read-only during play. Example: `max-size` on a hand zone.

**State.** Zones carry the same three mutable state types as cards: modifiers, accumulators, conditions/tags (§3). All are contribution-tracked by the engine.

**Zone grouping.** Zone groups are not a first-class engine concept. When a game creator needs to express "while in play" (meaning "in any of several zones"), they define a named boolean property keyword using `in-zone` and logical operators. Example: `is-in-play(card)` = `or(in-zone(card, battlefield), in-zone(card, structure-zone))`. No separate zone-group entity is defined or tracked by the engine.

**Zone membership as a criterion.** Zone membership — including via composed property keywords — is a valid criterion for:
1. **Lifetime scope** — a static effect's while-condition may reference zone membership.
2. **Effect scope** — an effect may target or apply to cards based on their current zone.

**Lifecycle.**
- Created at game setup (for zones declared in the game definition) or during play via `create-zone` (§9.1).
- Once created, a zone exists for the remainder of the game. The engine provides no primitive to destroy a zone. Game creators who need to model an "inactive" or "closed" zone may apply conditions to it; the zone entity itself persists.

**Invariants.**
- Every zone has exactly one owner. Owner is set at the moment of creation and never changes.
- A card cannot occupy more than one zone simultaneously.
- A zone is never destroyed by engine mechanisms; it exists until the game ends.

---

### 2.4 Ownership

**Definition.** Ownership is the relationship between a game entity (card or zone) and a player. It is a first-class engine concept.

**Invariants.**
- Every card has exactly one owner.
- Every zone has exactly one owner.
- Ownership is set at the moment of entity creation and is immutable for the life of the entity.
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

### 4.3 Reserved Names

The engine reserves the following binding names across all evaluation contexts. Game creators may not declare variables, parameters, or keyword names that clash with these.

| Name | Context | Value |
|---|---|---|
| `trigger_event` | Trigger-fired effect block scope | The `EventRef` of the event that satisfied the trigger (§5.3). Always pre-bound; always present. |
| `candidate` | `events-matching` predicate expression | The `EventRef` of the event currently being tested against the predicate (§9.2). |
| `source` | Static effect evaluation contexts (parameter modification filter and adjustment expressions; trigger conditions) | The entity on which the static effect is defined — its owning entity (§5.4, §5.3). |
| `original` | Parameter modification adjustment expressions | In Additive and Multiplicative expressions: the raw invocation argument value before any adjustments. In Replace expressions: the running result of all preceding Replace adjustments (§5.4). |

---

## 5. Static Effects

**Definition.** A static effect is a persistent engine entity with a lifetime, an optional state contribution, and an optional trigger. Static effects are the mechanism by which temporary state changes and conditional triggers are expressed.

**Origins.** A static effect is created in one of two ways:

- **Declarative static effect** — defined directly on a card's schema. Does not require invocation; the engine manages activation and re-activation based on the card's existence and its lifetime conditions. A declarative static effect with a while-condition re-instantiates each time the while-condition transitions from false to true: the engine creates a new instance with a new identity, fresh trigger fire count, and fresh high-water mark (see Lifecycle below).
- **Dynamic static effect** — created at runtime by a standing mutation keyword invocation within an effect block. The invocation is what brings the static effect into existence.

Both origins produce entities of the same kind. They differ in two ways: how they come to exist, and whether they re-instantiate after a while-condition expiry (declarative do; dynamic do not).

**Lifecycle.**
- Created: either at game setup (declarative) or when a standing mutation keyword is invoked (dynamic).
- Active: while its lifetime specification is satisfied.
- Expired: when the first of its lifetime conditions is met (see §5.1).
- On expiry: all state contributions from this static effect are automatically removed by the engine.
- **Re-instantiation (declarative effects with a while-condition only):** after a declarative static effect expires because its while-condition evaluated to false, the engine monitors the while-condition on subsequent checks. When the condition evaluates to true again, a new instance of the effect is created — with a new identity, a trigger fire count of zero, and a high-water mark of zero. The original expired instance is not resumed; the new instance and the expired instance are fully distinct entities. Dynamic static effects never re-instantiate; their expiry is permanent.

---

### 5.1 Lifetime Specification

**Definition.** A lifetime specification defines when a static effect expires. It is a set of zero or more primitive lifetime conditions combined as **OR**: the static effect expires as soon as any one of its conditions is satisfied.

**Zero conditions = permanent.** A static effect with an empty lifetime specification is permanent — it persists until explicitly removed by a mutation keyword.

**Primitive lifetime condition types.**

| Type | Description |
|---|---|
| **Turn timer** | Expires after N turns have elapsed since the static effect became active. |
| **Trigger count** | Expires after the static effect's attached trigger has fired N times. (Only valid when the static effect has a trigger.) |
| **While-condition** | Active while a boolean property expression evaluates to true; checked after each effect block resolves. Expires the first time the expression evaluates to false after a check. For **declarative** static effects, expiry under a while-condition is not permanent: when the expression later evaluates to true again, a new instance is created (see §5 Lifecycle). For **dynamic** static effects, expiry under a while-condition is permanent. |

**Composition.** Any number of primitive conditions may be OR'd in a single lifetime specification. The static effect expires on the first satisfied condition.

**While-condition evaluation timing.** The while-condition is not checked continuously; it is checked after each effect block resolves. A static effect whose while-condition becomes false mid-block does not expire until the block completes. The re-instantiation check for declarative effects occurs at the same moment: after each effect block resolves, the engine both expires declarative instances whose while-conditions have become false and creates new instances of declarative effects whose while-conditions have become true.

**Invariants.**
- A lifetime specification is immutable after the static effect is created.
- A turn-timer condition has N ≥ 1.
- A trigger-count condition has N ≥ 1 and requires the static effect to have a trigger.
- A while-condition accepts any boolean-valued property expression.
- A re-instantiated declarative static effect is a distinct entity from its predecessor; it does not inherit the expired instance's trigger fire count, high-water mark, or contribution IDs.

---

### 5.2 State Contribution

**Definition.** The state contribution is the modifier, accumulator delta, or condition/tag that a static effect contributes to an entity while the static effect is active.

**Optionality.** A static effect may have zero or one state contribution. A static effect without a state contribution exists solely to hold a trigger.

**Engine management.** The engine tracks the static effect as the source of its contribution. When the static effect expires, the engine automatically removes the contribution without requiring any game-creator-defined cleanup logic.

---

### 5.3 Trigger

**Definition.** A trigger is a condition on the event log that, when satisfied, causes the static effect to fire an activated effect (an effect block).

**Optionality.** A static effect may have zero or one trigger.

**Trigger resolution order.** Trigger resolution order is a **game-level setting** — the game creator declares one of three modes when defining the game:
- **Oldest first** (default) — the oldest active static effect (lowest identity value) fires first. Within a single effect, the earliest matching event fires first. Deterministic without player input.
- **Newest first** — the newest active static effect fires first. Within a single effect, event order is still chronological. Also deterministic without player input.
- **Player choice** — when multiple effects trigger simultaneously, the active player orders them before any fire. Within a single effect, event order remains chronological.

The default (`oldest first`) is appropriate for most games and requires no player interaction.

**Trigger timing.** Triggers resolve between actions, never mid-block.

**Triggering event access.** When a trigger fires its effect block, the engine pre-binds the event that satisfied the trigger to the reserved name `trigger_event` in the block's local scope, typed as `EventRef` (§7.1). This binding is always present in a trigger-fired block regardless of what other bindings the game creator declares. The block uses `event-arg(trigger_event, name)` (§9.2) to access the triggering event's bound arguments. Game creators may additionally declare named convenience bindings that map specific event arguments to friendlier variable names; these coexist with `trigger_event` and do not replace it.

**Invariants.**
- A trigger fires at most once per event that satisfies its condition (it does not fire multiple times for the same event).
- Trigger resolution occurs between actions in the scope hierarchy.
- Every trigger-fired block has `trigger_event` pre-bound in its local scope as an `EventRef`. This name is reserved; game creator-declared variables may not use it.

---

### 5.4 Parameter Modification

**Definition.** A parameter modification is a fourth optional component on a static effect. It intercepts mutation keyword invocations before they execute — adjusting argument values or cancelling the invocation entirely. Interception applies at every dispatch point in the execution tree, including invocations deep inside composite keywords, not only at the block-step level.

**Optionality.** A static effect may have zero or one parameter modification.

**Variants.**

**`ParameterAdjustment`** — modifies the argument values of a named mutation keyword before execution proceeds. Three kinds of per-parameter adjustment may be declared:
- **Additive** — adds a numeric delta to the parameter value. `original` refers to the raw invocation argument.
- **Multiplicative** — multiplies the parameter value by a numeric factor. `original` also refers to the raw invocation argument.
- **Replace** — replaces the parameter value outright. `original` refers to the running result of all preceding Replace adjustments.

Evaluation order for a given parameter mirrors §3.2: all active additive adjustments are summed and applied first; then all multiplicative adjustments are multiplied together and applied; then Replace adjustments are applied in oldest-first order, each seeing the previous result as `original`.

**`Disable`** — cancels the target invocation entirely. The keyword does not execute. Its normal event is not logged. A `keyword-disabled` engine event is appended instead (see below). Triggers subscribed to the original keyword do not fire; triggers subscribed to `keyword-disabled` may fire.

If any active static effect's parameter modification is a `Disable` that matches the invocation, the invocation is cancelled — regardless of how many other effects carry `ParameterAdjustment` on the same keyword.

**Filter condition.** Both variants carry an optional boolean filter expression evaluated before interception is applied. If the filter is absent or evaluates to true, the modification is applied; otherwise it is skipped. The filter expression has access to:
- The reserved name `source` — the entity on which the static effect is defined (§4.3).
- Named invocation arguments declared by the game creator on the modification.
- Current game state (via property keywords).

The filter may not invoke mutation keywords and may not access the event log. It is evaluated synchronously at dispatch time, before any event is logged.

**`keyword-disabled` engine event.** When a `Disable` fires, the engine appends a `keyword-disabled` event in place of the suppressed invocation. Its bound arguments always include:
- `"keyword"` — the name of the suppressed mutation keyword.
- One entry per bound argument of the suppressed invocation, using the same argument names as the keyword's declared parameters.

This event is fully observable by the trigger system and `events-matching`. Game creators write trigger conditions on `keyword-disabled`, filtering by `"keyword"`, to react to specific suppressions.

**Invariants.**
- A parameter modification targets exactly one mutation keyword by name.
- `Disable` takes precedence over any `ParameterAdjustment` modifications active on the same invocation.
- The filter expression may not invoke mutation keywords and may not access the event log.
- `source` in any parameter modification expression resolves to the owning entity of the static effect.
- `original` resolves to the raw invocation argument for Additive and Multiplicative adjustments; to the running post-Replace result for Replace adjustments.

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

**Built-in engine events.** The engine itself appends certain events that are not produced by game-creator-defined keywords. These are fully observable by the trigger system and `events-matching`.

| Event keyword name | When appended | Bound arguments |
|---|---|---|
| `keyword-disabled` | When a `Disable` parameter modification (§5.4) cancels a mutation keyword invocation. | `"keyword"` — the suppressed keyword's name; plus one entry per bound argument of the suppressed invocation, using the same argument names as the keyword's declared parameters. |

**Invariants.**
- The event log is append-only. Events are never modified or removed.
- Events are structured (they carry enough information for the trigger condition system to evaluate against them).
- An event is produced by exactly the block that invoked the mutation keyword, not by any containing scope.
- Built-in engine events follow the same structure as game-creator-defined events and are subject to the same trigger and query mechanisms.

---

### 7.1 EventRef

**Definition.** An `EventRef` is a read-only reference to a specific, already-finalized event in the event log. It is a first-class value type in the engine's type system: it can be stored in block-scope variable bindings, passed as an argument to keywords that accept it, and returned by `events-matching` (§9.2).

**Contents.** An `EventRef` exposes two pieces of information:
1. The **keyword name** of the event — the name of the mutation keyword that produced it.
2. The **bound arguments** — the set of named values that were bound to the keyword's parameters at the time of invocation.

**Accessor.** The engine provides a single built-in read primitive for operating on an `EventRef`: `event-arg(event, name)` (§9.2). No other built-in accessors exist. Game creators compose named property keywords on top of `event-arg` to express domain concepts (e.g. `damage-amount(event)` = `event-arg(event, "amount")`).

**Provenance.** `EventRef` values enter a block's scope in three ways:
1. **`events-matching` result** — each item in the `Collection<EventRef>` returned by `events-matching` is an `EventRef`.
2. **`trigger_event` binding** — in a trigger-fired effect block, the engine pre-binds the triggering event to the reserved name `trigger_event` as an `EventRef` (see §5.3).
3. **`event-arg` return** — if an event's bound argument is itself an `EventRef`, `event-arg` returns it typed as `EventRef`.

**Invariants.**
- An `EventRef` always refers to a finalized event. It never refers to an event that has not yet been appended to the log.
- An `EventRef` is read-only. Game creators cannot use it to modify the event or the log.
- Accessing a named argument that does not exist on the referenced event is an authoring-time error, caught by the tooling.

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

| Keyword | Parameters | Returns | Description |
|---|---|---|---|
| `modify-accumulator` | entity, name, delta | — | Adds delta (positive or negative) to the named accumulator on the entity. Permanent — no lifetime. |
| `clear-accumulator` | entity, name | — | Resets the named accumulator on the entity to zero. |
| `apply-modifier` | entity, property-name, kind, value, lifetime? | ContributionId | Adds a modifier contribution to the named static property on the entity. kind is additive or multiplicative. lifetime is an optional lifetime specification (§5.1); if omitted, the modifier is permanent. |
| `remove-modifier` | contribution-ID | — | Removes the specific modifier contribution identified by the given ID, regardless of its remaining lifetime. |
| `apply-condition` | entity, condition-name, lifetime? | ContributionId | Applies a condition/tag contribution to the entity. lifetime is optional; if omitted, the condition is permanent. |
| `remove-condition` | entity, condition-name | — | Removes all contributions of the named condition on the entity, regardless of remaining lifetimes. |
| `create-card` | zone: Entity, definition-name: CardDefinitionName, owner: Player | Entity | Instantiates a new card from the named card definition; places it in the specified zone with the given owner. Declarative static effects from the definition are activated immediately. Appends a creation event. |
| `copy-card` | source: Entity, destination-zone: Entity, owner: Player | Entity | Instantiates a new card using the same definition as `source`. The new card carries no runtime state from `source` — it starts with no modifiers, accumulators, or conditions, and its declarative static effects are activated fresh. Appends a creation event. |
| `create-zone` | owner: Player, definition-name: ZoneDefinitionName | Entity | Instantiates a zone from the named zone definition; initially empty. Appends a creation event. |

**Notes.**
- `apply-modifier` and `apply-condition` each accept an optional inline lifetime, so game creators can express "give +2 attack until end of turn" in a single keyword invocation without separately spawning a static effect. The engine manages cleanup automatically.
- `remove-modifier` removes exactly one contribution (by ID). `remove-condition` removes all contributions of a given name on a given entity. There is no built-in "remove one contribution of a named condition by ID" — if a game needs that granularity, the game creator uses the contribution-ID returned by `apply-condition` and defines their own removal logic around `remove-modifier`'s model.
- `modify-accumulator` produces no contribution-ID; accumulator deltas merge permanently into the total.
- `create-card` and `copy-card` both append a creation event of the same keyword name. They are distinct authoring conveniences — `copy-card` derives its definition from a live entity reference rather than a named definition string — but they are not distinguishable by event type alone in the event log.
- **`CardDefinitionName`** is a string-valued type representing the name of a card definition registered in the game definition. The tooling validates it against the registered card definitions at authoring time. The engine resolves it to a definition reference at game-definition load time; no name lookup occurs at execution time.
- **`ZoneDefinitionName`** is a string-valued type representing the name of a zone definition registered in the game definition. Same validation and resolution semantics as `CardDefinitionName`.

---

### 9.2 Read Primitives

All are property keywords: no state changes, no event log entries.

| Keyword | Parameters | Returns |
|---|---|---|
| `get-state` | entity, field | The computed current value of a mutable state field on the entity (modifier-adjusted property value, accumulator total, or condition presence). |
| `get-property` | entity, field | The design-time static property value of the entity (unmodified base value). |
| `in-zone` | entity, zone | Boolean: true if the entity is currently in the named zone. |
| `events-matching` | scope, keyword-name, predicate? | A collection of all events in the given scope, at any depth in the event tree, whose keyword name matches `keyword-name` and (if a predicate is supplied) whose bound arguments satisfy the predicate. Returns `Collection<EventRef>`. See notes below. |
| `event-arg` | event: EventRef, name: string | The value of the named bound argument on the referenced event. The return type matches the declared parameter type of the keyword that produced the event. Accessing a name that does not exist on the event is an authoring-time error. |
| `random-int` | min: Number, max: Number | A uniformly distributed integer in the range [min, max] inclusive. See randomness note below. |
| `shuffle` | collection: Collection\<Entity\> | A new collection containing the same entities as the input in a random order. Does not mutate the source collection. See randomness note below. |

**`events-matching` notes.**

**`scope`** is an `EventScope` value — one of: `this-block`, `this-action`, `this-turn`, `this-game`. Each is a strict superset of the one before it. Trigger conditions may not use `this-block` as the scope: block scope is no longer meaningful once the block exits. All four scopes are valid within an executing effect block.

**`keyword-name`** is a string. Only events whose recorded keyword name exactly matches are considered. The search descends to any depth in the event tree — events produced by internally invoked keywords within a composite keyword are included, not just top-level events in the scope.

**`predicate`** is an optional boolean property expression. If supplied, it is evaluated once per candidate event; only events for which it returns true are included in the result. Within the predicate expression, the reserved name `candidate` refers to the `EventRef` of the event currently being tested. The predicate uses `event-arg(candidate, name)` (§9.2, A3) to access the event's bound arguments. No state changes occur during predicate evaluation; the event log is not modified.

**Invariants.**
- `events-matching` is a property keyword: it returns a value and produces no side effects.
- The result reflects the state of the event log at the moment of evaluation. Events appended after the call are not included.
- A trigger condition must not supply `this-block` as its scope.

**Randomness note** (`random-int`, `shuffle`).

Both `random-int` and `shuffle` are property keywords in that they return values, mutate no game state, and append nothing to the event log. They differ from other property keywords in one respect: each invocation advances the engine's internal random number generator. RNG state is not game state — it is not queryable, not contribution-tracked, and not logged — so this advancement does not violate the property keyword invariant at the domain level.

Because randomness makes evaluation non-repeatable, `random-int` and `shuffle` are **restricted to effect block bodies** (including cost blocks). They may not appear in any deterministic evaluation context:
- Trigger conditions
- Lifetime while-conditions
- State-based rule conditions
- Activation conditions

Violations are caught at authoring time by the tooling.

The randomness consumed by these primitives is implicitly recorded in the event log: the mutation keyword that receives the random value logs it as a bound argument. For example, `modify-accumulator(goblin, "damage", random-int(1, 6))` logs `{delta: 4}`, not `{delta: random-int(1, 6)}`.

**Invariants.**
- `min` must be less than or equal to `max` for `random-int`. This is an authoring-time constraint.
- `shuffle` returns a new collection; the source collection is not modified.
- Neither primitive may appear in a trigger condition, while-condition, state-based rule condition, or activation condition.

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
| `add(a, b)` | binary numeric | a + b |
| `subtract(a, b)` | binary numeric | a − b |
| `multiply(a, b)` | binary numeric | a × b |
| `max(a, b)` | binary numeric | the greater of a and b |
| `min(a, b)` | binary numeric | the lesser of a and b |

---

### 9.4 Collection Primitives

These are property keywords that operate on collections returned by `events-matching` (§9.2). All are stateless, produce no side effects, and append nothing to the event log.

| Keyword | Parameters | Returns |
|---|---|---|
| `count(collection)` | `Collection<EventRef>` | Number: the number of items in the collection. |
| `any(collection)` | `Collection<EventRef>` | Boolean: true if the collection contains at least one item. Equivalent to `greater-than(count(collection), 0)` but stated directly for clarity. |
| `sum-arg(collection, arg-name)` | `Collection<EventRef>`, string | Number: the sum of the named numeric bound argument across all events in the collection. If any event in the collection does not carry the named argument, or if the argument is not numeric, that event contributes zero to the sum. |

**Examples.**
- "How many creatures died this turn?" → `count(events-matching(this-turn, "creature-died"))`
- "Did any damage occur this block?" → `any(events-matching(this-block, "take-damage"))`
- "Total damage dealt this block" → `sum-arg(events-matching(this-block, "modify-accumulator"), "delta")`

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
| **`event-arg`** | A read primitive that returns the value of a named bound argument on an `EventRef` (§9.2). |
| **`CardDefinitionName`** | A string-valued type naming a card definition registered in the game definition; validated at authoring time, resolved at load time (§9.1). |
| **`ZoneDefinitionName`** | A string-valued type naming a zone definition registered in the game definition; same semantics as `CardDefinitionName` (§9.1). |
| **`create-card`** | A mutation primitive that instantiates a card from a named definition and places it in a zone (§9.1). |
| **`copy-card`** | A mutation primitive that instantiates a fresh card sharing the source entity's definition but carrying no runtime state (§9.1). |
| **`create-zone`** | A mutation primitive that instantiates a zone from a named definition (§9.1). |
| **`Disable`** | A parameter modification variant that cancels a named mutation keyword invocation and logs a `keyword-disabled` event instead (§5.4). |
| **`keyword-disabled`** | A built-in engine event appended when a `Disable` parameter modification cancels an invocation (§5.4, §7). |
| **`original`** | Reserved binding name in parameter modification adjustment expressions; the raw invocation argument or running Replace result (§4.3, §5.4). |
| **`ParameterAdjustment`** | A parameter modification variant that adjusts a mutation keyword's argument values before execution (§5.4). |
| **`ParameterModification`** | A fourth optional component on a static effect that intercepts and adjusts or cancels named mutation keyword invocations (§5.4). |
| **`source`** | Reserved binding name in static effect evaluation contexts; the entity on which the static effect is defined (§4.3, §5.3, §5.4). |
| **`random-int`** | A property keyword returning a uniformly distributed integer in [min, max] inclusive; advances RNG state; valid only in effect block bodies (§9.2). |
| **`shuffle`** | A property keyword returning a new randomly ordered `Collection<Entity>`; advances RNG state; valid only in effect block bodies (§9.2). |
| **`EventRef`** | A read-only reference to a finalized event in the event log; a first-class value type (§7.1). |
| **`events-matching`** | A read primitive that queries the event log by scope, keyword name, and optional predicate; returns a `Collection<EventRef>` (§9.2). |
| **`EventScope`** | An enumeration of the four queryable event log scopes: `this-block`, `this-action`, `this-turn`, `this-game` (§9.2). |
| **`candidate`** | Reserved binding name within an `events-matching` predicate expression; refers to the `EventRef` of the event currently being tested (§9.2). |
| **`trigger_event`** | Reserved binding name in every trigger-fired effect block; holds the `EventRef` of the event that satisfied the trigger (§5.3). |
| **`count`** | Collection primitive: returns the number of items in a `Collection<EventRef>` (§9.4). |
| **`any`** | Collection primitive: returns true if a `Collection<EventRef>` contains at least one item (§9.4). |
| **`sum-arg`** | Collection primitive: returns the sum of a named numeric argument across all events in a `Collection<EventRef>` (§9.4). |
| **Standing mutation** | A mutation keyword that instantiates a static effect entity (§1.2). |
| **State-based rule** | An effect block that runs automatically after every effect block until game state is stable (§8.3). |
| **Static effect** | A persistent engine entity with a lifetime, optional state contribution, and optional trigger (§5). |
| **Triggered-only** | An activation type: the effect block fires only via a trigger, never directly (§6). |
| **Turn timer** | A lifetime condition that expires after N turns (§5.1). |
| **Trigger** | A condition on the event log that fires an effect block when satisfied (§5.3). |
| **Trigger count** | A lifetime condition that expires after the trigger has fired N times (§5.1). |
| **While-condition** | A lifetime condition that holds while a boolean property expression is true (§5.1). |
| **Zone** | A named container for cards; has the same state model as cards (§2.3). |
