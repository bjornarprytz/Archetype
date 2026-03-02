# Archetype — Requirements

## Status
**Complete. Signed off 2026-03-01.**

---

## Purpose

Archetype is a card game engine. The primary goal is a card definition system where a single definition drives both:
1. **Rules execution** — the engine resolves game state changes
2. **Card text rendering** — human-readable rules text is generated from the same definition

Magic: The Gathering is the closest reference point for the kind of card game this engine should support.

---

## Cards

Cards have two categories of data:

### Static Properties
Defined at design time. **Read-only during play.** Examples: base attack, base health, name, cost, color, rarity.

Card types (e.g. creature, spell, thing) are **not engine-level concepts**. The engine does not define or enforce card types. Games built on the engine may define their own type taxonomy and implement associated rules.

### State
Mutable runtime data. The engine defines the following first-class state types:

- **Modifiers** — adjustments to static properties. Can be additive (+N) or multiplicative (×N).
  Evaluation order: all additive modifiers are applied first, then multiplicative.
  Computed value = (base + sum of additives) × product of multiplicatives.

- **Accumulators** — independently tracked values not tied to a static property (e.g. damage taken).

- **Conditions / Tags** — categorical or boolean states (e.g. sleeping, bleeding). May have timers associated with them.

State is a **collection of contributions**. Multiple static effects may contribute to the same state field simultaneously. The engine tracks the source of each contribution and automatically removes it when the contributing effect expires. Game creators do not write cleanup logic.

---

## Keywords, Effects, and Properties

**Keyword** is the umbrella concept — a named function with typed parameters that is composable and dual-use (supports both execution and human-readable text rendering). Keywords come in two subtypes:

### Effects
An effect is a keyword that **mutates game state**. Effects are the only place where game state changes occur.

Effects can be:
- **Primitive** — directly mutate state by calling the engine's built-in `change-state` keyword (e.g. `take_damage(atom, amount)` reduces health via `change-state`)
- **Composite** — call other effects with derived arguments (e.g. `attack(atom, amount)` calls `take_damage` after subtracting defense)

### Properties
A property is a keyword that **queries game state** and returns a value. Properties do not mutate state. Return types include booleans, numbers, collections, or other values.

Properties can be:
- **Primitive** — read a base value directly (e.g. `base_health(unit)`)
- **Composed** — combine other properties into a named concept (e.g. `delirium` = true if 4+ distinct card types in discard pile; `creatures_with_type(wizard)` = collection of wizard creatures in play)

Properties are used as arguments and conditions within effect blocks — they are not standalone steps.

The full composition tree of both effects and properties must be available at runtime so the game layer can decide what to show the player.

> **Terminology note**: the word "effect" is also used in the terms *activated effect* and *static effect* (see below). In those terms it carries the broader meaning of "something that produces an outcome." The keyword subtype *effect* (mutation keyword) is a narrower use. This overloading is flagged for the architect to consider resolving.

---

## Effect Blocks

An effect block is an ordered sequence of **effect** invocations (mutation keywords). It is the atomic unit of execution — no triggers or state-based rules resolve while a block is executing.

Effect blocks have access to a scoped view of the event log, allowing later effects in the block to reference the results of earlier ones (e.g. heal for the total damage dealt in this block). Properties may be used as arguments to effects within the block.

Effect blocks are defined on cards and can also be fired by triggers on static effects.

---

## Events

Every **effect** execution appends structured events to a global append-only event log. Properties do not append events — they only read state. The log is queryable by scope:

- `events.this_block`
- `events.this_action`
- `events.this_turn`
- `events.this_game`

This log is the mechanism for inter-effect communication within a scope, and for trigger conditions on static effects.

---

## Effects: Static vs Activated

- **Activated effect**: an effect block that executes when a player takes an action (plays a card, activates an ability).
- **Static effect**: a persistent condition with a defined lifetime. A static effect may have a **trigger** — a condition on the event log — which fires an activated effect when met.

