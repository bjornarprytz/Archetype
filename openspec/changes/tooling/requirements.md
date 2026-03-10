---
status: draft
owner: requirements-analyst
last-updated: 2026-03-10
depends-on:
  - docs/requirements.md
  - docs/domain-model.md
  - src/Archetype.Core/GameDefinition.cs
  - src/Archetype.Core/StaticEffects.cs
  - src/Archetype.Core/Keywords.cs
  - src/Archetype.Core/CostDef.cs
---

# Tooling Requirements

## Purpose

The authoring tool is a standalone desktop application for game creators. Its primary job is to make it fast and ergonomic to author complete game definitions — keywords, cards, card sets, and localization — and to export those definitions as both a serialized game definition file and generated Godot GDScript classes ready for UI prototyping.

The tool is not an engine development environment. Engine developers work in the C# codebase. The tool is the game creator's workspace.

---

## Guiding Principles

- **Joy to use.** The tool should feel fast and fluid. Keyboard-driven operation is a first-class requirement — the full authoring workflow should be achievable without reaching for the mouse.
- **The definition is a graph.** Every element of a game definition (keywords, cards, zones, phases, rules, localization strings) is a node in a graph. Any node should be reachable from any other node. Navigation is bidirectional and frictionless.
- **Save freely, block output.** The tool never blocks saving, even when the definition contains errors. Output generation (game definition export, Godot class generation) is blocked until the definition is clean.
- **Complete coverage.** The tool must be able to produce every field of a `GameDefinition` — no field should require hand-editing the output JSON.
- **Lightweight.** The tool should start quickly and avoid unnecessary complexity. It is not a heavy IDE.

---

## Platform

- Standalone desktop application (Windows / Mac / Linux).
- Preferred technology direction: web-based stack (HTML/CSS/TypeScript, Electron or equivalent). XAML-based frameworks (Avalonia, WPF) are explicitly excluded.
- Godot editor plugin is not ruled out but is low priority due to Godot version stability concerns.
- Final platform/framework decision is deferred to the architect.

---

## Authoring: Keywords

Keywords are the foundational unit of a game definition. The keyword editor must be fast and first-class.

### Creation and Editing
- A game creator can create a new keyword from anywhere in the tool — not just from a dedicated keyword list.
- Keyword definitions are authored in the DSL. The editor provides:
  - **Syntax validation** inline as you type — errors are flagged immediately, not on save.
  - **Autocomplete** driven by the type system: available keywords, valid parameter types, in-scope expressions.
  - **Undefined keyword references** are allowed while authoring — a game creator can reference a keyword by name before defining it. The tool flags it as unresolved but does not block editing.
  - A **shortcut to create or navigate to** the referenced keyword from the call site (e.g. Cmd+Click or a quick-action).

### Impact Propagation
- When a keyword signature changes (name, parameter types, return type), the tool immediately surfaces all affected call sites across the entire definition.
- Conflicts are shown:
  - **Inline** — at the affected call site in the editor.
  - **Problems panel** — a dedicated panel listing all unresolved errors across the definition, navigable by keyboard.
- The game creator can save at any time regardless of unresolved conflicts. Output generation is blocked until the problems panel is empty.

### Graph Navigation
- From a keyword definition, the game creator can navigate to:
  - All **cards that use it** (directly or transitively).
  - All **composite keywords it is composed into**.
  - All **primitive keywords it composes** (its dependencies).
- This navigation is available by keyboard shortcut and requires no manual search.

---

## Authoring: Cards

### Card-First Workflow
- A game creator can start with a card idea, typing keyword invocations for keywords that do not yet exist. The tool flags unresolved references but does not block card creation.
- From an unresolved keyword reference on a card, there is a direct shortcut to create the keyword definition and return.

