---
status: signed-off
owner: requirements-analyst
signed-off: 2026-03-11
last-updated: 2026-03-11
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
- **Static properties** — key/value fields declared in the game's static property schema (e.g. `base_attack`, `mana_cost`). Values are set per card. The tool presents exactly the properties declared in the schema for Cards; new properties are added via the schema editor, not per-card (see Static Property Schema section).
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
- **Lifetime** — a `LifetimeSpec` composed from turn timer, trigger count, and/or while-condition (see domain model). The tool provides a **mini-DSL field** for authoring the lifetime (see Lifetime Editor section below).
- **State contribution block** — optional effect block that applies state contributions (modifiers, conditions) when the static effect is active.
- **Trigger** — optional. Specifies the event keyword to listen for, the trigger scope (`ThisAction`, `ThisTurn`, `ThisGame`), event parameter declarations, an optional filter condition, event bindings (mapping event args to block variables), and the effect block to fire. All authored in the DSL or structured form.
- **Parameter modification** — optional. Either a `ParameterAdjustment` (intercepts a keyword invocation and adjusts its arguments: additive, multiplicative, or replace) or a `Disable` (cancels the invocation entirely). Specifies the target keyword, an optional argument filter, an optional filter condition, and the modification expression(s).

### Lifetime Editor

`LifetimeSpec` is authored in a single **mini-DSL text field**. The three components — turn timer, trigger count, and while-condition — are combined with `|` (OR semantics). Any subset may be used. Examples:

```
2 turns
1 trigger
while in_play(this)
2 turns | 1 trigger
while owner.has_condition(shielded) | 3 triggers
```

Syntax reference (each component is optional; combine any with `|`):

| Component | Syntax |
|---|---|
| Turn timer | `<N> turns` |
| Trigger count | `<N> trigger` / `<N> triggers` |
| While-condition | `while <bool-expr>` |

The field provides:
- **Inline syntax validation** — errors flagged as you type.
- **Autocomplete** — keyword names, atom accessors, and scope variables offered at the cursor position.
- **Scope panel** — displayed adjacent to the field, listing every variable and atom in scope at the point of authoring the while-condition (e.g. `this: CardInstance`, `owner: PlayerInstance`, event-bound variables if inside a trigger). The scope panel updates as the game creator navigates between static effects. This makes the available expression vocabulary visible without requiring the creator to look elsewhere.
- **Parsed preview** — a read-only structured summary of the parsed lifetime shown below the field (e.g. "Expires after 2 turns OR when triggered 1 time"), confirming the creator's intent.

---

## Authoring: Card Sets

- A card set is a named collection of card definitions.
- The tool supports creating, naming, and populating card sets.
- A card may belong to one or more sets (or none during early design).

---

## Authoring: Zones

Zone definitions (`ZoneDefinition`) are game-creator-defined named containers. The tool must support:
- Creating and naming zone definitions.
- Setting **static property values** per zone definition, from the properties declared in the game's static property schema (see Static Property Schema section).

Zone definitions are design-time data only. Runtime zone state is configured in the initial game state (see Initial Game State section).

---

## Authoring: Player Definitions

Player definitions (`PlayerDefinition`) describe the static properties of a player role (e.g. "player", "opponent"). The tool must support:
- Creating named player definitions.
- Setting **static property values** per player definition, from the properties declared in the game's static property schema.

Initial mutable player state (accumulators, conditions) is configured in the initial game state.

---

## Authoring: Static Property Schema

`CardDefinition.StaticProperties`, `ZoneDefinition.StaticProperties`, and `PlayerDefinition.StaticProperties` are backed by a **per-game schema declaration** — the single source of truth for what property names and types are valid for each entity kind.

The game creator maintains a schema per entity kind (Cards, Zones, PlayerDefs). Each schema entry declares:
- **Name** — the property key (e.g. `mana_cost`, `is_legendary`).
- **Type** — one of the engine's supported value types (`int`, `float`, `bool`, `string`).
- **Default value** — optional. Applied to all existing and future entities of that kind that do not explicitly set the property.

The tool enforces the schema at edit time:
- Every card/zone/player definition editor shows exactly the properties declared in the schema for that entity kind — no more, no less.
- Type-appropriate controls are used (number input for `int`/`float`, checkbox for `bool`, text field for `string`).
- Values that deviate from the declared type are flagged as errors in the problems panel.