### Static Effect Lifetimes

The engine provides three first-class lifetime types. Game creators compose them when defining a static effect:

- **Turn timer** — expires after N turns.
- **Trigger count** — expires after the effect has triggered N times.
- **While-condition** — active as long as some condition holds (e.g. while a specific card is in play).

Lifetimes compose as **OR**: the effect expires when the first of its conditions is met. Example: "trigger once within the next two turns" combines trigger-count (N=1) and turn-timer (N=2).

### State and Static Effects

Temporary state changes (conditions, timed modifiers, etc.) are always expressed by spawning a static effect with an appropriate lifetime. The engine iterates active static effects each tick, resolving expirations and removing their state contributions automatically. **Effects** may make permanent state contributions directly (e.g. damage from an attack), but anything that needs to expire must go through a static effect.

---

## Actions and Turn Structure

The atomic action unit is **activating an effect block**. "Playing a card" is one way to do this. The engine does not privilege any particular action type.

### Action Inputs
Before an action resolves, the player (or AI) must provide all **binding-time inputs** — inputs declared by the effect block's signature that must be known before execution begins. Types include:
- **Targets** — one or more game entities meeting declared criteria
- **Cost payment choices** — how to pay when multiple payment options exist
- **Variable values** — e.g. choosing X

Binding-time inputs are validated before execution starts. If validation fails (e.g. no valid targets exist), the action cannot be taken at all. Once all binding-time inputs are provided and validated, the action scope is created and the effect block resolves with those inputs bound into it.

### Resolution Semantics
Once binding succeeds, resolution does not fail. If an effect cannot fully execute (e.g. "discard 2 cards" when only 1 is in hand), it does as much as it can. There is no rollback or panic — partial execution is acceptable.

### Prompts
A prompt is a **required input** initiated by the game rather than the player. It blocks game progression until answered.

Prompts come in two kinds:

**Phase prompts** — generated by phase init/cleanup rules (e.g. "discard to hand size at end of turn"). These are discrete actions initiated by the engine between player action windows.

**Mid-effect prompts** — some keywords, during execution, require the player to make a choice that cannot be known at binding time (e.g. "discard 2 cards of your choice" — which cards to discard is chosen mid-execution). The effect block pauses at that keyword, waits for the player's response, binds it into the remaining block, and continues. Prompts only bind variables — they do not change state or initiate new actions — so block atomicity is preserved: no triggers or state-based effects resolve while waiting for a mid-effect prompt.

**Short-circuit rule**: if the number of valid candidates is less than or equal to the number of choices required, the prompt is skipped and all candidates are selected automatically (no player input required).

### Turn Phases
Turns are composed of phases. Each phase has the structure:

```
phase
  ├── init        (effect block — set up the phase, e.g. draw a card)
  ├── wait        (player action window)
  └── cleanup     (effect block — teardown, e.g. hand size check)
```

Init and cleanup are effect blocks defined by the game creator. Either may be empty. Specific phases, their order, and what their init/cleanup blocks do are defined by the game creator, not the engine.

### Game Rules

The engine provides three mechanisms for game creators to define game-level rules:

**1. Phase init/cleanup** — effect blocks attached to a phase's lifecycle. Used for things like drawing a card at the start of upkeep, or triggering a hand size check at end of turn.