### Card Fields
Each card (`CardDefinition`) has:
- **Name** — localizable string.
- **Static properties** — game-creator-defined key/value fields (e.g. base attack, mana cost). Values are set per card. The tool must support adding, naming, and setting values for arbitrary static properties.
- **Primary effect block** — the effect block that fires when the card is played. Authored in the DSL editor with the same inline validation and autocomplete as keyword definitions.
- **Additional (named) effect blocks** — zero or more named, activatable effect blocks (e.g. activated abilities). Each has a name, an optional activation condition, an optional cost, and a body. All authored in the DSL editor.
- **Static effects** — zero or more static effect definitions attached to the card. Each specifies a lifetime, an optional state contribution block, an optional trigger, and an optional parameter modification. See Static Effects section below.
- **Activation condition** — an optional boolean expression (authored in the DSL) that guards whether the card's primary effect block can be played at all.
- **Cost** — zero or more cost definitions for the primary effect block. Each cost has a body (effect block), optional parameters, and an optional text template. See Costs section below.
- **Art** — optional. See Art section below.
- **Flavour text** — optional localizable string. (See note on domain model below.)

### Art
- Art is optional. A card can be fully defined and exported without art.
- The game creator specifies a source image file (stored by path).
- The tool provides a **crop tool**: the game creator selects a rectangular region of the source image. The cropped region is what gets exported — not the full source image.
- Art assets (cropped images) are bundled into the game definition export, not referenced by path at runtime.

### Costs

A cost on a card or ability is a `CostDef`. The tool must support authoring:
- **Body** — an effect block (authored in the DSL) that pays the cost and may contain `assert` calls to express un-affordability.
- **Parameters** — optional typed parameters for player-provided choices at activation time (e.g. which card to discard). Each parameter has a name and a type drawn from the engine's type vocabulary.
- **Text template** — an optional localizable string describing the cost for player-facing display, with `{paramName}` placeholders. If absent, the engine falls back to structural rendering.

### Static Effects

A static effect definition (`StaticEffectDef`) on a card has:
- **Lifetime** — a `LifetimeSpec` composed from turn timer, trigger count, and/or while-condition (see domain model). The tool provides a structured editor for composing lifetimes.
- **State contribution block** — optional effect block that applies state contributions (modifiers, conditions) when the static effect is active.
- **Trigger** — optional. Specifies the event keyword to listen for, the trigger scope (`ThisAction`, `ThisTurn`, `ThisGame`), event parameter declarations, an optional filter condition, event bindings (mapping event args to block variables), and the effect block to fire. All authored in the DSL or structured form.
- **Parameter modification** — optional. Either a `ParameterAdjustment` (intercepts a keyword invocation and adjusts its arguments: additive, multiplicative, or replace) or a `Disable` (cancels the invocation entirely). Specifies the target keyword, an optional argument filter, an optional filter condition, and the modification expression(s).

---

## Authoring: Card Sets

- A card set is a named collection of card definitions.
- The tool supports creating, naming, and populating card sets.
- A card may belong to one or more sets (or none during early design).

---

## Authoring: Zones

Zone definitions (`ZoneDefinition`) are game-creator-defined named containers. The tool must support:
- Creating and naming zone definitions.
- Adding arbitrary **static properties** (key/value fields) to a zone definition (e.g. `max_size`, `is_hidden`).

Zone definitions are design-time data only. Runtime zone state is configured in the initial game state (see Initial Game State section).

---

## Authoring: Player Definitions

Player definitions (`PlayerDefinition`) describe the static properties of a player role (e.g. "player", "opponent"). The tool must support:
- Creating named player definitions.
- Adding arbitrary **static properties** to each player definition.

Initial mutable player state (accumulators, conditions) is configured in the initial game state.

---

## Authoring: Phases and Turn Structure

Turn structure is defined as an ordered list of `PhaseDefinition` records. The tool must support:
- Creating, naming, reordering, and deleting phases.
- For each phase: authoring an optional **init** effect block and an optional **cleanup** effect block, both in the DSL editor.
- The turn structure must be visually clear — the game creator should be able to see the full phase sequence at a glance and understand what happens in each phase's init and cleanup.

---

## Authoring: Game Rules

### Action Rules