**Adding a property to the schema is a deliberate, explicit action:**
- There is a dedicated schema editor (one per entity kind) reachable from the main navigation. It lists all declared properties and provides an explicit "Add property" control.
- A property is never auto-created from a value entry — there is no creation-on-typo or creation-on-paste behaviour.
- The "Add property" flow always prompts for name and type before the property appears anywhere. The property is created only after the game creator confirms both fields.
- From within an individual card/zone/player editor, a shortcut can invoke the same "Add property to schema" flow — but confirmation and type selection are always required; there is no one-click auto-creation.

Schema changes propagate immediately:
- Adding a property adds a blank or default-valued slot to all existing entities of that kind.
- Removing a property removes it from all entities of that kind; the tool shows a confirmation warning that lists the affected entities and the values that will be discarded before proceeding.

---

## Authoring: Phases and Turn Structure

Turn structure is defined as an ordered list of `PhaseDefinition` records. The tool must support:
- Creating, naming, reordering, and deleting phases.
- For each phase: authoring an optional **init** effect block and an optional **cleanup** effect block, both in the DSL editor.
- The turn structure must be visually clear — the game creator should be able to see the full phase sequence at a glance and understand what happens in each phase's init and cleanup.

---

## Authoring: Game Rules

### Action Rules

Action rules (`ActionRuleDefinition`) wrap a named action type with before/after effect blocks. The tool presents action rules grouped by action type in an **accordion**: one collapsible section per named action type (e.g. `"play-card"`, `"end-turn"`). Within each section, the rules for that action type are listed in execution order.

The tool must support:
- Creating a new action rule by naming an action type — if the type already has a section, the rule is appended to it; if not, a new section is created.
- For each rule: authoring an optional **before** effect block and an optional **after** effect block in the DSL editor.
- **Reordering** rules within an action type section by drag handle or up/down keyboard shortcut. The displayed order is the execution order. Rules for different action types cannot be interleaved — order is meaningful only within a type's group.
- Renaming and deleting individual rules, and deleting an entire action type section (with confirmation if it contains rules).

### State-Based Rules

State-based rules (`StateBasedRule`) fire automatically when a condition holds, repeating until a fixpoint is reached. The tool presents state-based rules in an **ordered list** using the same accordion-free variant of the same pattern: rules are listed in evaluation order with a drag handle or up/down keyboard shortcut for reordering.

The tool must support:
- Creating, naming, and deleting state-based rules.
- For each rule: authoring a **condition** expression (boolean, in the DSL) and a **body** effect block (in the DSL).
- **Reordering** rules; the displayed order is the evaluation order (registration order).

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

### InitManifest Layout

The InitManifest editor uses a **player-scoped accordion with a neutral zones section at the top**. The layout is text-based — no spatial board visualization.

Structure:

```
[ Neutral / Shared Zones ]          ← top-level collapsible section
  Zone: <local-id> (<ZoneDefinition>)
    accumulators: ...
    conditions: ...
    Cards:
      <CardDefinition> (owner: <player>)
      ...

[ Player: <name> ]                  ← one collapsible section per PlayerDefinition
  Player State                      ← collapsible sub-header
    accumulators: ...
    conditions: ...
  Zone: <local-id> (<ZoneDefinition>)
    accumulators: ...
    conditions: ...
    Cards:
      <CardDefinition>
      ...
```

Interaction model:
- Zones with no owner appear in the Neutral section; zones with an owner appear under that player's section.
- Cards are shown inline under the zone they start in. A card's owner is displayed but does not affect placement — a card owned by Player 1 may start in a shared zone and is shown there.
- Adding a card is an explicit inline action within a zone row (e.g. an "Add card" control that opens a card-definition picker). Cards are never auto-populated.
- Adding a zone is an explicit action within a player section or the Neutral section.
- Reordering zones within a player's section is supported (drag handle or up/down keys). Card order within a zone is also reorderable (relevant for draw-pile top-to-bottom order).
- All fields (accumulators, conditions, owner, zone definition, card definition) are editable inline.
- The accordion state (expanded/collapsed per section) is persisted per session.

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

## Design Decisions Arising from Tooling Requirements

The following decisions were made during tooling requirements elicitation. They affect the engine API (D14) and must be actioned by the architect as a D14 addendum before implementation begins. They are recorded here so the pipeline has a single authoritative source.

### InitManifest is mandatory and append-only

