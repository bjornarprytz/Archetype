## Context

The existing architecture (D19) left three items explicitly deferred: cost pre-flight, target enumeration, and the ownership assumption baked into `ComputeAvailableActions`. The stubs are already in the type signatures — `NamedEffectBlockDef.Cost: EffectBlockDef?`, `PlayerAction.CostChoices`, `PlayerAction.Targets`, `ValidTargets` on the option types — but the semantics of `CostDef`, the sequential validation algorithm, and the `ValidateActionArgs` host callback are not specified.

This design fills those gaps and removes the ownership filter from `ComputeAvailableActions`.

---

## Goals / Non-Goals

**Goals:**
- Define `CostDef` as a first-class type with an evaluation function (pure affordability check), a body (state-mutating payment), player-provided parameters, and localized text.
- Replace `Cost: EffectBlockDef?` on `NamedEffectBlockDef` and add `Cost: CostDef[]` to both `CardDefinition` and `NamedEffectBlockDef`.
- Specify sequential clone-based multi-cost validation: each cost's `EvaluationFunction` is checked against a clone that has had all prior costs' bodies applied.
- Add `ValidateActionArgs: Func<PlayerAction, ValidationResult>` to `AvailableActions`; define `ValidationResult`.
- Remove the hard-coded ownership filter from `ComputeAvailableActions`; zone membership + `ActivationCondition` do all filtering.
- Specify how costs are sequenced and paid before the main effect at execution time.

**Non-Goals:**
- Full action dry-run / outcome telegraphing (triggered effects, SBR preview).
- Target enumeration (`ValidTargets` remains an empty list).
- Cost pre-flight filtering inside `ComputeAvailableActions` itself (the engine still surfaces all zone-and-condition-eligible actions; the host filters further via `ValidateActionArgs`).

---

## Decisions

### D20 — `CostDef` type and extended `assert` built-in

**Decision:**

```
CostDef {
  Body         : EffectBlockDef    // mutating — pays the cost; assert() signals un-affordability
  Parameters   : ParameterDecl[]  // player-provided args (e.g. which card to discard)
  TextTemplate : string?           // localized cost description; {paramName} placeholders
}
```

`CostDef` has no separate `EvaluationFunction`. Affordability is signalled by `assert` inside `Body`. The `assert` built-in has an extended signature (signed off 2026-03-09):

```
assert(condition: Boolean, on_fail: OnFail = continue, notify: NotifyFlag = on) → Void
```

`OnFail` values: `continue | stop | panic`. `NotifyFlag` values: `on | off`.
These are **inline-literal-only** — game creators cannot declare keyword parameters of these types.

**Cost-body hardwiring:** when `BlockExecutor` executes any `CostDef.Body`, it sets `IsCostBody = true` on the `ExecutionContext`. Under this flag, every `assert` call ignores its `on_fail` and `notify` arguments and behaves as `panic` / `off` (raises `EngineException`, no observer call). This ensures cost bodies always fail hard and silently on unaffordable state.

**`assert` semantics outside cost bodies:**

| `on_fail` | `notify` | Failure behaviour |
|---|---|---|
| `continue` (default) | `on` (default) | Calls `OnDiagnostic`, continues execution |
| `stop` | `on` | Calls `OnDiagnostic`, halts block (no exception) |
| `panic` | `on` | Calls `OnDiagnostic`, then raises `EngineException` |
| any | `off` | Does NOT call `OnDiagnostic`; applies `on_fail` behaviour |

`notify: on` calls `IEngineObserver.OnDiagnostic(DiagnosticEvent)` BEFORE raising `EngineException` when `on_fail: panic`.

`assert` NEVER appends to the event log under any outcome.

Game creators define reusable cost keywords by composing `assert` and payment steps:
```
energy_cost(x) → [assert(gte(energy(source), x)), modify-accumulator(source, "energy", -x)]
discard_cost(card) → [assert(in-hand(card)), move-card(card, discard-zone)]
```

`CostDef` cannot itself carry a `CostDef` (no recursive costs). `Body` is a normal `EffectBlockDef`.

**Alternatives considered:**
- *Separate `EvaluationFunction: KeywordNode` per `CostDef`*: introduces a novel interleaving of pure checks and mutations between cost body executions not present anywhere else in the engine. Rejected — the `assert`-in-body approach uses the same single-block execution path for both validation and real execution.
- *Single combined cost EffectBlockDef without CostDef wrapper*: loses per-cost text and parameters needed for player-facing display and cost arg binding.

**`CardDefinition` updated:**
```
CardDefinition {
  ...
  Cost                : IReadOnlyList<CostDef>   // NEW — empty = no cost
  ActivationCondition : KeywordNode?
  PrimaryEffect       : EffectBlockDef?
  ...
}
```

