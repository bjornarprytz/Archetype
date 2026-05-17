---
name: archetype-cards
description: Use this skill when implementing card definitions for a game built on Archetype — when working with CardDefinition, CostDef, EffectBlockDef, ActivationCondition, StaticEffectDef, or state map declarations for card atoms.
version: 1.0.0
user-invocable: false
---

# Archetype — Card Authoring

A `CardDefinition` is the design-time blueprint for a card. It combines static read-only properties, runtime state declarations, cost definitions, activated effects, and static effects into a single structure.

---

## CardDefinition Structure

```csharp
var fireball = new CardDefinition(
    Name:                "Fireball",
    StaticProperties:    new Dictionary<string, object>
    {
        ["cost"]        = 3.0,
        ["color"]       = "red",
        ["type"]        = "spell",
    },
    StateMapDeclarations: [], // no per-instance runtime state needed for a spell
    Cost:                [energyCost],
    ActivationCondition: inHandCondition,
    PlayableZoneNames:   ["hand"],
    ActivatedEffects:    [fireballEffect],
    StaticEffects:       [],
    FlavourText:         "It doesn't just burn — it forgets you existed.");
```

All fields except `Name` and `StaticProperties` have sensible defaults (empty lists / null).

---

## Static Properties

Defined at design time. Read-only during play. Stored as `IReadOnlyDictionary<string, object>`.

- Values must be primitive JSON-serialisable types: `double`, `bool`, or `string`.
- The engine does not define property names — game creators choose them freely.
- Access at runtime via `get-state` (for accumulator/modifier-computed values) or by reading the atom's static property dictionary directly in host code.

```csharp
StaticProperties: new Dictionary<string, object>
{
    ["base-attack"] = 3.0,
    ["base-health"] = 5.0,
    ["name"]        = "Stone Golem",
    ["creature-type"] = "construct",
}
```

---

## State Map Declarations

State map declarations define which runtime state fields a card atom carries. Declared via `StateFieldDecl`:

```csharp
using Archetype.Build;

var stateMap = new StateMapBuilder()
    .AddAccumulator("damage",  defaultValue: 0.0)
    .AddAccumulator("counters", defaultValue: 0.0)
    .AddModifier("attack",  baseSource: "base-attack")
    .AddModifier("health",  baseSource: "base-health")
    .AddCondition("tapped")
    .AddCondition("sleeping")
    .Build();
```

| Field Type | What it tracks |
|---|---|
| Accumulator | An independently tracked value (e.g. damage taken, charge counters) |
| Modifier | An adjustment to a static property; engine computes `(base + additives) × multiplicatives` |
| Condition/Tag | A categorical or boolean state; may be applied multiple times with independent lifetimes |

**Contribution tracking**: every `apply-modifier` and `apply-condition` returns a `ContributionId`. The engine tracks the source of each contribution and automatically removes it when the contributing static effect expires. Game creators do not write cleanup logic for lifetime-bound contributions.

---

## Cost Model

A `CostDef` defines what a player must pay to play a card or activate an ability.

```csharp
// Cost: assert the player has >= 3 energy, then deduct 3.
var energyCost = new CostDefBuilder()
    .WithTextTemplate("Pay {amount} energy.")
    .WithParam("amount", TypeName.Number)
    .WithBody(new EffectBlockDef([
        new EffectBlockStep("assert", [
            new Invocation("at-least",
                new Invocation("get-state",
                    new Invocation("session"),
                    new Literal("energy")),
                new ParameterRef("amount")),
            new Literal((double)(int)OnFail.Panic),
            new Literal((double)(int)NotifyFlag.Off),
        ]),
        new EffectBlockStep("modify-accumulator", [
            new Invocation("session"),
            new Literal("energy"),
            new Literal(-3.0),
        ]),
    ]))
    .Build();
```

**Rules for cost bodies:**
- `assert` in a cost body always uses `OnFail.Panic` and `NotifyFlag.Off` — this is the affordability signal; the engine catches the panic and reports the card as unplayable.
- The cost body may also contain state-mutation steps that execute on payment (e.g. deducting a resource).
- Cost validation runs against a clone of game state; mutations in the clone are discarded. The real payment happens when the action is confirmed.
- `CardDefinition.Cost` is a list — multiple costs are validated sequentially and all must pass.
- An empty list means the card has no cost.

---

## Activated Effects

An activated effect is an `EffectBlockDef` — an ordered sequence of keyword invocations that executes atomically when the card is played or the ability is activated.

```csharp
var fireballEffect = new EffectBlockDef([
    // Deal 3 damage to target.
    new EffectBlockStep("take-damage", [
        new ParameterRef("target"),
        new Literal(3.0),
    ]),
    // If target has taken >= 5 total damage, destroy it.
    new EffectBlockStep("if-destroyed-by-damage", [
        new ParameterRef("target"),
        new Literal(5.0),
    ]),
]);
```

`EffectBlockStep(keywordName, args[])` — each arg is a `KeywordNode` (use `Kw.*` constructors or build nodes directly).

No triggers or state-based effects resolve mid-block. The block appends to the event log; triggers are resolved between actions, not between steps.

---

## Activation Condition

`ActivationCondition` is a `KeywordNode` tree that evaluates to a boolean. It determines whether a player can play this card at all. If null, the card is always playable from any listed zone.

```csharp
// Playable only if the player's mana accumulator >= the card's cost value.
ActivationCondition: new Invocation("at-least",
    new Invocation("get-state", new Invocation("session"), new Literal("mana")),
    new Literal(3.0))
```

- The engine does not filter by ownership. Ownership checks must be expressed as part of `ActivationCondition`.
- `PlayableZoneNames` lists the zones from which this card may be played. Cards in other zones are never offered as actions.

---

## Static Effects

A static effect is a standing condition with a lifetime. It contributes persistent modifiers, conditions, or triggers to game state for as long as it is active.

```csharp
var tauntEffect = new StaticEffectDef(
    // Lifetime: permanent (empty condition list = no expiry)
    Lifetime: new LifetimeSpec([]),
    // Trigger: fire the body whenever any card takes damage
    Trigger: new TriggerSpec("take-damage", predicate: null),
    // Body: apply "taunt" condition to all enemy creatures
    Body: new EffectBlockDef([...]),
    // Optional parameter modification: suppress or adjust specific keyword args
    ParameterModification: null);
```

### Lifetime types (combined as OR — any one expiring removes the effect)

| Type | When it expires |
|---|---|
| `TurnTimer(n)` | After N turns |
| `TriggerCount(n)` | After firing N times |
| `WhileCondition(expr)` | When `expr` evaluates to false |
| Empty list | Permanent — never expires |

### Declarative re-activation

A static effect with a `WhileCondition` lifetime is **declarative**: when the condition transitions from false → true, a fresh instance is created with new idatom and counters. Dynamic effects (non-declarative) expire permanently.

### Parameter modification

`ParameterModification` intercepts every dispatch of a specific keyword and adjusts or disables it:

```csharp
ParameterModification: new ParameterModificationSpec(
    TargetKeyword: "take-damage",
    Filter: null, // null = applies to all invocations of this keyword
    Adjustments: [
        new ParameterAdjustment("amount", AdjustmentKind.Additive, new Literal(-1.0))
    ])
```

`Disable` variant suppresses the keyword entirely and appends a `keyword-disabled` event.

---

## Registering Cards

```csharp
builder.RegisterCard(fireball);
builder.RegisterCard(stoneGolem);
```

Card definition names must be unique within the game definition.