Action rules (`ActionRuleDefinition`) wrap a named action type with before/after effect blocks. The tool must support:
- Associating one or more action rules with a named action type (e.g. `"play-card"`, `"end-turn"`).
- For each rule: authoring an optional **before** effect block and an optional **after** effect block in the DSL editor.
- Multiple rules per action type are allowed and must be orderable.

### State-Based Rules

State-based rules (`StateBasedRule`) fire automatically when a condition holds, repeating until a fixpoint is reached. The tool must support:
- Creating, naming, and deleting state-based rules.
- For each rule: authoring a **condition** expression (boolean, in the DSL) and a **body** effect block (in the DSL).
- The evaluation order of state-based rules is meaningful (they run in registration order). The tool must allow the game creator to see and reorder them.

### Trigger Resolution Order

The game definition carries a `TriggerResolutionOrder` setting that determines how simultaneous triggers are sequenced. The tool must allow the game creator to choose one of:
- `OldestFirst` — oldest active static effect fires first (default).
- `OldestLast` — newest fires first.
- `PromptPlayer` — the affected player chooses the order at runtime.

---

## Authoring: Initial Game State

The `InitManifest` (default initial game state) declares the starting state of a game session — the zones, cards, and player state that exist before the first phase runs. The tool must support authoring:

### Zone Instances
Each `ZoneSpec` declares a zone to create at session start:
- **Local ID** — a name used to reference this zone instance in card specs.
- **Owner** — which player owns this zone (by player definition name).
- **Definition** — which `ZoneDefinition` to instantiate.
- **Initial accumulators** — optional key/value pairs setting starting accumulator values on the zone.
- **Initial conditions** — optional list of condition names applied to the zone at start.

### Card Instances
Each `CardSpec` declares a card to create at session start:
- **Owner** — which player owns this card.
- **Zone** — which zone instance (by local ID) the card starts in.
- **Definition** — which `CardDefinition` to instantiate.
- **Initial accumulators** — optional starting accumulator overrides.
- **Initial conditions** — optional starting conditions.

### Player State
Each `PlayerStateSpec` declares the starting mutable state for one player:
- **Player** — which player definition this applies to.
- **Initial accumulators** — e.g. starting health, starting mana.
- **Initial conditions** — e.g. starting tags or status conditions.

### Game Definition Identity
Each game definition has a unique **ID** (a non-empty string). The tool must allow the game creator to set this. The ID is stored in save snapshots and used to validate that a loaded save matches the current definition.

### Playable Zone Names
The game definition carries an optional list of **playable zone names** — the zones from which cards may be played (e.g. `"hand"`). The tool must allow the game creator to configure this list. If left empty, no zone filter is applied.

---

## Authoring: Game Definition

The tool supports authoring all elements of a complete `GameDefinition`:
- Game definition ID and playable zone names
- Keyword definitions
- Card definitions (with static effects, costs, activation conditions, additional effects)
- Zone definitions
- Player definitions
- Card sets
- Phase definitions and turn structure
- Action rules and state-based rules
- Trigger resolution order
- Default initial game state (`InitManifest`)
- Localization strings (see Localization section)

No field of `GameDefinition` should require the game creator to hand-edit the exported JSON.

---

## Localization

### Source Language
- The game creator designates one language as the **source language**. This can be any natural language (e.g. English) or a set of text-IDs (e.g. `card.fireball.name`, `kw.take_damage.text`). Text-IDs as source are fully supported.
- All other languages are translations derived from the source.

### Scope
Localization covers all game-design text:
- Card names
- Card rules text (rendered from effect block text templates)
- Keyword names
- Keyword rules text (text templates)
- Flavour text
- Any other game-creator-defined text associated with cards, keywords, or sets

Tool UI strings (menus, buttons, panel labels) are out of scope — those are the tool's own responsibility, not part of the game definition.

### Workflow
- Author in the source language first.
- Translations are managed as a separate pass — the tool presents a translation view showing source strings alongside editable target strings.
- Missing translations are flagged in the problems panel but do not block saving. Whether missing translations block export is configurable (game creator decides at export time whether a partial translation is acceptable).