**`NamedEffectBlockDef` updated:**
```
NamedEffectBlockDef {
  Name                : string
  ActivationCondition : KeywordNode?
  Cost                : IReadOnlyList<CostDef>   // replaces Cost: EffectBlockDef?
  Body                : EffectBlockDef
}
```

See `docs/architecture.md#D20` for the full decision record including `ExecutionContext` flag and `Kw.Assert` C# signature.

---

### D21 — Combined cost block validation via state clone

**Decision:** When `ValidateActionArgs` is called, all `CostDef.Body` blocks for the action are combined into a single composite `EffectBlockDef` (steps concatenated in declaration order) and executed against a lightweight clone of `GameState`. If the combined block completes without throwing `EngineException`, the costs are affordable and `ValidationResult.IsValid` is true. If `EngineException` is thrown (by an `assert` or any other runtime failure), `IsValid` is false.

The clone is a shallow copy of `GameState`'s mutable atom table and accumulator maps — it does not include `EventLog`, active static effects, or contribution registries. This is safe because cost bodies that use `assert` only read accumulator state (energy, health, etc.) and the combined block uses the same single-block execution semantics as real execution, ensuring validation behaviour matches real execution exactly.

`CostResult.Text` is always resolved (from `CostDef.TextTemplate` + locale) regardless of validation outcome, for player-facing display.

**`ValidationResult`:**
```
ValidationResult {
  IsValid   : bool
  CostTexts : IReadOnlyList<string>   // one per CostDef, always populated; resolved from TextTemplate
}
```

**Alternatives considered:**
- *Sequential EvaluationFunction + Body per cost, evaluated against rolling clone state*: introduces a novel interleaving of pure checks and mutations not present anywhere else in the engine; `CheckLifetimes` cannot run between steps, creating a subtle inconsistency between validation and real execution. Rejected in favour of combined single-block execution (see D20 rationale).
- *Full game state clone including event log and effects*: accurate but expensive and unnecessary.

---

### D22 — `ValidateActionArgs` callback placement

**Decision:** `ValidateActionArgs` is a `Func<PlayerAction, ValidationResult>` field on `AvailableActions`. The host calls it freely, as many times as needed, before returning a `PlayerAction` from `IPlayerStrategy.SelectActionAsync`.

```
AvailableActions {
  PlayableCards        : IReadOnlyList<PlayableCardOption>
  ActivatableAbilities : IReadOnlyList<ActivatableAbilityOption>
  CanPass              : bool
  ValidateActionArgs   : Func<PlayerAction, ValidationResult>   // NEW
}
```

The delegate is constructed by the engine at `ComputeAvailableActions` time. It captures the current `GameState` snapshot (read-only) and the `GameDefinition`. The host does not need a direct reference to `GameSession`.

**Alternatives considered:**
- *Public method on `GameSession`*: requires the host to hold a `GameSession` reference in the strategy implementation; creates a tighter coupling than passing a delegate.
- *Static helper*: has no access to the current `GameState`.

---

### D23 — Cost execution sequencing at action time

**Decision:** When `ActionResolver` executes a `PlayCard` or `ActivateAbility` action, costs are paid before the main effect in declaration order. Each cost's `Body` runs as its own `EffectBlockDef` within the same action scope as the primary effect (no separate action scope for cost payment; cost events appear in `events.this_action`). If any cost body raises an `EngineException` (e.g. insufficient resources), the action fails and the exception propagates — it is the host's responsibility to call `ValidateActionArgs` before submitting an action.

**Alternatives considered:**
- *Separate action scope per cost*: adds scope overhead and splits cost events from effect events, complicating event-log queries.
- *Rollback on cost failure*: too expensive and semantically complex; the validated path via `ValidateActionArgs` is the intended usage.

---

### D24 — Ownership filter removal

**Decision:** `ComputeAvailableActions` removes the "zone owner == active player" predicate. Step 1 (PlayCard candidates) iterates all card atoms whose zone's definition name is in `PlayableZoneNames` (regardless of owner). Step 2 (abilities) iterates all card atoms (regardless of owner). The `ActivationCondition` is the sole mechanism for expressing ownership or any other playability constraint.

Games that want the prior ownership behaviour add `ActivationCondition: Kw.OwnedByActivePlayer()` on the relevant `CardDefinition` or `NamedEffectBlockDef`.

**`Kw.OwnedByActivePlayer()` convention (signed off 2026-03-09):** This is a helper in `Archetype.Build` (not an engine primitive). It expands to:
```
Kw.Eq(Kw.OwnerOf(Kw.Param("source")), Kw.GetState(Kw.Session(), "active-player"))
```
Requirement: the game must declare a session state field named `"active-player"` (a `string` value). If this field is absent, `EvaluateCondition` will throw at runtime. The `Archetype.Build` XML doc and game creator guide must document this requirement.