**2. Action rules** — middleware that wraps a named action type with before/after effect blocks. The payload (the action's own effect block) resolves in the middle. Example: a "play card" action rule might move the card to the stack before resolution, then move it to the discard pile after. Multiple action rules may apply to the same action type. Game creators address action rules by action name.

**3. State-based rules** — effect blocks that run automatically after every effect block resolves. The engine re-runs all state-based rules repeatedly until none trigger (i.e. until game state is stable). Used for things like:
- "If a unit's damage ≥ its health, destroy it"
- "If a player meets the loss condition, they lose"
- "If a player meets the win condition, they win"

Win and loss conditions are state-based rules defined by the game creator — they are not a separate engine concept.

**Infinite loop prevention**: state-based rules run to stability, so game creators are responsible for writing convergent rules (rules that, once applied, do not immediately re-trigger themselves).

---

## Built-in Keywords

The engine provides a small set of primitive keywords — one per state type for mutation, and two for reading. All game-creator-defined keywords are ultimately composed on top of these.

### Mutation Primitives

The three state types (accumulators, modifiers, conditions/tags) are structurally different and require distinct primitives:

- **Accumulator primitive** — permanently adds to or subtracts from an independently tracked value (e.g. damage taken). No lifetime involved.
- **Modifier primitive** — adds an adjustment to a static property. Accepts an **inline lifetime specification**, so a game creator can express "give +2 attack until end of turn" in a single call without spawning a separate static effect. The engine manages cleanup automatically.
- **Condition/tag primitive** — applies a categorical or boolean state to an entity. Also accepts an inline lifetime specification.

Explicit removal primitives exist for all three, to support effects that cancel or dispel state contributions before their natural expiry.

**Ease of expression is a first-class requirement.** Game creators should be able to express temporary state changes inline and concisely. The exact primitive signatures are for the domain modeler to determine.

### Read Primitives

**`get-state(entity, field)`** — reads a mutable runtime state value (modifier, accumulator, or condition/tag) from an entity. All game-creator-defined properties that query runtime state are ultimately composed on top of `get-state`.

**`get-property(entity, field)`** — reads a static design-time property (e.g. base attack, mana cost) from an entity. All game-creator-defined properties that query static data are ultimately composed on top of `get-property`.

Game creators do not typically call primitives directly; they define named keywords (e.g. `take-damage`, `current-health`, `mana-value`) that compose them with appropriate arguments.

---

### Effect Blocks on Cards
A card may have multiple effect blocks. Each effect block on a card has:
- **Activation type**: directly activatable by the player, or triggered-only (fires only via a trigger, never directly)
- **Activation condition** *(directly activatable only)*: a condition that must be true for the player to activate it
- **Cost** *(directly activatable only)*: what must be paid to activate it, independent of the card's base cost

One effect block per card is designated the **primary effect block** — the one that fires when the card is played. The game creator is responsible for presenting and routing to secondary effect blocks (activated abilities, modal choices, etc.).

### Scope hierarchy
```
game
  └── turn
        └── phase
              └── action
                    └── effect block
```

Triggers resolve between actions, not mid-block.

---

## Zones

Zones are named containers that hold cards. The engine treats zones as pure containers — no inherent behaviour is prescribed. Games assign meaning to zones through rules and by defining properties on them.

Zones have the same data model as cards:
- **Static properties** — defined at design time, read-only during play (e.g. `max_size` on a hand zone).
- **State** — mutable runtime data, following the same contribution model (modifiers, accumulators, conditions/tags).

### Zone Grouping
A game creator must be able to group zones so that effects can reference the group (e.g. "while in play" may mean "in any of: battlefield, structure zone, ..."). The grouping mechanism is an architectural decision; the requirement is that zone membership can be evaluated against a logical group, not just a single zone.

### Zone Membership as a Criterion
Zone membership is a valid criterion for two things:
1. **Lifetime scope** — a static effect's while-condition can reference zone membership (e.g. "active while this card is in play").
2. **Effect scope** — an effect can target or apply to cards based on their current zone (e.g. "deal 1 damage to all cards in zone X").

---

## Costs

Costs are **effects** — state mutations defined and implemented by the game creator. The engine does not define any first-class resource types. Game creators define whatever cost effects their game needs.

Costs are special in three ways:

1. **All-or-nothing semantics** — unlike regular effects (which do as much as they can), a cost must fully resolve or it does not apply at all. There is no partial cost payment.

2. **Bind-time validation** — at binding time, the engine performs a dry run of the cost to confirm it can be paid. If validation fails, the action cannot be taken.

3. **Resolution order** — costs resolve before the main effect block. The events generated by cost resolution are in scope for the main effect block and can be used as parameters. Example: a card costs "discard a card," then deals damage equal to the discarded card's mana value — the discard event is visible to the damage effect.

### Isolation

No engine-level "stack" or "resolving" zone is required. If a game needs to prevent a card from paying for itself (e.g. a "discard a card from hand" cost targeting the card being played), the game creator's action rule for "play card" can move the card out of its zone before costs resolve.

---

## Trigger Resolution Order

When a single event causes multiple static effects to trigger simultaneously, triggers resolve in **source lifetime order — oldest static effect first**. This gives the engine a deterministic, predictable resolution order without requiring player input.

---

## Card Sets and Pool

A **card set** is a named collection of card definitions. Sets may be thematically or mechanically grouped, but that is a game-level concern — the engine imposes no structure beyond the name and membership.

The **card pool** is the union of all card sets available to a game.

---

## Players

A player is a first-class engine entity with the same state model as cards: modifiers, accumulators, and conditions/tags, all contribution-tracked and engine-managed.

The engine targets **two participants** — one human player and one AI. Games are not required to be symmetrical; the game creator determines each participant's starting state, available actions, and rules.

**Ownership** is a first-class concept. Game entities (cards, zones, etc.) have an owner. Ownership is a valid criterion in effects and targeting (e.g. "discard one of your own cards"). The ownership model should be general enough that a game creator could extend it to support multiplayer without engine changes.

The distinction between *owner* and *controller* (e.g. a card temporarily controlled by the opponent) is noted as potentially necessary but is deferred to the domain modeler.

---

## Open Topics

- ~~Card types and how type affects play rules~~ *(resolved: game-level concern, not engine)*
- ~~Targeting — how targets are declared using properties, chosen, and validated~~ *(resolved: binding-time inputs declared by effect signature; mid-effect prompts for execution-time choices; short-circuit rule)*
- ~~Costs — what resources exist, how costs are paid and enforced~~ *(resolved: costs are effects; game-creator defined; all-or-nothing; bind-time dry run; resolve before main block)*
- ~~Turn structure — phases, whose turn it is, action limits~~ *(resolved: phase init/wait/cleanup structure; action inputs; prompts)*
- ~~Win and loss conditions~~ *(resolved: state-based rules defined by game creator)*
- ~~Zones — where cards/units live and how they move between zones~~ *(resolved: named containers; zone grouping and membership as targeting/lifetime criterion)*
- ~~Static effect lifetime and cleanup~~ *(resolved: engine-managed, tracked by source)*
- ~~Trigger resolution order when multiple triggers fire simultaneously~~ *(resolved: see Trigger Resolution Order section)*
- ~~Multiplayer — number of players, teams, etc.~~ *(resolved: see Players section)*
- ~~Card set and pool organization~~ *(resolved: see Card Sets and Pool section)*
- ~~The UI/tool layer for defining keywords and cards~~ *(resolved: see Tool Layer section)*

---

## Tool Layer

The tool layer is for **game creators**, not players. Its purpose is to produce all structured data needed to define a game built on the engine.

### Authoring Scope

The tool must support authoring of:
- **Keyword definitions** — game creators define their own effect and property keywords, effectively creating the language for their game
- **Cards** — art, static properties, and effect blocks
- **Game rules** — action rules, state-based rules
- **Phases** — phase definitions including init and cleanup effect blocks
- **Card sets** — named collections grouping cards into sets

### Authoring Modality

Effect blocks and keyword definitions are authored in a **DSL with an editor**. The editor must provide **autocomplete** driven by the type system (available keywords, valid parameter types, in-scope properties, etc.). Other structured data (static properties, art, phase ordering) may use a form-based or GUI interface.

### Set Analysis

The tool must include **set analysis tooling** to give game creators an overview of a card set for balancing purposes. The specific metrics and views are to be determined, but the goal is making it easy to spot imbalances across a set.