---

## Set Overview and Keyword Graph

The tool provides a **set overview** for balancing and design review:

- **Keyword distribution** — across a selected card set, how many cards use each keyword.
- **Keyword composition graph** — a visualization of which keywords compose which, showing the full dependency graph across the game definition.

Deeper analytical features (mana curve analysis, numerical stat distribution, simulation, playtest log review) are deferred. They are acknowledged as desirable but depend on game-definition structure that varies too much to specify now.

---

## Card Text Preview

The tool includes a **preview mode** for rendered card text. A game creator can see how a card's effect block renders as human-readable text, using the text templates defined on keywords. This is a read-only preview — not interactive simulation. Interactive playtesting is done in Godot.

---

## Output and Export

### Validation Gate
Export is blocked until the definition is clean (problems panel empty). Save is always available regardless of definition state.

### Game Definition Export
- Produces a serialized game definition file (JSON, per the existing architecture).
- Includes all keywords, cards, card sets, phase definitions, game rules, and localization strings.
- Cropped art assets are bundled into the export package.

### Generated Godot GDScript Classes (per-game-definition)
Generated once per export from the game definition. These files are never hand-edited — they are always derived from the definition.

For each major domain type in the game definition (Card, Zone, Session, Player, and any game-creator-defined variants), the tool generates a GDScript class with:
- **Properties** reflecting the type's static properties and runtime state fields as declared in the definition.
- **Signals** wired to the engine's event log — for example, a `on_damage` signal that fires when the `take_damage` keyword appends its event. The signal set is derived from the keywords used in the game definition.

### GDScript Engine Interop Wrapper (one-time)
A GDScript wrapper that bridges GDScript to the C# engine API (Archetype.Core / Archetype.Engine). This is generated from the engine's public C# API surface, not from the game definition. It is generated once and checked in; it needs regeneration only when the engine's public API changes.

### Card Importer
A Godot-side GDScript utility (generated as part of the per-game-definition output) that:
- Instantiates and hydrates a card class from a concrete card/zone/session definition.
- Loads the designated art asset for the card (if present).

The goal is that a game creator can export a definition and immediately begin prototyping a Godot UI against the generated classes, with minimal manual wiring.

---

## Open Items / Deferred

- **Platform/framework** — deferred to architect. Preference: Electron or web-based stack. No XAML.
- **Deeper balancing analytics** — deferred. Numerical stat distribution, mana curve, simulation, playtest log review are acknowledged as desirable but depend on game-definition specifics. To be revisited once a concrete game definition exists.
- **Flavour text domain model** — flavour text is referenced here as a per-card optional localizable string but is not yet in `docs/domain-model.md`. The domain modeler must add it before this requirements document can be signed off.
- **Signal derivation rules** — exactly which engine events map to which GDScript signals, and how the tool derives the signal set from keyword definitions, is not yet specified. This is an architect-level decision.
- **Export package format** — whether art assets are bundled as a zip, a folder, or embedded in the JSON is an architect-level decision.
- **Missing-translation export gate** — whether missing translations block export is described as configurable, but the exact UX for this configuration is not specified.
- **Lifetime editor UX** — `LifetimeSpec` can compose turn timer, trigger count, and while-condition as OR. The exact form-based or DSL UI for this is not yet specified; an architect or UX pass is needed.
- **Static property schema** — `CardDefinition.StaticProperties` and `ZoneDefinition.StaticProperties` are untyped `Dictionary<string, object>` in the engine. The tool needs a way for the game creator to declare what static property names and types exist for a given definition type. Whether this is a per-game schema declaration or inferred from existing values is not yet specified.
- **InitManifest UX** — the tool must present an ergonomic view of zone instances, card instances, and player state as a single "starting board state." The exact layout is not specified here; it is a UX/design decision for the architect.
- **Multiple action rules per action type ordering** — the engine runs multiple rules per action type in registration order. The tool must allow reordering them, but the exact UX for this is not yet specified.