**Decision:** `InitManifest` is required on every `GameDefinition`. It is not nullable. `Build()` throws if it is absent (even an empty manifest — empty lists — is acceptable). The host cannot replace or skip it.

**Rationale:** The `InitManifest` provides invariants that game rules can rely on — zones exist, player state is initialized, known cards are present. A host that replaces the manifest entirely can silently remove structure that authored rules depend on. The append-only model preserves that invariant unconditionally.

**Impact on D14:**
- `GameDefinition.DefaultInitManifest : InitManifest?` becomes `InitManifest : InitManifest` (required, not nullable; field renamed to drop "Default" since it is no longer optional).
- `GameSessionBuilder` loses `.UseDefaultInit()` (no longer a choice — InitManifest is always applied) and loses `.WithInitManifest(InitManifest)` / `.WithInitManifest(Action<ManifestBuilder>)` (the replacement-mode overloads).
- The "if none is called the session begins with no atoms" escape hatch is removed.

### HostManifest — session-time append layer

**Decision:** The host may supply a `HostManifest` at session build time. The engine applies `InitManifest` first, then appends `HostManifest` entries on top. `HostManifest` may contain both zones and cards.

**Rationale:** The primary use case is variable-deck games: the game definition declares zones and player state in `InitManifest`; the host appends the player's chosen cards into the draw-pile zone at session time. Allowing the host to also append zones (not just cards) is preserved as fertile design space — for example, a session-specific "draft table" zone that only exists for certain game modes.

**Impact on D14:**
- New type `HostManifest { Zones: IReadOnlyList<ZoneSpec>, Cards: IReadOnlyList<CardSpec> }` (same `ZoneSpec`/`CardSpec` shapes as `InitManifest`).
- `GameSessionBuilder` gains `.WithHostManifest(HostManifest)` and `.WithHostManifest(Action<HostManifestBuilder>)`. These are optional; omitting them means no host additions.
- Provisioning order gains a step after `InitManifest` provisioning: host manifest zones are created, then host manifest cards are placed (referencing `LocalId`s from either `InitManifest` or `HostManifest` zones).

### LocalId uniqueness rule

**Decision:** `LocalId` uniqueness is enforced across the union of `InitManifest.Zones` and `HostManifest.Zones`. A `HostManifest` zone `LocalId` that collides with any `InitManifest` zone `LocalId` is a `SessionException` at `.Build()` time. No namespace prefixing or reserved ranges are required — a single uniqueness check across both sets is sufficient. `HostCardSpec.ZoneLocalId` may reference a `LocalId` from either `InitManifest` or `HostManifest`.

---

## Open Items / Deferred

### Resolved UX Items (all four closed)

- ~~**Lifetime editor UX**~~ — resolved. See Lifetime Editor section under Authoring: Cards.
- ~~**Static property schema**~~ — resolved. See Static Property Schema section.
- ~~**InitManifest UX**~~ — resolved. See InitManifest Layout section under Authoring: Initial Game State.
- ~~**Multiple action rules per action type ordering**~~ — resolved. See Action Rules section under Authoring: Game Rules.

### Deferred to Architect

The following are explicitly deferred. They are acknowledged requirements, not gaps — the architect is responsible for specifying them.

- **Platform/framework** — deferred to architect. Stated preference: Electron or web-based stack (HTML/CSS/TypeScript). No XAML. Final decision is the architect's.
- **Signal derivation rules** — exactly which engine events map to which GDScript signals, and how the tool derives the signal set from keyword definitions, is not yet specified. Architect-level decision.
- **Export package format** — whether art assets are bundled as a zip, a folder, or embedded in the JSON is an architect-level decision.
- **Missing-translation export gate** — described as configurable (game creator decides at export time). The exact UX for this configuration is not specified here; architect to specify.
- **D14 addendum required** — the InitManifest mandatory/append-only and `HostManifest` decisions (see Design Decisions section) must be written into `docs/architecture.md` as a D14 addendum by the architect before implementation.

### Deferred to Domain Modeler

- **Flavour text domain model** — flavour text is referenced here as a per-card optional localizable string but is not yet in `docs/domain-model.md`. The domain modeler must add it before this requirements document can be signed off.

### Deferred / Acknowledged as Out of Scope

- **Deeper balancing analytics** — numerical stat distribution, mana curve analysis, simulation, playtest log review are acknowledged as desirable but depend on game-definition structure that varies too much to specify now. To be revisited once a concrete game definition exists.
