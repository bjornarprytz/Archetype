---
name: archetype-keywords
description: Use this skill when implementing or designing keywords for a card game built on Archetype — when working with KeywordDefinition, KeywordDefinitionBuilder, Kw.*, ParameterDecl, TextTemplate, or deciding whether a built-in primitive covers a mechanic.
version: 1.0.0
user-invocable: false
---

# Archetype — Keyword Authoring

Keywords are the primitive unit of expression in Archetype. Every rule, cost, condition check, and state change is a keyword invocation or composition of invocations. Follow this skill when defining any keyword.

---

## Keyword Subtypes

Every keyword is exactly one of:

| Subtype | Does | Returns |
|---|---|---|
| **Mutation keyword** | Changes game state | `Void` or a `ContributionId` |
| **Property keyword** | Reads and returns a value; never mutates | Any non-Void type |

Mutation keywords may call property keywords as arguments. Property keywords must never call mutation keywords.

---

## Primitive vs Composite

- **Primitive** — body is a single built-in invocation (or a sentinel marking it as engine-primitive). Do not re-implement built-ins.
- **Composite** — body invokes other keywords (built-ins or game-creator-defined) with derived arguments. This is how all game-creator keywords are defined.

---

## The Type System

`ParameterDecl` declares each parameter with a name and a `TypeName`:

| TypeName | Represents |
|---|---|
| `TypeName.Atom` | Any atom (Card, Zone, Player, or Session) |
| `TypeName.Card` | Shorthand — sets `AtomKindRestriction` to `[AtomKind.Card]` |
| `TypeName.Zone` | Shorthand — sets restriction to `[AtomKind.Zone]` |
| `TypeName.Player` | Shorthand — sets restriction to `[AtomKind.Player]` |
| `TypeName.Number` | A numeric value |
| `TypeName.Boolean` | A boolean value |
| `TypeName.ConditionName` | A string naming a condition/tag |
| `TypeName.PropertyName` | A string naming a property keyword |
| `TypeName.ContributionId` | A handle returned by `apply-modifier` / `apply-condition` |
| `TypeName.Lifetime` | A lifetime specification |
| `TypeName.EffectBlock` | An effect block passed as a higher-order argument |

To restrict an `Atom` parameter to specific atom kinds, use `AtomKindRestriction`:

```csharp
new ParameterDecl("target", TypeName.Atom,
    AtomKindRestriction: [AtomKind.Card, AtomKind.Player])
```

**Every keyword definition must declare:**
- `ReturnType` — explicit, non-null
- `Description` — human-readable string; required by the type system

---

## Building a Keyword

### Using `KeywordDefinitionBuilder` (preferred for game-creator code)

```csharp
using Archetype.Build;
using Archetype.Build.Extensions;
using Archetype.Core;

var takeDamage = new KeywordDefinitionBuilder("take-damage")
    .WithDescription("Deals damage to a target card.")
    .WithReturnType(TypeName.Void)
    .WithParam("target", TypeName.Card)
    .WithParam("amount", TypeName.Number)
    .WithBody(Kw.ModifyAccumulator(
        Kw.Param("target"),
        Kw.Str("damage"),
        Kw.Param("amount")))
    .WithTextTemplate("Deal {amount} damage to {target}.")
    .Build();
```

### Using the `KeywordDefinition` record directly

```csharp
var takeDamage = new KeywordDefinition(
    Name:         "take-damage",
    Description:  "Deals damage to a target card.",
    ReturnType:   TypeName.Void,
    Parameters:   [
        new ParameterDecl("target", TypeName.Card),
        new ParameterDecl("amount", TypeName.Number),
    ],
    Body:         Kw.ModifyAccumulator(
                      Kw.Param("target"),
                      Kw.Str("damage"),
                      Kw.Param("amount")),
    TextTemplate: "Deal {amount} damage to {target}.");
```

### Composite keyword calling another game-creator keyword

```csharp
// Assumes "take-damage" is already registered.
var attack = new KeywordDefinitionBuilder("attack")
    .WithDescription("Attacks a target, dealing damage reduced by the attacker's power.")
    .WithReturnType(TypeName.Void)
    .WithParam("attacker", TypeName.Card)
    .WithParam("target", TypeName.Card)
    .WithBody(Kw.Invoke("take-damage",
        Kw.Param("target"),
        Kw.Max(
            Kw.Num(0),
            Kw.Subtract(
                Kw.Invoke("get-state", Kw.Param("attacker"), Kw.Str("power")),
                Kw.Invoke("get-state", Kw.Param("target"), Kw.Str("armor"))))))
    .WithTextTemplate("{attacker} attacks {target}.")
    .Build();
```

---

## `Kw.*` Node Constructors