See `docs/architecture.md#D24` for the full decision record.

**Updated algorithm sketch:**
```
ComputeAvailableActions(string activePlayer, GameState state):

  // Step 1: PlayCard candidates
  candidates = all card atoms in state
  if PlayableZoneNames is non-empty:
    candidates = candidates where zone.DefinitionName ∈ PlayableZoneNames
  for each candidate:
    source = candidate
    if cardDef.ActivationCondition == null OR EvaluateCondition(cardDef.ActivationCondition, state, {source}):
      add PlayableCardOption(Card: candidate.Id)

  // Step 2: Abilities — all card atoms, all zones
  for each card atom in state:
    for each ability in cardDef.AdditionalEffects:
      source = card atom
      if ability.ActivationCondition == null OR EvaluateCondition(ability.ActivationCondition, state, {source}):
        add ActivatableAbilityOption(Source: card.Id, EffectName: ability.Name)

  result.CanPass = true
  result.ValidateActionArgs = (action) => SequentialCostValidation(action, state, def)
  return result
```

---

### D25 — Breaking changes, `DiagnosticEvent`, and `IEngineObserver.OnDiagnostic`

**Decision:** The following types and interfaces change as part of this change. All are introduced together.

**`DiagnosticEvent` type (new, `Archetype.Core`):**
```
DiagnosticEvent {
  Kind          : DiagnosticKind   // int-backed extensible enum; AssertionFailed = 0
  Message       : string
  ConditionNode : KeywordNode?     // the failing condition AST node; null if unavailable
  OnFail        : OnFail           // the on_fail value in effect at assertion time
  Location      : string           // human-readable, e.g. "energy_cost @ PlayCard"
}
```

`DiagnosticKind` is int-backed (not a closed `switch`-exhaustive enum) so future kinds can be added without a breaking change to the observer interface.

**`IEngineObserver.OnDiagnostic` (new method):**
- Signature: `void OnDiagnostic(DiagnosticEvent e)`
- Called by `BlockExecutor` when `assert` fails with `notify: on`
- Called BEFORE raising `EngineException` when `on_fail: panic`
- Null observer = no-op (existing guard pattern)
- Sync void; must not throw (caller treats any exception as engine error)
- Does NOT write to the event log

**Breaking change catalogue:**

| Component | Change |
|---|---|
| `IEngineObserver` | Adds `OnDiagnostic` — all implementations must add the method |
| `AvailableActions` | Adds `ValidateActionArgs: Func<PlayerAction, ValidationResult>` — all struct literals must supply it |
| `NamedEffectBlockDef` | `Cost: EffectBlockDef?` → `Cost: IReadOnlyList<CostDef>` — all construction and match sites must update |
| `PlayCard`/`ActivateAbility` | Cost bodies now execute before primary effect — tests observing effect events without prior cost events must add cost definitions or update expectations |
| `ComputeAvailableActions` | Ownership filter removed — callers expecting only active-player cards must add `ActivationCondition: Kw.OwnedByActivePlayer()` |

See `docs/architecture.md#D25` for the full decision record.

---

## Risks / Trade-offs

- **Clone fidelity**: The lightweight clone (atom table + accumulators only) accurately reflects cost interactions on simple numeric resources. If a cost body triggers a while-condition transition (e.g. moving a card changes a zone-based modifier), the clone will not reflect that modifier change. This is an acceptable known limitation — deep-clone cost validation is out of scope.
- **Performance**: `ValidateActionArgs` is synchronous. For AI strategies calling it on every candidate action in a large hand, the cost is O(hand × costs × clone-depth). Expected to be fast in practice; no async path needed.
- **Ownership removal is a breaking behaviour change**: Any existing game definition that relied on implicit ownership filtering must explicitly add an `ActivationCondition`. The `Kw.OwnedByActivePlayer()` helper makes migration straightforward; all existing tests must be audited.
- **Deferred target validation**: `ValidateActionArgs` does not validate target legality (only cost affordability). `IPlayerStrategy` implementations must not assume targets are validated by the engine.

---

## Open Questions

- Should `CostDef.Parameters` bind into the `source` evaluation context, or do cost parameters use a separate namespace from `PlayerAction.CostChoices`? (Proposal: cost parameters use the same binding map as `source` — they are resolved from `CostChoices` by parameter name.)
- Should `ValidationResult.CostResults` include resolved text even for costs that weren't evaluated (because an earlier cost failed)? (Proposal: yes — always resolve all cost texts for display purposes; only `CanAfford` reflects the sequential evaluation result.)