| Method | Produces |
|---|---|
| `Kw.Param("name")` | `ParameterRef` — resolves to the named parameter at runtime |
| `Kw.Num(3)` | `Literal<double>` |
| `Kw.Bool(true)` | `Literal<bool>` |
| `Kw.Str("damage")` | `Literal<string>` — use for accumulator names, condition names, etc. |
| `Kw.Invoke("keyword-name", args...)` | `Invocation` — calls any registered keyword |
| `Kw.ModifyAccumulator(atom, name, delta)` | Typed shorthand for `modify-accumulator` |
| `Kw.ApplyModifier(atom, name, kind, value, lifetime)` | Typed shorthand |
| `Kw.RemoveModifier(id)` | Typed shorthand |
| `Kw.ApplyCondition(atom, name, lifetime)` | Typed shorthand |
| `Kw.RemoveCondition(id)` | Typed shorthand |
| `Kw.MoveCard(card, destination)` | Typed shorthand |
| `Kw.DeclareWinner(player)` | Typed shorthand |
| `Kw.DeclareDraw()` | Typed shorthand |
| `Kw.Assert(condition, onFail, notify)` | Typed shorthand — see cost model |
| `Kw.Add(a, b)` / `Kw.Subtract(a, b)` / `Kw.Multiply(a, b)` | Arithmetic |
| `Kw.Max(a, b)` / `Kw.Min(a, b)` | Arithmetic |

---

## Built-In Primitives Reference

Check built-ins before writing a composite keyword. If a built-in covers the mechanic, invoke it directly.

### Mutation primitives (`§9.1`)

| Keyword | Signature | Notes |
|---|---|---|
| `modify-accumulator` | `(atom, name: str, delta: num) → Void` | Adds delta to the named accumulator |
| `apply-modifier` | `(atom, name: str, kind: str, value: num, lifetime) → ContributionId` | Kind: `"additive"` or `"multiplicative"` |
| `remove-modifier` | `(id: ContributionId) → Void` | |
| `apply-condition` | `(atom, name: str, lifetime) → ContributionId` | |
| `remove-condition` | `(id: ContributionId) → Void` | |
| `move-card` | `(card: Card, destination: Zone) → Void` | Preserves owner and all state |
| `create-card` | `(defName: str, zone: Zone, owner: Player) → Card` | |
| `copy-card` | `(source: Card, zone: Zone, owner: Player) → Card` | Copies static props only; no runtime state |
| `create-zone` | `(defName: str, owner: Player) → Zone` | Zones are never destroyed |
| `declare-winner` | `(player: Player) → Void` | Sets `GameIsOver`; no further actions run |
| `declare-draw` | `() → Void` | Sets `GameIsOver` |
| `assert` | `(condition: bool, onFail, notify) → Void` | In cost bodies: always `OnFail.Panic, NotifyFlag.Off` |

### Property primitives (`§9.2`)

| Keyword | Signature | Notes |
|---|---|---|
| `get-state` | `(atom, name: str) → Number` | Reads an accumulator or modifier-computed value |
| `get-property` | `(atom, name: PropertyName) → *` | Invokes a property keyword by name |
| `in-zone` | `(card: Card, zone: Zone) → Boolean` | |
| `owner-of` | `(atom) → Player` | |
| `player-by-name` | `(name: str) → Player` | |
| `events-matching` | `(scope, predicate) → EventCollection` | Queries the event log |
| `event-arg` | `(event: EventRef, name: str) → *` | Reads a bound arg from an event |
| `random-int` | `(min: num, max: num) → Number` | Only valid in effect block bodies |
| `shuffle` | `(zone: Zone) → Void` | |

### Arithmetic (`§9.3`)

`add`, `subtract`, `multiply`, `max`, `min`, `at-least`, `at-most`

---

## TextTemplate

`TextTemplate` is an optional format string with `{paramName}` placeholders. It provides the human-readable card text for this keyword invocation.

- If present, the text renderer emits the template with parameters substituted.
- If absent, the renderer recurses into the body tree — useful for composite keywords where showing the composition is correct.
- Use `[display](keyword-name)` syntax inside a TextTemplate to embed a cross-reference to another keyword's rendered text.

```csharp
.WithTextTemplate("Deal {amount} damage to {target}. If {target} has less than 1 health, destroy it.")
```

---

## Dual-Use Invariant

Every keyword definition must support both execution and text rendering from the same tree. This means:
- Do not write separate "display" and "execution" versions.
- If the natural composition tree produces verbose or confusing text, use `TextTemplate` to override it.
- The renderer is available at any depth; it respects overrides at every level.

---

## Registration

All game-creator keywords must be registered before `Build()`:

```csharp
builder.RegisterKeyword(takeDamage);
builder.RegisterKeyword(attack);
```

`Build()` validates that every `ParameterRef` in every keyword body refers to a declared parameter, and that every invoked keyword name is known.
