---
status: signed-off
owner: technical-architect
signed-off: 2026-03-11
last-updated: 2026-03-11
depends-on:
  - docs/requirements.md
  - docs/domain-model.md
---

# Archetype — Architecture

## Status
**Complete. Signed off 2026-03-02. Updated and re-signed off 2026-03-03 (A14, A15, D17). Updated 2026-03-03 (D18, A16, D9/D7 corrections). Updated 2026-03-05 (game outcome primitives ratified in D7/D14 addendum; D19 ComputeAvailableActions contract). Updated 2026-03-06 (D11 amendment: `RenderStaticEffect` omits lifetime node for permanent effects; `PromptChannel` suspension gap ratified as host integration concern). Updated 2026-03-11 (tooling change: D26–D31 added; D14 addendum in D29 for InitManifest mandatory, HostManifest, LocalId uniqueness). Updated 2026-03-11 (D27 implementation-review amendments: `KeywordEntry.ReturnType` mandatory; `CardEntry.ArtCropRegion` specified; `StaticEffectEntry` full schema specified, static-effect export not deferred; `RenameEntry` DSL-rewrite requirement; `ZoneSpec` serialisation bug corrected; D28 `GetSymbolInfo` `referencedBy` shape corrected).**

All decisions D1–D31 are stable and signed off. Updated 2026-03-02 to incorporate domain model amendments A1–A13: declarative re-activation mechanism (D6), dormant effect tracking, resolved domain model flags in D4/D8/D9/D12/D13, and consistency fixes in D12/D16/D17. Updated 2026-03-03 to incorporate A14 (type system formalization — `ParameterDecl` atom-kind subtype restriction, D2 addendum), A15 (Session atom as a fourth atom kind; player registry generalization — D14 addendum, D15 and D16 minor updates), and D17 (Save/Load — turn-boundary granularity, `GameStateSnapshot`, `BoundValue`, `SeededRandom` reimplementation, `IEngineObserver.OnTurnStart`). Updated 2026-03-03 to add D18 (Keyword cross-references in card text — `RulesRef` render node, `[display](key)` tag syntax in `TextTemplate`, `TextRenderer.Resolve`). Updated 2026-03-03 to incorporate A16 (zone movement primitive — `move-card` added to D12 primitives table, `Kw.MoveCard` added to D14, `BuiltInKeywords` note updated in D15) and to correct stale `IPromptChannel` constructor references in D9 and D7 consequences (superseded by D14/A15). Updated 2026-03-05 to ratify game outcome primitives (`declare-winner`, `declare-draw`, `player-by-name`) and their `GameIsOver` propagation contract (D7 amendments and D14 addendum), and to add D19 (`ComputeAvailableActions` contract — `get-atoms-in-zone` primitive, `CardDefinition.ActivationCondition`, `GameDefinition.PlayableZoneNames`). Updated 2026-03-11 to add D26 (Electron + .NET sidecar platform), D27 (sidecar-authoritative data layer, project file format, DSL text as canonical form), D28 (validation trigger model, debounce, sidecar protocol surface), D29 (D14 addendum: InitManifest mandatory and renamed, HostManifest append layer with StateOverrides, CardSpec LocalId, AtomStateOverride, updated nine-step provisioning order), D30 (Godot export pipeline: folder-drop package, Level 1 signal derivation with opt-out, post-action event log polling via GameStateView.LastActionEvents), and D31 (missing-translation export gate: warning classification, confirmation dialog, no persistent preference).

---

## Decisions

### D1 — Language and Runtime

**Decision:** C# / .NET 10. The engine is a plain .NET class library with no dependency on any game framework.

**Rationale:**
- Team has C# familiarity from the prior implementation.
- Strong static typing can enforce the mutation/property keyword distinction at compile time.
- `async`/`await` maps cleanly onto mid-effect prompt suspension: a block awaits player input via `TaskCompletionSource<T>` without blocking and without threads.
- Godot 4.x natively supports .NET, so the engine embeds directly into the Godot project as a referenced assembly — no IPC layer required.
- LINQ gives expressive, readable event log querying.

**Consequences:**
- The engine targets .NET 10. All NuGet dependencies must be .NET 10-compatible.
- **WASM constraint.** The target deployment path is Godot → WebAssembly → itch.io. The engine must be WASM-safe:
  - No `System.Threading.Thread` or `ThreadPool`. WebAssembly is single-threaded in the Godot export context.
  - `async`/`await` is safe — it compiles to state machines, not threads.
  - Minimize reflection. Trim-unfriendly code inflates binary size and may fail at runtime under the WASM IL stripper.
  - No raw file I/O or sockets in the engine core. Those belong in the host/Godot layer.
- **Godot integration.** Godot C# scripts call the engine's public API directly. No GDScript bindings are required. The engine has no Godot types in its public surface.
- **Risk: Godot C# WASM export.** C# WASM export in Godot 4 has been maturing; verify the target Godot version fully supports it before committing the playtesting pipeline.
- **Tooling is separate.** The authoring tool (DSL editor, card/keyword/rule creator) is a standalone desktop application. It produces serialized game definitions that the engine consumes at runtime. It need not be WASM-compatible and may use richer .NET APIs freely.
- **Serialization boundary.** Because the tooling runs separately from the engine, there is a data serialization boundary between them. Game definitions (keywords, cards, phases, rules) must have a well-defined serialized form. This feeds into the keyword representation and game creator API decisions.

---

### D2 — Keyword Representation

**Decision:** Keywords are represented as interpreted expression trees. The source format is a textual DSL authored by game creators. The tooling parses DSL text into a `KeywordNode` tree and serializes it to JSON. The engine loads JSON and deserializes to trees at startup — it does not contain a parser.

**Tree structure:**

```
KeywordNode (abstract record)
  ├── ParameterRef(name: string)
  │     Refers to a declared parameter by name.
  ├── Literal(value)
  │     A hardcoded value: number, boolean, string, or atom reference.
  └── Invocation(keywordName: string, args: KeywordNode[])
        Calls another keyword (built-in or game-creator-defined) with argument nodes.
```

A `KeywordDefinition` contains:
- `Name: string`
- `Parameters: ParameterDecl[]` — each with a name and a declared type
- `Body: KeywordNode` — the expression tree (for composite keywords), or a sentinel marking which engine primitive this is (for primitives)
- `TextTemplate: string?` — an optional format string with `{paramName}` placeholders, used by the text renderer. If absent, the renderer recurses into the body tree.

`ParameterDecl` types form the engine's type vocabulary: `Atom`, `Number`, `Boolean`, `ConditionName`, `PropertyName`, `ContributionId`, `Lifetime`, `EffectBlock`. A14 (domain model) formalized the atom subtype hierarchy — `Atom` with subtypes `Card`, `Zone`, `Player`, `Session` — which extends the type vocabulary and informs type-checking rules described below.

**`ParameterDecl` structure (updated for A14):**

```
ParameterDecl {
  Name                : string
  Type                : TypeName        // declared type; may be Atom or a specific atom kind
  AtomKindRestriction : AtomKind[]?     // null = unrestricted; non-null = argument's resolved
                                        // static atom kind must be in this set (authoring-time check)
  ReturnType          : TypeName        // on KeywordDefinition — must be explicit per §1.4
  Description         : string          // human-readable; required per §1.4
}
```

`AtomKindRestriction` exists because some built-in keywords accept an `Atom`-typed parameter but are further constrained by the domain model to a subset of atom kinds. The canonical example is `owner-of`: declared `atom: Atom` but restricted to `{ Card, Zone }`. The type-checker enforces this restriction when it can resolve the argument's static atom kind:

- If the argument is a `ParameterRef` whose `ParameterDecl` has type `Card` or `Zone` → valid.
- If the argument is an `Invocation` whose resolved return type is `Card` or `Zone` → valid.
- If the argument is a `ParameterRef` typed `Player` or `Session` → authoring-time error.
- If the argument is a `ParameterRef` typed `Atom` (generic) → authoring-time error; the game creator must declare a more specific parameter type. Conservative rejection is correct: `owner-of` requires a guarantee, not a possibility.

`AtomKindRestriction` is stored in `BuiltInKeywords` in `Core` alongside the parameter's declared type. The type-checker in `Archetype.Build` reads it during validation. Game-creator-defined keywords may not declare `AtomKindRestriction` — it is a mechanism for engine built-ins only.

**Serialization boundary.** The tooling (desktop app) owns the parser. It parses DSL text → validates → emits a JSON game definition file. The engine owns the deserializer. It reads JSON → constructs `KeywordDefinition` trees in memory. The engine never sees raw DSL text; the parser is not in the engine assembly. This keeps the WASM binary smaller and the engine free of parser complexity.

**Dual-use.** Two separate interpreters walk the same `KeywordNode` tree:
- **Execution interpreter** — walks the tree against live `ExecutionContext` (game state, scope, variable bindings), applying mutations and reading values.
- **Text renderer** — walks the same tree, substituting parameter names and recursing into composite bodies to the depth the game layer requests.

**Rationale:**
- The tree is pure data: immutable, serializable, inspectable. It satisfies §1.1's dual-use invariant without duplicating definitions.
- Keeping the parser in the tooling and the deserializer in the engine maintains a clean boundary, reduces engine complexity, and improves WASM binary size.
- The `TextTemplate` on each definition gives game creators control over rendered text without a separate file; falling back to structural rendering preserves the full composition tree for detailed inspection.

**Consequences:**
- The JSON schema for `KeywordDefinition` trees is a first-class contract between the tooling and the engine. It must be versioned.
- The tooling must validate the tree at parse time (type-checking, acyclicity, mutation/property subtype invariants) so the engine can trust what it loads.
- The text renderer needs a depth parameter or strategy so the game layer can choose between "show top-level text only" and "expand full composition."
- Built-in (primitive) keywords are registered in the engine at startup, not loaded from JSON. The JSON file references them by name; the engine resolves the name to its built-in implementation.

**Addendum — `TextTemplate` cross-reference tag syntax (D18).** `TextTemplate` strings and locale file template strings may contain keyword cross-reference tags alongside `{paramName}` substitutions:

- **Short form:** `[keyword-name]` — the keyword name is used as both the lookup key and the display text.
- **Long form:** `[display text](keyword-name)` — explicit display text when the prose term differs from the keyword name (e.g. `[damage](take-damage)`).

Both forms produce a `RulesRef` node in the rendered output (see D18). The `keyword-name` in a tag must resolve to an entry in `GameDefinition.Keywords` (built-in or game-creator-defined); this is validated at authoring time — build time for the C# builder, parse time for the DSL tooling, load time for the JSON deserializer. An unknown keyword name in a tag is a `DefinitionException`. Tag parsing occurs after `{paramName}` substitution resolution; a tag may not span a `{paramName}` boundary.

---

### D3 — Effect Block Execution Model

**Decision:** The block interpreter is an `async` method. Mid-effect prompt suspension is modeled with `TaskCompletionSource<T>`. The engine never uses `Task.Run()` — all async suspension is prompt-driven, not thread-driven.

**Execution flow:**

```
async Task<BlockResult> ExecuteBlock(EffectBlock block, ExecutionContext ctx)
  for each Step in block.Steps:
    args = EvaluateArgs(step.ArgNodes, ctx)   // walks KeywordNode trees synchronously
    if step is a PromptStep:
      response = await ctx.PromptChannel.RequestAsync(promptCtx)  // suspends here
      ctx.Bindings[step.VariableName] = response
    else:
      result = DispatchKeyword(step.KeywordName, args, ctx)
      // mutation keywords append events; property keywords return values
  return BlockResult
```

**`ExecutionContext`** is passed through every interpreter call and carries:
- `GameState` — the mutable game state (atoms, accumulators, modifiers, conditions)
- `Bindings: Dictionary<string, object>` — the block's local variable scope
- `ScopeIds` — the current `BlockScopeId`, `ActionScopeId`, `TurnScopeId` (used to stamp events and answer scope queries)
- `PromptChannel: IPromptChannel` — the interface through which the engine requests player input

**`IPromptChannel`** is an engine-defined interface implemented by the host (Godot). The engine `await`s it; Godot presents UI and calls `Complete(response)` on the underlying `TaskCompletionSource<T>` when the player responds.

```
interface IPromptChannel
  Task<PromptResponse> RequestAsync(PromptContext ctx)
```

**Atomicity enforcement.** The `ActionResolver` (see module boundaries) owns a flag `BlockInProgress`. It sets this flag before calling `ExecuteBlock` and clears it when the returned `Task` completes — including after all prompt suspensions. Trigger evaluation and state-based rule execution check this flag and are skipped while it is set. Because execution is single-threaded (Godot's synchronization context; WASM), no locking is required.

**WASM invariant.** No `Task.Run()`, `Thread`, or `ThreadPool` anywhere in the engine. Every `await` in the engine either awaits `IPromptChannel.RequestAsync` (player input) or awaits a child `ExecuteBlock` call (recursive cost/nested block execution). The call stack unwinds cooperatively on the single game thread.

**Rationale:**
- `async`/`await` in C# compiles to a state machine — this is exactly the hand-rolled continuation approach (Option B) but with readable linear code.
- Godot's C# synchronization context ensures resumed continuations post back onto the main thread, keeping game state access thread-safe by construction.
- `IPromptChannel` keeps the engine decoupled from Godot: the engine defines the interface; the host satisfies it.

**Consequences:**
- All keyword dispatch methods and block executors are `async`-capable but most will complete synchronously (only prompts actually suspend). Callers use `await` uniformly throughout.
- Cost execution is a separate `ExecuteBlock` call that runs before the main block. Its events are visible in `events.this_action` when the main block runs.
- The short-circuit rule (§4.2) is handled inside `ExecuteBlock`: before posting a prompt, it counts valid candidates; if ≤ required choices, it auto-binds without calling `IPromptChannel`.

**Addendum — Block Step Return Binding.** Some mutation keywords return values (`apply-modifier` returns a `ContributionId`; `apply-condition` returns a `ContributionId`; `create-card`, `copy-card`, and `create-zone` return an `Atom`). To capture these values for use by later steps in the same block, `EffectBlockStep` carries an optional `BindTo` field:

```
EffectBlockStep {
  KeywordName : string
  ArgNodes    : KeywordNode[]
  BindTo      : string?    // if non-null, bind the keyword's return value to this variable name
}
```

Dispatch logic:
```
result = DispatchKeyword(step.KeywordName, args, ctx)
if step.BindTo != null && result != null:
  ctx.Bindings[step.BindTo] = result
```

The bound name is then available to any subsequent step in the block as a `ParameterRef`. Steps whose keywords return void (e.g. `modify-accumulator`) may set `BindTo` or leave it null — both are valid; a null result is never written to bindings. This mechanism also clarifies how the existing `apply-modifier` / `apply-condition` return values reach the game creator's variable scope; the domain model's existing language ("returns a contribution-ID") is fully accounted for here.

---

### D4 — Event Log Structure

**Decision:** Events are tree-structured. Every mutation keyword invocation — composite or primitive — produces a `GameEvent` node whose children are the events produced by its internal invocations. Each scope (block, action, turn) maintains a **local event accumulator** that is live and queryable during execution. Scopes merge their accumulators into their parent scope when they exit. The global log receives finalized event trees only as scopes close.

**`GameEvent` record:**

```
GameEvent {
  SequenceNumber : long                         // assigned on finalization; reflects completion order
  KeywordName    : string                       // the keyword that produced this event
  BoundArgs      : Dictionary<string, object>   // parameter name → evaluated value at call time
  Children       : List<GameEvent>              // events from internally invoked mutation keywords
}
```

Property keyword invocations produce no events. Arguments that involve property keyword evaluation appear in `BoundArgs` as already-evaluated values, not as sub-events.

**Scope accumulator model.** Four nested accumulators are live at any point during execution:

```
GameLog          (global, permanent)
  └── TurnScope  (accumulates until the turn exits, then merges into GameLog)
        └── ActionScope  (accumulates until the action exits, then merges into TurnScope)
              └── BlockScope  (accumulates until the block exits, then merges into ActionScope)
```

Within the execution interpreter, a **parent event stack** tracks the currently-executing composite keyword chain. As a primitive keyword completes, its `GameEvent` is immediately appended to the innermost parent event node's `Children`. When a composite keyword completes, its fully-assembled event node is appended to the next outer parent (or to the block accumulator if at the block's top level). This means completed children are visible in the scope accumulator even before their parent composite finishes — the accumulator exposes the in-progress subtree.

**Scope queries** read from the live accumulator at the appropriate depth, including in-progress subtrees:

| Query | Source |
|---|---|
| `events.this_block` | BlockScope accumulator (in-progress subtree included) |
| `events.this_action` | ActionScope accumulator + current BlockScope |
| `events.this_turn` | TurnScope accumulator + current ActionScope + current BlockScope |
| `events.this_game` | GameLog + all live scope accumulators |

This enables patterns like "deal damage equal to total damage dealt this block" — the in-progress event subtree is queryable mid-execution without waiting for the enclosing composite to finalize.

**Trigger conditions** search the event tree to whatever depth they need using the built-in read primitive:

```
events-matching(scope, keywordName, argPredicate) → Collection<GameEvent>
```

This searches all events at any depth within the given scope whose `KeywordName` matches and whose `BoundArgs` satisfy `argPredicate`. Trigger conditions evaluate against `events.this_action` or broader scopes (never `events.this_block` — block scope is no longer meaningful once the block exits). In-block references use `events.this_block`. When a predicate is supplied, the reserved name `candidate` refers to the `EventRef` of the event currently being tested within the predicate (§4.3 of the domain model, A10).

**Example.** `attack(goblin, 3)` calls `take_damage(goblin, 1)` which calls `modify-accumulator(goblin, "damage", 1)`:

```
Event("attack", {target: goblin, amount: 3})
  └── Event("take_damage", {target: goblin, amount: 1})
        └── Event("modify-accumulator", {atom: goblin, name: "damage", delta: 1})
```

Mid-execution — while still inside `take_damage` — the `modify-accumulator` event is already appended to `E_take_damage.Children` and is visible via `events.this_block`. A later keyword in the same block can query `events-matching(this_block, "modify-accumulator", ...)` and find it.

**Rationale:**
- Scope-local accumulators with deferred merge into the global log give game creators the best of both: live in-scope queries (for effect chaining) and a clean global log (for triggers and history).
- Tree structure allows triggers to match at the semantic level of game-creator-defined keywords, not just at the primitive level.
- `BoundArgs` on every node means trigger and in-block queries can inspect any argument of any invocation at any depth without per-keyword event schemas.

**Consequences:**
- The execution interpreter maintains a parent event stack (current composite chain) alongside the block accumulator. Dispatching any mutation keyword pushes a new node; completing it pops the node and appends it to the parent.
- `events-matching` is a built-in read primitive in §9.2 of the domain model (resolved as A2).
- Trigger condition expressions reference `BoundArgs` fields of matched events via `EventRef` and `event-arg` — both are first-class types in the engine's type vocabulary (resolved as A3).
- `SequenceNumber` is assigned at finalization (pop time), so it reflects completion order within the tree.

---

### D5 — Contribution Tracking

**Decision:** Modifier and condition contributions are separate record types sharing a common `ContributionId`. The engine maintains a global registry for O(1) lookup by ID. Each atom maintains per-property and per-condition indexes for efficient state evaluation. Static effects maintain a list of the contribution IDs they own so cleanup on expiry requires no global scan.

**`ContributionId`:** A monotonically incrementing `long`, incremented by a single counter on `GameState`. Single-threaded execution means no synchronization is needed.

**Record types:**

```
ModifierContribution {
  Id           : ContributionId
  Source       : ContributionSource       // AtomId or StaticEffectId that created this
  TargetAtom : AtomId
  PropertyName : string
  Kind         : Additive | Multiplicative
  Value        : double
  Lifetime     : LifetimeSpec?            // null = permanent
}

ConditionContribution {
  Id            : ContributionId
  Source        : ContributionSource
  TargetAtom  : AtomId
  ConditionName : string
  Lifetime      : LifetimeSpec?
}
```

**Storage layout:**

- `GameState` holds:
  - `ContributionRegistry: Dictionary<ContributionId, IContribution>` — global, for O(1) removal by ID (`remove-modifier`)
- Each atom holds:
  - `ModifierIndex: Dictionary<string, List<ModifierContribution>>` — keyed by property name; drives modifier evaluation
  - `ConditionIndex: Dictionary<string, List<ConditionContribution>>` — keyed by condition name; presence = non-empty list

**Modifier evaluation** for a property on an atom:
```
computed = (base + Σ additives) × Π multiplicatives
```
Both sums iterate `ModifierIndex[propertyName]` — always a small list in practice.

**Condition presence** is `ConditionIndex[name].Count > 0`. Absent condition = key absent or empty list.

**Static effect ownership.** Each `StaticEffect` carries `OwnedContributions: List<ContributionId>`. When a `apply-modifier` or `apply-condition` call is made on behalf of a static effect, the returned `ContributionId` is added to this list. On expiry, the engine removes each ID via the registry and drops it from the atom's index. No global scan required.

**Rationale:**
- Separate indexes per atom per property/condition keep evaluation fast without requiring a global sweep.
- Static effect ownership of contribution IDs makes expiry cleanup O(k) where k is the number of contributions that effect owns — typically 1.
- The global registry is only needed for explicit `remove-modifier`; it's a secondary index, not the source of truth.

**Consequences:**
- `apply-modifier` and `apply-condition` allocate a `ContributionId`, create the contribution record, insert it into the atom's index and the global registry, and return the ID.
- `remove-modifier(id)` looks up in the registry, removes from atom index, removes from registry, removes from owning static effect's list if applicable.
- `remove-condition(atom, name)` removes all entries from `ConditionIndex[name]`, removes each from the registry, and removes from owning static effect lists.
- Accumulator deltas have no contribution tracking — they merge permanently into a running total per `(atom, name)` pair on the atom. No registry entry.

---

### D6 — Static Effect Lifecycle Management

**Decision:** The engine maintains a single `List<StaticEffect>` of all active static effects. After every effect block resolves, the engine evaluates all while-conditions and removes any expired static effects and their contributions. Turn-timer and trigger-count conditions are checked at their natural moments (turn boundary and trigger fire, respectively).

**`StaticEffect` record:**

```
StaticEffect {
  Id                 : StaticEffectId    // allocated from a global monotonic counter shared with ContributionId
  Origin             : Declarative | Dynamic
  LifetimeSpec       : LifetimeSpec
  TriggerFireCount   : int               // incremented by trigger resolution; checked against TriggerCount conditions
  StateContribution  : ContributionId?   // null if this effect has no state contribution
  Trigger            : TriggerDefinition?
  OwnedContributions : List<ContributionId>
}
```

**Trigger resolution ordering.** The domain model requires the oldest active static effect to fire first (§5.3). "Oldest" is defined as lowest `StaticEffectId`. Because `StaticEffectId` is allocated from the same global monotonic counter as `ContributionId` — and execution is single-threaded — allocation order is unambiguous at any granularity: two effects created in the same turn, the same action, or even the same block are still totally ordered by their ID. No separate timestamp or turn counter is needed.

**`LifetimeSpec` as data:**

```
LifetimeSpec {
  Conditions: List<LifetimeCondition>    // OR'd; empty = permanent
}

LifetimeCondition (discriminated union):
  | TurnTimer(turns: int)
  | TriggerCount(count: int)
  | WhileCondition(expression: KeywordNode)   // boolean property keyword expression
```

`LifetimeCondition` is itself a `KeywordNode` expression (a `WhileCondition` wraps a property keyword subtree), so it is serializable through the same JSON schema as keyword definitions.

**Post-block check routine (two-phase, updated for A1).** After every `ExecuteBlock` call returns (including state-based rule blocks), the `ActionResolver` calls `CheckLifetimes(gameState)`:

**Phase 1 — expire active effects:**
1. Iterate all active static effects.
2. For each, evaluate its `LifetimeSpec`: check all `WhileCondition` expressions against current game state; check `TurnTimer` conditions against current turn count; check `TriggerCount` against `TriggerFireCount`.
3. If any condition is satisfied (OR semantics), collect it as expired.
4. For each expired effect:
   - Remove all `OwnedContributions` (§D5); remove from `ActiveStaticEffects`.
   - Classify expiry: **terminal** if any TurnTimer or TriggerCount condition fired; **while-condition expiry** if only a WhileCondition fired.
   - If while-condition expiry AND `se.Origin == Declarative`: add `DormantDeclarativeEffect { OwnerAtom: se.OwnerAtom, EffectDef: se.SourceDefinition }` to `GameState.DormantDeclarativeEffects`.
5. If any effects expired, repeat Phase 1 — expiry can cascade (a condition that was true only because of a now-removed contribution may now be false, or vice versa).

**Phase 2 — activate dormant declarative effects:**
6. Iterate all `DormantDeclarativeEffects`. For each, evaluate its `EffectDef.LifetimeSpec`'s WhileCondition against current game state, with `{ "source": dormant.OwnerAtom }` as the evaluation bindings.
7. If true: call `InstantiateStaticEffect(dormant.EffectDef, dormant.OwnerAtom)` — allocate a fresh `StaticEffectId`; set `TriggerFireCount = 0`, `TriggerHighWaterMark = 0`; apply any state contribution; add to `ActiveStaticEffects`. Remove from `DormantDeclarativeEffects`.
8. If any dormant effects activated, return to Phase 1 — new active effects may introduce contributions that change other conditions.

**Turn-timer check** is performed at the same `CheckLifetimes` call — no separate hook needed since the call happens after every block, including phase init/cleanup blocks that mark turn boundaries.

**Trigger-count expiry.** When trigger resolution fires a static effect's trigger (§D7), it increments `TriggerFireCount`. The next `CheckLifetimes` call will catch the satisfied `TriggerCount` condition and expire the effect. Trigger-count expiry does not bypass the normal lifetime check loop.

**Declarative static effect activation (A1).** At card creation, each declarative static effect is provisioned based on its while-condition state:
- **No while-condition, or while-condition evaluates to true:** instantiate immediately and add to `GameState.ActiveStaticEffects`.
- **While-condition evaluates to false:** add to `GameState.DormantDeclarativeEffects` without creating a `StaticEffect` instance.

The same logic applies when cards are created during play via `create-card` or `copy-card`. Both paths call a shared `ProvisionDeclarativeEffect(effectDef, ownerAtom, state)` helper.

**Dormant tracking data structure:**

```
DormantDeclarativeEffect {
  OwnerAtom : AtomId
  EffectDef   : StaticEffectDef
}
```

`GameState` gains `DormantDeclarativeEffects : List<DormantDeclarativeEffect>` alongside the existing `ActiveStaticEffects` list.

**Re-instantiation rule.** When a `StaticEffect` expires, the expiry is classified before deciding whether to go dormant:
- **Terminal expiry** — any TurnTimer or TriggerCount condition fired (regardless of while-condition state). Discard permanently. No re-instantiation. This preserves the intent: TurnTimer and TriggerCount are authoring signals that the effect is fundamentally finite.
- **While-condition expiry** — only a WhileCondition fired (no TurnTimer or TriggerCount also satisfied). If declarative, add to `DormantDeclarativeEffects` for potential re-activation. Dynamic effects are always discarded permanently on expiry.

A declarative effect may later be re-instantiated any number of times. Each re-instantiation produces a new `StaticEffect` with a new idatom, fresh `TriggerFireCount = 0`, and fresh `TriggerHighWaterMark = 0`. The expired instance is never resumed.

**Rationale:**
- A single `List<StaticEffect>` with a post-block sweep is simple, correct, and fast for the expected number of active effects in a card game (tens, not thousands).
- Cascading the lifetime check handles effects whose expiry depends on other effects' contributions — an important edge case for effects that chain.
- Placing trigger-count expiry in the normal `CheckLifetimes` loop (rather than inline in trigger resolution) keeps expiry logic in one place.
- The two-phase sweep (expire then activate dormant) handles declarative re-instantiation without a separate scheduling mechanism. Because `CheckLifetimes` runs on every block boundary already, no additional hooks are needed.
- Classifying expiry as terminal vs. while-condition honours the domain model's distinction: TurnTimer and TriggerCount signal that an effect is finite; a while-condition is a predicate the effect follows for its lifetime. A TurnTimer expiry coinciding with a false while-condition still discards permanently.
- Tracking dormant effects as explicit `(ownerAtom, effectDef)` pairs rather than re-scanning all card definitions each sweep keeps Phase 2 O(dormant) rather than O(all_cards × effects_per_card).

**Consequences:**
- `CheckLifetimes` is called by `ActionResolver` after every block, including cost blocks, state-based rule blocks, and trigger-fired blocks.
- `WhileCondition` expressions must be evaluable against a `GameState` without an `ExecutionContext` (no variable bindings, no event log scope). The property keyword evaluator needs a state-only evaluation path. This applies equally to Phase 2 dormant activation checks.
- The cascade loop in `CheckLifetimes` must terminate — guaranteed by the game creator's responsibility for convergence (§8.3 of the domain model).
- `GameState` gains `DormantDeclarativeEffects : List<DormantDeclarativeEffect>` alongside `ActiveStaticEffects`.
- `StaticEffect` gains `SourceDefinition : StaticEffectDef?` — non-null for declarative effects, null for dynamic. Used to populate the dormant record on while-condition expiry. See D13 for the canonical updated `StaticEffect` record.
- Card provisioning (manifest provisioning and `create-card`/`copy-card` implementations) calls a shared `ProvisionDeclarativeEffect(effectDef, ownerAtom, state)` helper that performs the active-vs-dormant split. Both provisioning paths must use this helper to stay in sync.
- `InstantiateStaticEffect(effectDef, ownerAtom)` is a shared helper called by provisioning and by Phase 2. It allocates a new `StaticEffectId`, sets `TriggerFireCount = 0` and `TriggerHighWaterMark = 0`, applies any state contribution (registering the returned `ContributionId` in `OwnedContributions`), and sets `SourceDefinition = effectDef`.

---

### D7 — State-Based Rule Runner

**Decision:** State-based rules are records pairing a boolean condition expression with an effect block body. They are stored in `GameDefinition` (static game data), not `GameState`. The `ActionResolver` runs a fixpoint loop over them after every effect block. The post-action sequence tracks trigger-resolution batch count and notifies the host via an optional `IEngineObserver` interface after each batch, giving the host the ability to halt a runaway cascade.

**`StateBasedRule` record:**

```
StateBasedRule {
  Name      : string         // for debugging; matches action-rule addressing name (§8.3 of domain model)
  Condition : KeywordNode    // boolean property keyword expression — no side effects, no event log entries
  Body      : EffectBlockDef // the block that executes if Condition evaluates to true
}
```

Rules are registered by the game creator at game setup and stored in `GameDefinition.StateBasedRules: List<StateBasedRule>` in registration order.

**Fixpoint loop:**

```
async Task RunStateBasedRules(ExecutionContext ctx)
  loop:
    if ctx.GameState.GameIsOver: return              // terminal early-exit — prevents infinite loops on always-true SBRs (D14 addendum)
    triggered = [rule in GameDefinition.StateBasedRules
                 where EvaluateCondition(rule.Condition, ctx.GameState) == true]
    if triggered is empty: return

    for each rule in triggered (in registration order):
      await ExecuteBlock(rule.Body, ctx)   // sets BlockInProgress, clears it, calls CheckLifetimes (D6)
  // repeat — re-evaluate all conditions after each full pass
```

All triggered conditions are evaluated before any blocks fire in that pass; rules that fire together all execute before the next condition sweep. `CheckLifetimes` (D6) is called after each individual block inside `ExecuteBlock`.

**Rule ordering.** Registration order. The game creator is responsible for registering rules in an order that yields correct behavior. This is consistent with §8.3's convergence-is-your-responsibility stance.

---

**Post-action sequence.** `ActionResolver` owns the following sequence after every player action:

```
1.  ExecuteBlock(primaryBlock, ctx) → CheckLifetimes
2.  RunStateBasedRules(ctx)                          // fixpoint
    ── cascade loop ──────────────────────────────────────────────
3.  if GameIsOver: break cascade loop                // terminal early-exit (D14 addendum)
4.  triggerBatchCount++
5.  directive = await EngineObserver?.OnTriggerCascade(triggerBatchCount) ?? Continue
6.  if directive == Halt: break cascade loop
7.  CollectSatisfiedTriggers → sort by StaticEffectId ascending
8.  if none: break cascade loop
9.  for each triggered static effect (oldest first):
      await ExecuteBlock(triggeredBlock, ctx) → CheckLifetimes
      RunStateBasedRules(ctx)                        // fixpoint after each trigger-fired block
    ── repeat from step 3 ────────────────────────────────────────
10. Open next player action window
```

The observer is called *before* trigger collection in each batch (step 5), so the host can halt before more triggers fire. `triggerBatchCount` resets to zero at the start of each new action.

---

**`IEngineObserver` interface:**

```
interface IEngineObserver
  Task<CascadeDirective> OnTriggerCascade(int iterationCount)

enum CascadeDirective { Continue, Halt }
```

- Defined in the engine; implemented by the host (Godot layer or test harness).
- Injected into `ActionResolver` as a nullable reference. `null` → always `Continue` (simple games need not implement it).
- `iterationCount` is the count of trigger-resolution batches completed so far in the current action (1-based: first call passes `1`).
- When the host returns `Halt`, the engine exits the cascade loop cleanly and proceeds to step 10. No partial state is rolled back. State-based rules (step 2) do not re-run after a `Halt` — the loop exits immediately.
- The interface is `async` (returns `Task<CascadeDirective>`) so the host may present UI (e.g. "Infinite loop detected — player X wins") before the engine proceeds.

**Design note — host boundary.** The host (Godot layer) is a consumer of engine state: it renders, presents UI, and supplies player input. It does not author game logic and cannot invoke engine effects at runtime. `IEngineObserver` is the extent of the host's runtime influence on the engine — it may observe cascade depth and request a halt, but it may not push state changes into the engine. All game mechanics, including any "cascade depth matters" rule, must be expressed in the `GameDefinition` by the game creator (state-based rules, triggers, etc.) and evaluated by the engine itself. This boundary is a firm constraint on the game creator API decision (D9).

**Rationale:**
- Fixpoint-before-triggers matches the standard card game ordering: mandatory corrections (SBRs) are resolved before optional triggers so that trigger conditions observe a clean game state.
- A separate `IEngineObserver` rather than extending `IPromptChannel` keeps the input-request and host-notification contracts distinct. Both follow the same pattern: engine-defined interface, host-implemented, injected at construction.
- Making `IEngineObserver` optional (nullable) avoids burdening simple games with a required implementation.
- `Halt` without rollback means the game creator remains responsible for convergence — the engine provides the escape hatch, not the guarantee.

**Consequences:**
- `ActionResolver` construction-time dependencies: per-player `IReadOnlyDictionary<string, IPlayerStrategy>`, `IRandomSource`, and `IEngineObserver?` (nullable). `IPromptChannel` from D3 is retired; see D14 for the canonical constructor.
- The `triggerBatchCount` is a local variable in the `ActionResolver.ResolveAction` method, reset per action. It does not live on `GameState` or `ExecutionContext`.
- D8 (trigger resolution) will define `CollectSatisfiedTriggers` and the sort ordering referenced in step 6 above.
- `Halt` terminates only the cascade loop; the action is otherwise complete and the game continues. Win/loss conditions triggered by a `Halt` are the game creator's responsibility via SBRs.

---

---

### D8 — Trigger Resolution

**Decision:** A trigger is defined by a `TriggerDefinition` record on a `StaticEffect`. Collection uses a per-effect high-water mark to guarantee each event fires a given trigger at most once. Conditions are evaluated per-candidate-event with the candidate's `BoundArgs` injected as named values. The triggering event is always available in the fired block's scope as the reserved variable `trigger_event` (typed `EventRef`); `EventBindings` is an optional convenience for pre-binding specific args to friendlier names. Trigger resolution order is a game-level setting with three options: `OldestFirst` (default), `OldestLast`, or `PromptPlayer`.

---

**`TriggerDefinition` record:**

```
TriggerDefinition {
  EventKeyword  : string               // required; candidates are events whose KeywordName matches this
  Scope         : TriggerScope         // visibility granted to the Condition expression
  EventParams   : List<EventParamDecl> // declares which BoundArgs keys to expose to the Condition
  Condition     : KeywordNode?         // optional boolean filter on the candidate event; null = match all
  EventBindings : List<EventBinding>   // optional convenience: maps BoundArgs keys → friendly block variable names
  FiredBlock    : EffectBlockDef
}

TriggerScope = ThisAction | ThisTurn | ThisGame

EventParamDecl {
  ArgName   : string    // key in GameEvent.BoundArgs
  ParamName : string    // name resolved by ParameterRef nodes in the Condition expression
  Type      : TypeName
}

EventBinding {
  EventArgName : string   // key in GameEvent.BoundArgs
  BlockVarName : string   // friendly variable name pre-populated in the triggered block's Bindings
}
```

`EventKeyword` is required — it is the primary index for efficient candidate lookup. `Condition` is optional; if absent, every event from `EventKeyword` satisfies the trigger. `EventParams` is validated by the tooling against the declared parameters of `EventKeyword` so the condition expression is type-safe.

**`TriggerScope` semantics.** `Scope` governs what event history the `Condition` expression can reference via event-log queries (e.g. `events-matching` inside "only fire if 3 damage events have occurred this turn"). It does not restrict which events are candidates — that is handled entirely by the high-water mark. Trigger conditions may not reference `events.this_block` (block scope ceases to be meaningful once a block exits — see §7 of domain model).

---

**Full triggering event access in fired blocks.** The triggering `GameEvent` is always pre-populated in the fired block's bindings under the reserved name `trigger_event`, typed as `EventRef`. The block can extract any arg from it using the built-in read primitive:

```
event-arg(event: EventRef, name: string) → value
```

`EventRef` is a new first-class type in the engine's type vocabulary (alongside `Atom`, `Number`, `Boolean`, etc.). `EventBindings` are an optional convenience on top: they let the game creator pre-bind specific event args to friendly names (so the block can write `ParameterRef("target")` instead of `event-arg(trigger_event, "target")`). The full event is always accessible regardless of what `EventBindings` declares.

**Domain model note.** `EventRef` and `event-arg` are additions to §9.2 (Read Primitives) — resolved as A3. `EventRef` is defined in §7.1; `event-arg` is tabulated in §9.2.

---

**High-water mark.** Each `StaticEffect` carries:

```
TriggerHighWaterMark : long    // SequenceNumber of the last candidate event evaluated; 0 initially
```

This is the mechanism for "a trigger fires at most once per event" (§5.3 of domain model). The high-water mark advances past every candidate event seen in a collection pass — whether the condition matched or not — so subsequent passes only see events added *after* this pass (i.e. events from trigger-fired blocks in the current cascade iteration).

---

**Trigger resolution order.** A game-level setting on `GameDefinition`:

```
GameDefinition {
  ...
  TriggerResolutionOrder : TriggerResolutionOrder   // default: OldestFirst
}

TriggerResolutionOrder = OldestFirst | OldestLast | PromptPlayer
```

- **`OldestFirst`** (default) — sort by `(se.Id ASC, e.SequenceNumber ASC)`. The oldest active static effect fires first; within a single effect, the earliest matching event fires first.
- **`OldestLast`** — sort by `(se.Id DESC, e.SequenceNumber ASC)`. The newest active static effect fires first; within a single effect, event order is still ascending.
- **`PromptPlayer`** — the player chooses the order of effects. `CollectSatisfiedTriggers` groups firings by `StaticEffect`; the engine posts a `TriggerOrderPrompt` via `IPromptChannel` presenting these groups and asking the player to sequence them. Within each group, events still fire in `SequenceNumber ASC` order. The player orders effects, not individual firings.

`PromptPlayer` routes through the existing `IPromptChannel` using a new `TriggerOrderPrompt` variant of `PromptContext` — consistent with the existing pattern; no new interface is needed. The player's response is a permutation of the effect groups, which becomes the firing sequence.

---

**Collection algorithm (`CollectSatisfiedTriggers`):**

```
async Task<List<TriggerFiring>> CollectSatisfiedTriggers(GameState state)
  result = []
  for each active StaticEffect se with a non-null Trigger t:
    candidates = events in t.Scope
                   where KeywordName == t.EventKeyword
                     and SequenceNumber > se.TriggerHighWaterMark
                   ordered by SequenceNumber ascending

    newHighWater = se.TriggerHighWaterMark
    for each candidate event e:
      newHighWater = e.SequenceNumber
      evalCtx = TriggerEvaluationContext {
                  EventParams: bind(t.EventParams, e.BoundArgs),
                  GameState:   state,
                  LogScope:    t.Scope }
      if t.Condition == null OR EvaluateCondition(t.Condition, evalCtx):
        result.Add(TriggerFiring { Effect: se, Event: e })

    se.TriggerHighWaterMark = newHighWater   // advance past all seen, matched or not

  return await Order(result, GameDefinition.TriggerResolutionOrder)
```

`Order` applies the configured sort (`OldestFirst`/`OldestLast`) or posts a `TriggerOrderPrompt` (`PromptPlayer`) and awaits the player's response. The method is `async` to accommodate the prompt path.

Called at step 7 of the post-action sequence (D7).

---

**Condition evaluation context.** When evaluating a trigger's `Condition` against a candidate event `e`:

- The candidate event's `BoundArgs` are made available as named values by the `EventParams` mapping: `ParameterRef(paramName)` resolves to `e.BoundArgs[argName]`. Resolved before any other binding source.
- The evaluator has read access to the event log up to the declared `Scope` for `events-matching` queries.
- The evaluator has read-only `GameState` access for property keyword evaluation.
- No block-scope `ExecutionContext` bindings exist. The condition may only reference `EventParams`-declared names and game-state/log reads.

This is handled by `TriggerEvaluationContext` — a lightweight struct distinct from `ExecutionContext`.

---

**Firing a trigger:**

```
async Task FireTrigger(TriggerFiring firing, ExecutionContext parentCtx)
  se    = firing.Effect
  event = firing.Event

  // Always pre-populate the reserved trigger_event binding
  bindings = { "trigger_event": EventRef(event) }

  // Also apply any convenience EventBindings the game creator declared
  for each b in se.Trigger.EventBindings:
    bindings[b.BlockVarName] = event.BoundArgs[b.EventArgName]

  // Increment fire count BEFORE executing so CheckLifetimes sees the updated count
  se.TriggerFireCount++

  // New child action context (new ActionScopeId); inherits GameState and channels
  ctx = parentCtx.CreateChildActionContext(prePopulatedBindings: bindings)

  // Post-action sequence step 9: execute → CheckLifetimes → RunStateBasedRules
  await ExecuteBlock(se.Trigger.FiredBlock, ctx)
  CheckLifetimes(ctx.GameState)
  await RunStateBasedRules(ctx)
```

`TriggerFireCount` is incremented before `ExecuteBlock` so the immediately-following `CheckLifetimes` call correctly evaluates any `TriggerCount` lifetime condition.

---

**Domain model gaps flagged by this decision:** Both resolved.

1. **`events-matching` built-in** — resolved as A2. Added to §9.2 with `EventScope` type, optional `candidate`-scoped predicate, and collection primitives (`count`, `any`, `sum-arg`) in §9.4.

2. **`EventRef` type and `event-arg` primitive** — resolved as A3. `EventRef` defined as a first-class value type in §7.1; `event-arg` added to §9.2. `trigger_event` documented as a reserved name in §4.3.

**Rationale:**
- Per-event evaluation (rather than per-batch) is required by §5.3's "at most once per event" rule. The high-water mark with per-event iteration is the minimal mechanism to guarantee this.
- Advancing the high-water mark past non-matching events prevents re-evaluation in the next cascade pass, which would cause spurious fires when conditions change.
- `EventKeyword` as a required field limits candidate evaluation to O(events_for_keyword × triggers_watching_keyword) rather than O(all_events × all_triggers).
- `TriggerFireCount` is incremented before `ExecuteBlock` — not after — so a `TriggerCount(1)` condition (expire after one fire) is caught by the immediately-following `CheckLifetimes`, not deferred.
- `PromptPlayer` uses the existing `IPromptChannel` rather than a new interface, consistent with the established pattern that all player-input requests flow through one channel.
- `trigger_event` as a reserved always-present binding eliminates any case where a fired block is blind to its own cause. `EventBindings` remains as ergonomic sugar, not as the only access path.

**Consequences:**
- `StaticEffect` gains `TriggerHighWaterMark: long` alongside `TriggerFireCount: int`.
- `CollectSatisfiedTriggers` is now `async` to accommodate the `PromptPlayer` path.
- `CollectSatisfiedTriggers` mutates `TriggerHighWaterMark` on all evaluated effects. It must run exactly once per cascade iteration.
- `PromptContext` becomes a discriminated union with (at minimum) `ChoicePrompt` (existing, for mid-block choices) and `TriggerOrderPrompt` (new). The implementer must handle both in `IPromptChannel` implementations.
- `TriggerEvaluationContext` is a new lightweight struct: `{ EventParams: Dictionary<string,object>, GameState, LogScope }`. Distinct from `ExecutionContext`.
- `CreateChildActionContext` allocates a new `ActionScopeId`, copies the `GameState` reference and channel references, and accepts a pre-populated bindings dictionary. It does not inherit parent block-scope bindings.
- `EventRef` wraps a `GameEvent` reference. It is a value in the type system, not a `KeywordNode` subtype — it is a runtime value that can be stored in bindings and passed to `event-arg`.

---

---

### D9 — Randomness

**Decision:** The engine defines an `IRandomSource` interface. The engine also ships a default `SeededRandom : IRandomSource` implementation wrapping `System.Random(seed)`. The host provides either a seed (engine constructs `SeededRandom(seed)`) or a custom `IRandomSource` (for testing). `IRandomSource` is injected into `ActionResolver` at game construction and flows into every `ExecutionContext`. Two new built-in property keywords expose randomness to game creators: `random-int` and `shuffle`.

---

**`IRandomSource` interface:**

```
interface IRandomSource
  int  NextInt(int minInclusive, int maxInclusive)
  void Shuffle<T>(IList<T> list)   // in-place Fisher-Yates; list is a game-internal list, never external state
```

The engine ships `SeededRandom : IRandomSource`:

```
class SeededRandom : IRandomSource
  Random _rng   // System.Random; constructed once with the provided seed

  int  NextInt(int min, int max)     → _rng.Next(min, max + 1)
  void Shuffle<T>(IList<T> list)     → Fisher-Yates using _rng
```

`System.Random` is WASM-safe and single-threaded by construction (no locking needed). Seed is a `long`; `System.Random` in .NET 6+ accepts a 32-bit seed via its constructor — use `(int)(seed ^ seed >> 32)` to fold a `long` down, or use `Random(seed.GetHashCode())`. The implementer should verify the exact .NET 10 constructor signature.

**Injection.** `IRandomSource` is a construction-time dependency on `ActionResolver` alongside the per-player `IPlayerStrategy` dictionary and `IEngineObserver` (see D14 for the canonical constructor). It is stored on `ExecutionContext` so that the built-in keyword implementations can reach it during evaluation. `TriggerEvaluationContext` does not carry `IRandomSource` — randomness in trigger conditions is not supported (conditions are pure boolean expressions over game state and the event log; introducing randomness there would make trigger firing non-deterministic in a way that is difficult to audit or replay).

---

**New built-in property keywords:**

| Keyword | Parameters | Returns | Notes |
|---|---|---|---|
| `random-int(min, max)` | `min: Number, max: Number` | `Number` | Uniform integer in `[min, max]` inclusive. Consumes one `NextInt` call. |
| `shuffle(collection)` | `collection: Collection<Atom>` | `Collection<Atom>` | Returns a new shuffled collection. Consumes N calls where N = `collection.Count`. Does not mutate the source collection. |

Both are **property keywords**: they return values, have no side effects on game state, and append nothing to the event log. Their consumed randomness is implicitly captured in the event log through the `BoundArgs` of the mutation keyword that uses the result — e.g. `modify-accumulator(goblin, "damage", random-int(1, 6))` logs `{delta: 4}`, not `{delta: random-int(1,6)}`.

**Domain model note.** `random-int` and `shuffle` are additions to §9.2 (Read Primitives) — resolved as A4. The property keyword invariant ("no side effects") is technically violated in the sense that RNG state advances — however, since RNG is not game state (not queryable, not contribution-tracked, not logged), this is explicitly noted as acceptable in the domain model.

---

**Determinism.** A fixed seed produces a fully deterministic game given the same player inputs. This is valuable for:
- **Testing** — inject a `MockRandomSource` returning controlled values to test specific branches.
- **Replay / debugging** — record the seed and player inputs; the game is exactly reproducible.
- **Fairness** — the host (Godot layer / server) chooses and records the seed; neither player can influence it.

The seed is game-scoped (one `IRandomSource` per `GameSession`). There is no per-block or per-action re-seeding.

**Rationale:**
- `IRandomSource` over raw seed injection gives the testing path without ceremony: inject a mock that returns `[1, 1, 1, 6]` in sequence to reproduce a specific scenario without reverse-engineering a seed.
- Property keyword classification keeps the evaluation model clean. Random values flow through the existing argument evaluation path; no new interpreter logic is needed beyond calling `ctx.RandomSource.NextInt(...)`.
- `Shuffle` as a primitive — rather than composed from `random-int` — avoids requiring game creators to author a Fisher-Yates loop in the DSL, which would be awkward and fragile.
- Excluding randomness from `TriggerEvaluationContext` keeps trigger conditions deterministic and auditable. A trigger that fires randomly is a design smell; game creators who want probabilistic triggers can use `random-int` inside the *fired block*, not inside the *condition*.

**Consequences:**
- `ExecutionContext` gains `RandomSource: IRandomSource`.
- `ActionResolver` constructor signature: `(GameDefinition, IReadOnlyDictionary<string, IPlayerStrategy>, IRandomSource, IEngineObserver?)`. `IPromptChannel` from D3 is retired; this is the canonical form established by D14/A15.
- The host's game-session bootstrap must supply a seed or a custom `IRandomSource`. The engine provides a convenience constructor overload: `ActionResolver(GameDefinition, IReadOnlyDictionary<string, IPlayerStrategy>, long seed, IEngineObserver?)` that constructs `SeededRandom(seed)` internally.
- `random-int` and `shuffle` implementations live in the built-in keyword registry alongside other primitives.
- The game creator API (D10) must expose seed/`IRandomSource` as part of game session construction.

---

---

### D10 — Card Visibility and Orientation (Deliberate Non-Decision)

**Decision:** The engine has no concept of card visibility, face-down orientation, or per-player information asymmetry. These are entirely the game creator's responsibility.

**Rationale:**
- Face-down-ness is trivially modeled as a game-creator-named condition (e.g. `apply-condition(card, "face-down")`). The built-in condition system (D5) already provides everything needed: apply, remove, query with `is-face-down` as a property keyword composed from `get-state`.
- "Hidden from opponent" is a property of *zone membership*, not of the card itself. A card in a hand zone is hidden by virtue of being in that zone — the game creator models this via `in-zone` queries, not a visibility flag.
- Per-player visibility (e.g. "face-down but the owner can still see it") is game-specific, varies widely, and quickly becomes entangled with game rules. No single engine model fits all games.
- The host (Godot layer) is responsible for rendering decisions: it queries game state (zone, conditions) and decides what each player sees. The engine is single-source-of-truth with no per-player filtering.
- Prompts: if face-down cards should not be targetable, the game creator writes targeting criteria that exclude them (e.g. `not(get-state(target, "face-down"))`). The engine evaluates criteria; the host renders candidates.

**Consequences:**
- No built-in `is-face-down` keyword, `flip-face-down` mutation, or visibility-related primitives.
- No per-player game state views in the engine.
- Game creators who need orientation track it as a condition; game creators who need visibility track it as a combination of zone membership and conditions.
- This decision is final. If a future requirement surfaces that genuinely cannot be modeled with conditions and zones, it should be treated as a requirements change and reviewed by the domain modeler before any engine concept is added.

---

---

### D11 — Text Rendering Pipeline

**Decision:** The text renderer is a separate, read-only pass over the same `KeywordNode` trees used by the execution interpreter. It produces a structured `RenderNode` tree rather than a flat string, so the host can traverse and render at whatever depth and format it chooses. Two render modes exist — definition-time (unbound parameters render as labels) and invocation-time (bound parameters render as values) — handled by the same renderer with an optional bindings dictionary. The engine makes no assumptions about language; localization is handled via an optional locale file (a flat string map) injected at renderer construction, with `TextTemplate` serving as a language-neutral fallback.

---

**`RenderNode` tree:**

```
RenderNode (abstract record)
  ├── TextSpan(text: string)
  │     A leaf node: a literal string fragment, a substituted template, or a parameter label.
  │
  ├── CompositeNode(summary: RenderNode, body: RenderNode)
  │     Represents a composite keyword invocation.
  │     summary — produced from TextTemplate (if present) or a default structural summary.
  │     body    — always the full recursive expansion of the keyword's composition tree.
  │     The host chooses whether to show summary only, body only, or summary with expandable body.
  │
  └── SequenceNode(items: IReadOnlyList<RenderNode>)
        An ordered list: effect block steps, argument lists, etc.
        Separator and list formatting are host responsibilities.
```

The host's simplest implementation: walk the tree, emit `TextSpan.text` values, ignore `CompositeNode.body`. More capable implementations expand composites on hover, render steps as a numbered list, etc.

---

**Two render modes.** The renderer accepts an optional `IReadOnlyDictionary<string, object>? bindings` parameter:

- **Definition-time** (`bindings == null`): `ParameterRef` nodes render as their declared parameter name, optionally formatted by the enclosing `TextTemplate`'s placeholder. Produces stable card text for display before the card is played. Pre-computable and cacheable.
- **Invocation-time** (`bindings != null`): `ParameterRef` nodes are substituted with their bound values. `Literal` nodes render as their value. Used for "what just happened" log displays or previewing an effect after targets are chosen.

Both modes produce a `RenderNode` tree. The distinction is only in how `ParameterRef` is resolved.

---

**Template resolution order.** For any `KeywordDefinition`, the renderer resolves a template string as follows:

1. **Locale file** — if a locale file is loaded and contains an entry keyed by `keyword.Name`, use that template.
2. **`TextTemplate` fallback** — if no locale entry, use the `TextTemplate` string on the definition (if present). This is the game creator's primary-language text and the fallback for any locale that hasn't been translated.
3. **Structural rendering** — if neither is present, generate a default summary from the keyword name and its rendered arguments (e.g. `"take-damage(goblin, 3)"`).

In all cases, `CompositeNode.body` is always the recursive structural expansion — the locale and template only affect `CompositeNode.summary`.

Primitives have a registered default `TextTemplate` in the built-in keyword registry (step 2). Game creators who want custom text wrap primitives in named composite keywords with their own templates — which is the expected authoring pattern.

---

**`TextRenderer` class:**

```
class TextRenderer
  constructor(KeywordRegistry registry)

  RenderNode Render(KeywordNode node,
                    IReadOnlyDictionary<string, string>? localeStrings,
                    IReadOnlyDictionary<string, object>? bindings)
  RenderNode RenderBlock(EffectBlockDef block,
                         IReadOnlyDictionary<string, string>? localeStrings,
                         IReadOnlyDictionary<string, object>? bindings)
  RenderNode RenderStaticEffect(StaticEffectDef effect,
                                IReadOnlyDictionary<string, string>? localeStrings,
                                IReadOnlyDictionary<string, object>? bindings)
  RenderNode RenderLifetimeSpec(LifetimeSpec spec,
                                IReadOnlyDictionary<string, string>? localeStrings)
```

`localeStrings` is passed per call — a flat `Dictionary<string, string>` mapping lookup keys to locale-specific template strings for the desired locale. `null` means no locale; the renderer falls through to `TextTemplate` and structural rendering. Passing a different dictionary object is all that is required to switch locale at runtime — the host manages locale file loading and passes the appropriate dictionary on each render call.

`TextRenderer` is stateless beyond its internal cache (see Caching below) and the registry. No `GameState`, no `ExecutionContext`. One instance serves all locales for the lifetime of the game.

`RenderBlock` produces a `SequenceNode` of one `RenderNode` per step in the block.

`RenderStaticEffect` produces a `SequenceNode` containing: the rendered state contribution (if any), the rendered trigger (if any), and the rendered lifetime spec (if non-permanent). A permanent `LifetimeSpec` (no conditions) produces no lifetime node in the sequence — permanent effects carry no user-facing duration text. For declarative static effects, this is what appears as the card's ability text.

`RenderLifetimeSpec` uses reserved engine locale keys (looked up in `localeStrings` first, then falling back to the engine's registered defaults):

| Reserved key | Engine default | Placeholders |
|---|---|---|
| `engine.lifetime.turn_timer` | `"for {n} turn(s)"` | `{n}` |
| `engine.lifetime.trigger_count` | `"(up to {n} time(s))"` | `{n}` |
| `engine.lifetime.while_condition` | `"while {expr}"` | `{expr}` — the rendered expression |
| `engine.lifetime.or_separator` | `" or "` | none |

A locale file that wants to localise lifetime descriptions includes entries for these reserved keys alongside keyword name entries. The engine's registered defaults are not prescribed as English — they happen to be English in the reference implementation, but the game creator can override all of them in any locale file, including the "primary" one.

---

**Localization.**

Locale files are flat JSON files — `Dictionary<string, string>` — mapping lookup keys to locale-specific template strings with `{paramName}` placeholders. Keys are either keyword names (matching `KeywordDefinition.Name`) or reserved `engine.*` keys for engine-level strings.

Example locale file (`locale.fr.json`):
```json
{
  "take-damage":                   "inflige {amount} blessure(s) à {target}",
  "attack":                        "{attacker} attaque {target} pour {amount}",
  "engine.lifetime.turn_timer":    "pendant {n} tour(s)",
  "engine.lifetime.while_condition": "tant que {expr}",
  "engine.lifetime.or_separator":  " ou "
}
```

**Separate file per locale.** Each language is a separate file. The host is responsible for loading locale files and managing which dictionary is current. It may load all files upfront, load lazily on first switch, or reload from disk — the engine does not prescribe a loading strategy. The host passes the current locale dictionary on each render call; switching locale mid-session requires no engine interaction beyond passing the new dictionary. If the host passes `null`, the renderer uses `TextTemplate` values as written by the game creator (which may themselves be in any language). For a single-language game, the game creator sets `TextTemplate` directly and never creates a locale file.

**The engine has no default locale.** No language is hardcoded in the engine. The `engine.*` defaults in the built-in registry happen to be English in the reference implementation, but a game creator can override every one of them in their locale files — including whatever they treat as their primary language.

**Static property strings** (card names, atom names, zone names) are not handled by the text renderer — those are static data values on `CardDefinition`, `ZoneDefinition`, etc. If the game creator needs localised names, they author them as locale-keyed properties in the game definition and the host resolves them at render time. This is a tooling convention, not an engine mechanism.

**Tooling implications.** The authoring tool must:
- Allow game creators to author `TextTemplate` in their primary language.
- Support adding locale files per language, editing template strings per keyword per locale.
- Validate that every `{paramName}` placeholder in a locale file string matches a declared parameter name on the corresponding keyword (same validation as `TextTemplate`).
- Warn when a locale file is missing entries for some keywords (incomplete translation).

---

**Caching.** Definition-time `RenderNode` trees are stable for a given `(KeywordDefinition, localeStrings)` pair. The renderer maintains an internal cache using `ConditionalWeakTable<IReadOnlyDictionary<string,string>, Dictionary<KeywordDefinition, RenderNode>>` — keyed by locale dictionary *reference*. The host creates one dictionary object per locale and reuses it; the same object always hits the same cache bucket. When the host drops a locale dictionary (e.g. it was replaced by another language), its cache entries are automatically eligible for GC — no explicit invalidation needed. The `null` locale (no localization) has its own separate flat cache `Dictionary<KeywordDefinition, RenderNode>`.

Invocation-time renders (with `bindings != null`) are not cached — they vary per call. `ConditionalWeakTable` is available in .NET 10 and is WASM-safe.

---

**What the host does with a `RenderNode` tree.** The Godot layer:
- For card text display: traverses the tree, emits `TextSpan.text` values, uses `SequenceNode` items as lines or bullet points, and shows `CompositeNode.summary` with an optional expand affordance.
- For detailed tooltip: shows `CompositeNode.body` recursively when the player hovers or taps.
- For event log display: calls `Render` in invocation-time mode with the event's `BoundArgs` as bindings, producing text like "take-damage dealt 3 damage to Goblin."

The host does not call the text renderer at game-critical moments (combat resolution, etc.) — rendering is display-only and never on the execution path.

---

**Rationale:**
- A `RenderNode` tree rather than a flat string preserves the composition structure that D2 deliberately kept — the host can offer "show me how this works" expansion without re-parsing a string.
- Two modes from one renderer keeps the dual-use invariant (§1.1) tight: the same tree structure, the same renderer class, the same `TextTemplate`s serve both static card text and dynamic log display.
- `CompositeNode` always carrying both `summary` and `body` means the host never has to re-invoke the renderer to get more detail — the full tree is always present, the host decides what to show.
- `TextRenderer` being stateless (no `GameState`) keeps it safely usable from the tooling (DSL editor preview) without needing a running game instance.

**Consequences:**
- `KeywordDefinition` gains a cached `RenderNode? DefinitionRender` field (nullable; populated lazily or at startup by a `GameDefinition` build step).
- `EffectBlockDef` and `StaticEffectDef` similarly gain cached definition renders.
- The `TextTemplate` string format (`{paramName}` placeholders) must be validated by the tooling at parse time: every `{name}` must match a declared parameter name.
- Built-in keyword registrations include a default `TextTemplate` string alongside their C# implementation.
- `RenderNode` is pure data — no behaviour, no engine dependencies. It can be serialized if needed (e.g. for the tooling preview pipeline).

---

### D12 — Runtime Atom Creation

**Decision:** Three mutation primitives — `create-card`, `copy-card`, and `create-zone` — enable atoms to be created during play (for tokens, copies, and dynamic zones). No parameterized card or zone definitions; post-creation mutation via the existing modifier, accumulator, and condition system handles variable properties. All three primitives return an `Atom` value captured via the `BindTo` mechanism added in the D3 addendum.

**New primitives (additions to §9.1 of the domain model):**

| Keyword | Parameters | Returns | Description |
|---|---|---|---|
| `create-card` | `zone: Zone, definition-name: CardDefinitionName, owner: Player` | `Atom` | Instantiates a new card from the named definition; places it in the specified zone with the given owner. Owner is set at creation and immutable thereafter. Appends a creation event. |
| `copy-card` | `source: Atom, destination-zone: Zone, owner: Player` | `Atom` | Instantiates a card using the same definition as `source`. Copies no runtime state — the new card starts fresh (no modifiers, accumulators, or conditions from `source`). Appends a creation event. |
| `create-zone` | `owner: Player, definition-name: ZoneDefinitionName` | `Atom` | Instantiates a zone from the named zone definition; initially empty. Appends a creation event. |
| `move-card` | `card: Card, destination: Zone` | `void` | Moves an existing card to the specified zone. Captures `origin = card.ZoneId` before mutation, updates `card.ZoneId = destination`, and appends a `move-card` event with `{ card, origin, destination }` bound args. The card's `AtomId`, owner, accumulators, modifiers, conditions, and active static effects are unchanged. Post-block `CheckLifetimes` re-evaluates `in-zone` while-conditions naturally — no special handling in `move-card`. If `destination` does not resolve to an active zone atom in `GameState`, a runtime `EngineException` is thrown. |

**`CardDefinitionName` and `ZoneDefinitionName`** are new entries in the type vocabulary — string-valued types that the tooling validates at parse time against the named definitions registered in `GameDefinition`. They are resolved to definition references at game-definition load time; the engine performs no name lookups at execution time.

**Why no parameterized definitions.** Card and zone definitions are design-time data. Adding runtime parameters to them blurs the design-time/runtime boundary established in D2 and propagates complexity into the JSON schema, the tooling, and the text renderer. The post-creation pattern handles variable properties cleanly with existing machinery:

```
// "Create an X/X Elemental Token"
new-card = create-card(zone, "elemental-token", owner)   // base attack 0, base health 0
apply-modifier(new-card, "attack", additive, X, permanent)
apply-modifier(new-card, "health", additive, X, permanent)
// get-state(new-card, "attack") now returns X
```

The one thing post-creation mutation cannot change is a card's *static* properties (name, art, type tags) — those are fixed by the definition. For tokens and dynamic cards this is not an issue: the variable quantities are always mutable properties. If a future game requirement genuinely needs runtime-determined static properties, treat it as a requirements change and route through the domain modeler.

**`copy-card` semantics.** A copy shares the source card's *definition* (same static properties, same effect blocks, same text) but carries no runtime state. This matches the conventional card-game meaning of "copy": mechanically identical to the original but does not inherit counters, damage, conditions, or active static effects. If a game creator needs to transfer runtime state from source to copy, they do so with explicit mutation keywords after creation.

**Domain model gaps flagged by this decision:** All four resolved.

1. `create-card`, `copy-card`, `create-zone` — resolved as A5. Added to §9.1 (Mutation Primitives) with Returns column.
2. `CardDefinitionName` and `ZoneDefinitionName` — resolved as A6. Defined as string-valued types in §9.1 notes; validated at authoring time, resolved at load time.
3. Ownership timing — resolved as A7. §2.4 now reads "set at the moment of creation" — immutability is the invariant, not the timing.
4. Zone lifecycle — resolved as A8. §2.3 now states zones created during play via `create-zone` are never destroyed; inactive zones are modeled via conditions.

**Rationale:**
- Three primitives cover the three creation patterns that arise in practice (named token, clone, dynamic zone) without over-engineering.
- Returning `Atom` from all three unifies the usage pattern and follows the same `BindTo` model as `apply-modifier` / `apply-condition`.
- Deferring parameterized definitions keeps the definition data model clean and avoids propagating new complexity into D2, D11, and the forthcoming game creator API.

**Consequences:**
- `GameDefinition` carries named `CardDefinition` and `ZoneDefinition` registries (maps from name to definition) alongside the `StateBasedRules` list noted in D7.
- At game-definition load time, `CardDefinitionName` and `ZoneDefinitionName` values in `KeywordNode` trees are validated against these registries. An unknown name is a load-time error, not a runtime error.
- `create-card` and `copy-card` log the same `create-card` event type; the resolved definition reference appears in `BoundArgs`. The distinction between the two primitives is an authoring convenience, not an observable event-log difference.
- Declarative static effects on a card definition are provisioned on any card instance created from that definition — whether at game setup or dynamically during play — using the shared `ProvisionDeclarativeEffect` helper (D6). Effects whose while-condition is initially true activate immediately; effects whose while-condition is initially false begin dormant. The static effect lifecycle (D6) manages all subsequent transitions identically regardless of when the card was created.
- `create-zone` implies that `ZoneDefinition` is a first-class record in `GameDefinition` alongside `CardDefinition`. The game creator API (D14) will define how both are authored and registered.

---

### D13 — Keyword Parameter Modifications

**Decision:** Static effects may carry a `ParameterModification` that intercepts mutation keyword invocations before execution. There are two variants: `ParameterAdjustment` (modifies argument values) and `Disable` (cancels the invocation entirely). Numeric adjustments follow the same additive-then-multiplicative ordering as the state modifier system (D5), ensuring stacking effects compose without order dependence. A `Disable` produces a `keyword-disabled` engine event in the log rather than the normal event, making suppression observable by triggers and semantically distinct from "the keyword executed with a value of zero."

---

**Motivation.** The contribution system (D5) provides modifiers on *static properties* — values the game creator reads explicitly via property keywords. A "damage reduction" effect modeled as a modifier requires the game creator to always route damage through a composite keyword that reads it. If any path calls `take-damage` directly without consulting the modifier, the effect is silently bypassed. Parameter modification is the engine-level guarantee that the interception applies to every invocation of the named keyword regardless of call depth.

Additionally, multiplying a numeric argument by zero is mechanically equivalent to cancellation but semantically different: the keyword still executed, its event is still logged, and any trigger watching for that keyword will still fire. `Disable` is a distinct concept: the invocation never happens, a `keyword-disabled` event is logged instead, and triggers on the original keyword do not fire.

---

**`ParameterModification` (discriminated union):**

```
ParameterModification:
  | ParameterAdjustment {
      TargetKeyword   : string
      ArgFilter       : List<EventParamDecl>?   // which args to expose to FilterCondition
      FilterCondition : KeywordNode?            // optional boolean; if false, skip
      ParamMods       : List<ParamMod>
    }
  | Disable {
      TargetKeyword   : string
      ArgFilter       : List<EventParamDecl>?
      FilterCondition : KeywordNode?
    }
```

**`ParamMod`:**

```
ParamMod {
  ParamName  : string                       // declared parameter name of the target keyword
  Kind       : Additive | Multiplicative | Replace
  Expression : KeywordNode
}
```

- **`Additive`** — `Expression` evaluates to a numeric delta. All active additive mods for the same parameter are summed and added to the original invocation value. `ParameterRef("original")` in the expression resolves to the raw invocation argument.
- **`Multiplicative`** — `Expression` evaluates to a numeric factor. All active multiplicative mods for the same parameter are multiplied together and applied to the post-additive result. `ParameterRef("original")` in the expression also resolves to the raw invocation argument (not the post-additive result).
- **`Replace`** — `Expression` evaluates to the new value outright. Applied in `StaticEffectId` ascending order after all additive and multiplicative mods; each Replace mod sees the previous result as `ParameterRef("original")`. Valid for numeric and non-numeric parameters.

The formula for a numeric parameter mirrors D5's modifier evaluation:

```
post_additive      = raw_arg + Σ(additive expressions)
post_multiplicative = post_additive × Π(multiplicative expressions)
final              = Replace pipeline applied to post_multiplicative
```

Within the additive and multiplicative groups, ordering is by `StaticEffectId` ascending, but since addition and multiplication are commutative, that ordering does not affect the result. It matters only for Replace mods (pipeline semantics) and for determinism in logging.

---

`ParameterModification` is a fourth optional field on `StaticEffect` alongside `StateContribution`, `Trigger`, and `LifetimeSpec`.

**`StaticEffect` record (updated from D6):**

```
StaticEffect {
  Id                    : StaticEffectId
  OwnerAtom           : AtomId               // atom this effect is defined on (D13)
  Origin                : Declarative | Dynamic
  SourceDefinition      : StaticEffectDef?       // non-null for declarative effects; null for dynamic (A1/D6)
  LifetimeSpec          : LifetimeSpec
  TriggerFireCount      : int
  TriggerHighWaterMark  : long
  StateContribution     : ContributionId?
  Trigger               : TriggerDefinition?
  ParameterModification : ParameterModification? // D13
  OwnedContributions    : List<ContributionId>
}
```

`OwnerAtom` is the atom (card, player, or zone) on which this static effect lives. For declarative effects it is the card instance; for dynamic effects it is the atom in whose effect block the standing-mutation keyword was invoked. This field also resolves the same latent gap in D8's trigger evaluation — see below.

---

**Evaluation context for modification expressions.** When evaluating `FilterCondition` or any `ParamMod.Expression`:

- **`source`** — reserved name; resolves to `OwnerAtom`. Lets a declarative static effect refer to "this card" (e.g. `equal-to(target, source)`).
- **`original`** — reserved name; resolves to the raw invocation argument for Additive/Multiplicative expressions, or the running result of preceding Replace mods for Replace expressions.
- **Arg values by name** — the invocation's arguments, exposed via `ArgFilter`'s `ParamName` declarations.
- **GameState** — for property keyword reads.
- **No event log access.** Expressions are evaluated synchronously at dispatch time, before the invocation is logged. History-dependent modifications should track state via accumulators.

---

**Where interception happens.** `ApplyParameterModifications` is called in the keyword evaluator at every mutation keyword dispatch point — including nested invocations within composite keywords, not just block-step level.

Updated keyword evaluator (extends D3):

```
object EvaluateNode(KeywordNode node, EvalContext ctx):
  ...
  case Invocation(name, argNodes):
    args = argNodes.Select(n => EvaluateNode(n, ctx)).ToList()
    if IsMutationKeyword(name):
      result = ApplyParameterModifications(name, args, ctx)
      if result is CANCELED: return void           // Disable fired — do not dispatch
      return DispatchMutation(name, result, ctx)
    else:
      return DispatchProperty(name, args, ctx)
```

---

**`ApplyParameterModifications` algorithm:**

```
Result ApplyParameterModifications(string keyword, List<object> args, EvalContext ctx):

  // Collect matching active effects, ordered oldest-first.
  // Evaluate each filter condition; keep only those that pass.
  matching = []
  for each se in ctx.GameState.ActiveStaticEffects
               where se.ParameterModification?.TargetKeyword == keyword
               ordered by se.Id ascending:
    pm = se.ParameterModification
    evalBindings = { "source": se.OwnerAtom }
    if pm.ArgFilter != null:
      for each decl in pm.ArgFilter:
        evalBindings[decl.ParamName] = args[IndexOf(decl.ArgName, keyword)]
    if pm.FilterCondition == null
       OR EvaluateProperty(pm.FilterCondition, evalBindings, ctx.GameState):
      matching.Add((se, pm, evalBindings))

  // Step 1: Disable check — any matching Disable cancels the invocation.
  if any (_, pm, _) in matching where pm is Disable:
    log keyword-disabled event: { keyword: keyword, ...bound args }
    return CANCELED

  // Step 2: Apply ParameterAdjustments, per parameter, in Additive → Multiplicative → Replace order.
  for each parameter p declared on keyword:
    raw = args[IndexOf(p, keyword)]

    // Additive: sum all deltas; each expression sees "original" = raw invocation value
    additiveSum = Σ( EvaluateProperty(mod.Expression, evalBindings + {"original": raw}, ctx.GameState)
                     for (se, pm, evalBindings) in matching
                     for mod in pm.ParamMods
                     where mod.ParamName == p AND mod.Kind == Additive )
    result = raw + additiveSum

    // Multiplicative: multiply all factors; each expression also sees "original" = raw invocation value
    multiplicativeProduct = Π( EvaluateProperty(mod.Expression, evalBindings + {"original": raw}, ctx.GameState)
                                for (se, pm, evalBindings) in matching
                                for mod in pm.ParamMods
                                where mod.ParamName == p AND mod.Kind == Multiplicative )
    result = result × multiplicativeProduct

    // Replace: pipeline in StaticEffectId order; each expression sees "original" = running result
    for each (se, pm, evalBindings) in matching:
      for each mod in pm.ParamMods where mod.ParamName == p AND mod.Kind == Replace:
        result = EvaluateProperty(mod.Expression, evalBindings + {"original": result}, ctx.GameState)

    args[IndexOf(p, keyword)] = result

  return args
```

---

**`keyword-disabled` engine event.** When a `Disable` fires, the engine synthesizes and appends a `GameEvent` with `KeywordName: "keyword-disabled"` to the current block scope accumulator. Its `BoundArgs` always contains:
- `"keyword"` — the name of the suppressed keyword
- One entry per bound argument of that keyword invocation (same keys as would have appeared in the normal event's `BoundArgs`)

This event uses the same `GameEvent` structure as all other events and is fully visible to the trigger and event-log query systems. Game creators write trigger conditions on `EventKeyword: "keyword-disabled"` with an arg filter on `"keyword"` to react to specific suppressions (e.g., "whenever damage to this card is prevented, its controller draws a card").

---

**Examples.**

"This unit takes 2 less damage" (Additive):
```
ParameterAdjustment {
  TargetKeyword:   "take-damage",
  ArgFilter:       [ { ArgName: "target", ParamName: "target" } ],
  FilterCondition: equal-to(target, source),
  ParamMods: [ { ParamName: "amount", Kind: Additive, Expression: literal(-2) } ]
}
```

"This unit takes half damage, rounded down" (Multiplicative):
```
ParameterAdjustment {
  ...same filter...
  ParamMods: [ { ParamName: "amount", Kind: Multiplicative, Expression: literal(0.5) } ]
}
```

Both together (separate static effects, different ages): the formula gives `(original − 2) × 0.5`. The additive reduction applies first because the ordering is structural (Additive before Multiplicative), not age-based — so the result is independent of which effect was created first.

"This unit is immune to `take-damage`" (Disable):
```
Disable {
  TargetKeyword:   "take-damage",
  ArgFilter:       [ { ArgName: "target", ParamName: "target" } ],
  FilterCondition: equal-to(target, source)
}
```

When suppressed, the engine logs `keyword-disabled { keyword: "take-damage", target: <this-card>, amount: 3 }` instead of the normal `take-damage` event. Any trigger watching for `take-damage` does not fire; a trigger watching for `keyword-disabled` with `keyword == "take-damage"` may fire.

---

**`source` in trigger conditions (addendum to D8).** The same `OwnerAtom` field resolves the equivalent gap in trigger evaluation. A trigger such as "when *this* card deals damage, draw a card" requires the condition to reference the owning atom. `TriggerEvaluationContext` gains:

```
TriggerEvaluationContext {
  Source     : AtomId                      // NEW — se.OwnerAtom
  EventParams: Dictionary<string, object>
  GameState  : GameState
  LogScope   : TriggerScope
}
```

`ParameterRef("source")` in a trigger condition resolves to `Source`. Existing conditions that do not use `source` are unaffected.

---

**Domain model gaps flagged by this decision:** All four resolved.

1. `ParameterModification` — resolved as A9. Added to §5 as a fourth optional component on a static effect (`ParameterAdjustment` and `Disable` variants, with filter condition). Interception applies at every dispatch point including deep composite invocations.
2. Reserved binding names — resolved as A10. All four reserved names (`trigger_event`, `candidate`, `source`, `original`) consolidated in §4.3.
3. `keyword-disabled` engine event — resolved as A11. Defined in §5.4 and tabulated in §7. Bound args: `"keyword"` plus one entry per suppressed invocation argument.
4. Arithmetic primitives — resolved as A12. `add`, `subtract`, `multiply`, `max`, `min` added to §9.3.

**Rationale:**
- Additive-then-multiplicative ordering mirrors D5's modifier evaluation. Within each group, stacking effects are commutative, so the result is independent of which effect is older. This gives game creators the same compositional guarantees they have for static property modifiers.
- `Disable` is semantically distinct from Multiplicative × 0: it prevents the keyword from executing rather than executing it with a zeroed argument. This distinction is observable in the event log (different event type) and in triggers (normal triggers don't fire). It also carries clearer meaning to the player — "immune" is not the same as "takes 0 damage."
- Intercepting at the evaluator level (every dispatch point) is the only way to make the guarantee hold regardless of composition depth.
- Excluding event log access from modification expressions keeps the interception path synchronous and avoids re-entrant complexity.

**Consequences:**
- `StaticEffect` gains `OwnerAtom: AtomId` (D13), `SourceDefinition: StaticEffectDef?` (A1/D6), and `ParameterModification: ParameterModification?` (D13).
- `TriggerEvaluationContext` gains `Source: AtomId`.
- `ApplyParameterModifications` is called before every mutation dispatch in the keyword evaluator. For games with no `ParameterModification` static effects active, this is a no-op list scan.
- `ArgFilter` reuses `EventParamDecl` from D8 — extract into a shared type.
- `ParamMod.ParamName` validated by tooling against the target keyword's declared parameters. `Kind: Additive | Multiplicative` is valid only on numeric-typed parameters; `Replace` is valid on any type.
- Arithmetic primitives must be registered in the built-in keyword registry once the domain modeler formalizes them.
- A `Disable` from any matching effect cancels the invocation entirely; `ParameterAdjustment` mods from other effects are ignored when a Disable fires. If a game needs "bypass immunity" mechanics, the game creator models it via the `Disable` effect's own `FilterCondition` (e.g., checking for absence of a "piercing" condition on the attacker).

---

### D16 — Testing Strategy

**Decision:** Testing is layered: unit tests for isolated components (Layer 1), block-level integration tests as the primary pattern (Layer 2), and full game session scenario tests for end-to-end coverage (Layer 3). The minimal test harness consists of four hand-written helpers — `ScriptedPlayerStrategy`, `MockRandomSource`, `GameStateBuilder`, and assertion helpers — which must be in place before meaningful testing of any layer can begin. No mocking framework is required.

---

**Layer 1 — Unit tests on isolated components.**

Fast and precise. Each test constructs only what its target needs.

| Target | What to cover |
|---|---|
| `EvaluateNode` (property keywords) | `Literal`, `ParameterRef`, composite recursion, `GetState`/`GetProperty`/`InZone` |
| `ApplyParameterModifications` | Additive+multiplicative ordering (result independent of effect age), Replace pipeline, Disable short-circuit, filter conditions, `source` binding |
| `CollectSatisfiedTriggers` | High-water mark advancement (non-matching events also advance), condition evaluation with `EventParams`, `OldestFirst`/`OldestLast` sort, `PromptPlayer` routing |
| `CheckLifetimes` | WhileCondition expiry, TurnTimer decrement, TriggerCount expiry, cascade (expiry of one effect causes re-check that expires another), permanent effects unaffected; terminal vs. while-condition expiry classification (TurnTimer/TriggerCount = discard, only-WhileCondition + declarative = dormant); Phase 2 dormant activation (while-condition becomes true → new instance with fresh counters); re-activation cascade (new active effect alters another condition) |
| `RunStateBasedRules` | Fixpoint termination when no rules trigger, multi-pass when rules cascade, rule registration order respected |
| `TextRenderer` | `RenderNode` structure, locale > `TextTemplate` > structural resolution order, definition-time vs invocation-time modes, `SequenceNode` for blocks |

---

**Layer 2 — Block-level integration tests (the primary pattern).**

The key harness: construct a `GameState` directly, execute a single `EffectBlockDef` with a scripted strategy, assert the resulting state and event log. This is the fastest path to testing a keyword's correctness and the primary pattern game creators use when verifying their own definitions.

Example structure:

```
// Arrange
var state   = new GameStateBuilder()
    .WithPlayer("player1", out var player1Id)
    .WithZone("hand", "player1", out var hand)
    .WithCard("goblin", hand, "player1", out var goblin)
    .Build();

var strategy = new ScriptedPlayerStrategy();  // no queued inputs needed for this block
var block    = /* EffectBlockDef: take-damage(goblin, 3) */;

// Act
await blockExecutor.ExecuteBlock(block, ctx);

// Assert
AssertAccumulator(state, goblin, "damage", 3);
AssertEvent(log, "take-damage", ("target", goblin), ("amount", 3));
```

This pattern covers in a single test:
- Keyword evaluation (argument nodes resolved correctly)
- Mutation dispatch (game state changes applied)
- Event log structure (correct `KeywordName`, correct `BoundArgs`, correct tree nesting)
- `BindTo` (return values bound to variables for use by later steps)
- Parameter modifications (if any static effects are pre-loaded via `GameStateBuilder.WithStaticEffect(...)`)

**Testing the dual-use invariant.** Layer 2 is also where the dual-use property (§1.1 of the domain model) is verified: the same `KeywordDefinition` is used for both execution and rendering in the same test.

```
// Execution path:
await blockExecutor.ExecuteBlock(blockDef, ctx);
AssertAccumulator(state, goblin, "damage", 3);

// Rendering path (same definition, same test):
var rendered = renderer.Render(keywordDef.Body, localeStrings: null, bindings: null);
AssertRenderContains(rendered, "damage");   // structural fallback present
AssertRenderContains(rendered, "3");        // literal value in tree
```

**Testing mid-effect prompts.** Queue a `PromptResponse` in `ScriptedPlayerStrategy` before executing the block. Assert the variable binding is present in scope for subsequent steps. Assert no events were logged during the pause.

---

**Layer 3 — Full game session scenario tests.**

Slower but end-to-end. Use `GameDefinition.CreateBuilder()` + `GameSession.Create(...)` with scripted strategies and a seeded `MockRandomSource`. Each scenario tests a specific mechanic across the full post-action sequence.

| Scenario | What it validates |
|---|---|
| Trigger fires after action | Post-action sequence (D7), `CollectSatisfiedTriggers`, `TriggerHighWaterMark` advancement |
| SBR cascade | Fixpoint loop reaches stable state; SBRs run before triggers |
| Static effect expires mid-game | `CheckLifetimes` sweep, contribution auto-removal, while-condition evaluated after each block |
| Declarative effect re-activation | Effect goes dormant on while-condition expiry; card returns to play; next `CheckLifetimes` Phase 2 creates a new instance with `TriggerFireCount = 0` and `TriggerHighWaterMark = 0`; original expired instance's trigger history is not inherited |
| Trigger-count lifetime | Effect expires after N firings; next `CheckLifetimes` catches it |
| Win/loss condition | SBR produces outcome; `GameResult` populated correctly |
| Parameter modification stacking | Two additive effects + one multiplicative: result is `(base + Σ additives) × factor` |
| Disable prevents trigger | Disabled invocation logs `keyword-disabled`; trigger on the original keyword does not fire; trigger on `keyword-disabled` fires |
| `PromptPlayer` trigger ordering | Multiple simultaneous triggers prompt the player; scripted response controls order |
| `copy-card` starts fresh | Copied card has no accumulators, conditions, or static effects from source |

---

**The minimal test harness.**

These four helpers are required before any layer of testing is viable. They live in `Archetype.Tests` and use `InternalsVisibleTo` access to `Archetype.Engine`.

**`ScriptedPlayerStrategy : IPlayerStrategy`**

```
class ScriptedPlayerStrategy : IPlayerStrategy
  Queue<PlayerAction?> _actions
  Queue<PromptResponse> _responses

  ScriptedPlayerStrategy QueueAction(PlayerAction? a)   → self
  ScriptedPlayerStrategy QueueResponse(PromptResponse r) → self

  Task<PlayerAction?>    SelectActionAsync(...)   → _actions.Dequeue()
  Task<PromptResponse>   RespondToPromptAsync(...)→ _responses.Dequeue()
  // Throws InvalidOperationException if queue is empty — test setup error, not engine error
```

**`MockRandomSource : IRandomSource`**

```
class MockRandomSource : IRandomSource
  Queue<int> _values

  MockRandomSource Enqueue(params int[] values) → self

  int  NextInt(int min, int max)    → _values.Dequeue()   // ignores min/max; test controls output
  void Shuffle<T>(IList<T> list)    → uses _values to permute via Fisher-Yates
```

**`GameStateBuilder`**

Constructs a `GameState` directly — bypasses manifest provisioning, allocates real `AtomId`s from a fresh counter. Essential for Layer 1 and Layer 2 speed.

```
class GameStateBuilder
  .WithPlayer(string playerName, out AtomId id,
              Dictionary<string,object>? staticProps = null)                              → self
  .WithZone(string defName, string ownerPlayerName, out AtomId id)                     → self
  .WithCard(string defName, AtomId zone, string ownerPlayerName, out AtomId id)      → self
  .WithAccumulator(AtomId atom, string name, double value)                            → self
  .WithCondition(AtomId atom, string conditionName)                                   → self
  .WithModifier(AtomId atom, string prop, ModifierKind kind, double value)            → self
  .WithStaticEffect(StaticEffectDef def, AtomId ownerAtom)                           → self
  .WithSession(out AtomId id, Dictionary<string, double>? accumulators = null,
               IReadOnlyList<string>? conditions = null)                                  → self
  .Build() → GameState   // auto-provisions session atom if WithSession not called
```

`WithCard` instantiates declarative static effects from the `CardDefinition` automatically, matching production provisioning behaviour.

**Assertion helpers**

```
static class Assert
  // Event log
  .EventLogged(IReadOnlyList<GameEvent> log, string keyword,
               params (string param, object value)[] args)
  .NoEventLogged(log, string keyword)
  .EventLoggedDisabled(log, string originalKeyword)  // keyword-disabled event

  // Game state
  .Accumulator(GameStateView state, AtomId atom, string name, double expected)
  .ConditionPresent(state, AtomId, string conditionName)
  .ConditionAbsent(state, AtomId, string conditionName)
  .ComputedProperty(state, AtomId, string propName, double expected)  // modifier-adjusted value

  // Render
  .RenderContainsText(RenderNode root, string fragment)    // any TextSpan in tree contains fragment
  .RenderSummary(RenderNode root, string expected)         // top-level CompositeNode.summary text
```

---

**Domain model gaps flagged by this decision:** None.

**Rationale:**
- Centring on Layer 2 (block-level integration tests) means the implementer has a runnable test harness as soon as `ExecuteBlock` exists — before `ActionResolver`, phases, or the full session loop are built. This makes test-first development viable from day one.
- Hand-written harness helpers over a mocking framework: `ScriptedPlayerStrategy` and `MockRandomSource` are simple queue-draining implementations with no framework dependency. They are also easier to read in test output and easier to extend with game-specific helpers.
- `GameStateBuilder` constructing real `GameState` objects (not mocks) means Layer 1 and Layer 2 tests exercise the actual storage structures — `ModifierIndex`, `ConditionIndex`, contribution registry — not a pretend substitute.
- Dual-use invariant tests in Layer 2 are the most important tests architecturally: they verify that D2's central promise (one representation, two uses) holds for each keyword definition.

**Consequences:**
- `Archetype.Tests` is a single test project referencing all four engine assemblies. It has `InternalsVisibleTo` access to `Archetype.Engine` internals. It does not have production code; all four harness types are test infrastructure.
- If game-specific test projects are added later (e.g. a test project for a specific card game's keywords), they should reference `Archetype.Tests` helpers via a shared `Archetype.Tests.Shared` assembly rather than duplicating them.
- Layer 1 tests should complete in under one second in aggregate. Layer 2 tests should complete in under five seconds. Layer 3 scenario tests are permitted to be slower but must be deterministic and never flaky — `MockRandomSource` and `ScriptedPlayerStrategy` guarantee this.
- The `GameStateBuilder.WithCard` method must mirror the manifest provisioning logic (instantiate declarative static effects from the definition). If these diverge, tests will pass against a state that doesn't match production provisioning. A shared provisioning function called by both is the right implementation.

---

### D15 — Module Boundaries

**Decision:** The engine is partitioned into four assemblies. `Archetype.Core` contains all pure data types and interfaces — it has no dependencies beyond the .NET BCL and is WASM-safe by construction. `Archetype.Build`, `Archetype.Text`, and `Archetype.Engine` each depend on `Core` only; none depends on any other of the three. This lets the DSL tooling (a separate desktop application) reference `Core`, `Build`, and `Text` without pulling in the runtime engine.

---

**Dependency graph:**

```
                      Archetype.Core
                  (pure data, interfaces)
                   ↑         ↑         ↑
        ┌──────────┘  ┌──────┘  ┌──────┘
        │             │         │
Archetype.Build  Archetype.Text  Archetype.Engine
(C# authoring)  (text renderer) (runtime + session API)
```

No edges exist between `Build`, `Text`, and `Engine`. All lateral coupling goes through `Core`.

---

**`Archetype.Core`** — pure data types, interfaces, built-in keyword metadata.

No NuGet dependencies. No engine logic. WASM-safe by construction.

*Contents:*
- `KeywordNode` discriminated union (`ParameterRef`, `Literal`, `Invocation`) and `KeywordDefinition`
- `EffectBlockDef`, `EffectBlockStep` (including `BindTo`)
- `StaticEffectDef`, `LifetimeSpec`, `LifetimeCondition`
- `TriggerDefinition`, `EventParamDecl`, `EventBinding`
- `ParameterModification` discriminated union (`ParameterAdjustment`, `Disable`), `ParamMod`
- `ModifierContribution`, `ConditionContribution`, `ContributionId`
- `GameEvent`, `GameResult`
- `RenderNode` discriminated union (`TextSpan`, `CompositeNode`, `SequenceNode`)
- All definition records: `CardDefinition`, `ZoneDefinition`, `PhaseDefinition`, `ActionRuleDefinition`, `NamedEffectBlockDef`, `PlayerDefinition`, `CardSet`, `StateBasedRule`
- `GameDefinition` (the immutable aggregate)
- `InitManifest`, `ZoneSpec`, `CardSpec`, `PlayerStateSpec`
- `PlayerAction` discriminated union, `AvailableActions`, `PlayableCardOption`, `ActivatableAbilityOption`
- `PromptContext` discriminated union (choice prompt, trigger-order prompt — D8), `PromptResponse`
- Interfaces: `IPlayerStrategy`, `IEngineObserver`, `IRandomSource`
- Enums: `TriggerResolutionOrder`, `ModifierKind`, `ParamModKind`, `CascadeDirective` — `PlayerSlot` is retired (A15); players are referenced by string name throughout
- `BuiltInKeywords` — a static registry of all built-in keyword names and their `ParameterDecl[]` signatures (no C# implementations; implementations are in `Engine`). Used by `Build` for authoring validation and by `Engine` for dispatch. Covers all mutation primitives from D12 (including `move-card`), the game outcome primitives (`declare-winner`, `declare-draw`, `player-by-name`) from the D14 addendum, the zone query primitive (`get-atoms-in-zone`) from D19, and all read primitives from §9.2 of the domain model.
- `DefinitionException` — thrown by authoring-time validation failures

*What is not here:* any mutable runtime state, any execution logic, any JSON parsing.

---

**`Archetype.Build`** — C# game definition authoring API.

Depends on `Core` only. No engine dependency. WASM-safe (though it is more likely used at desktop/editor time than in a WASM export).

*Contents:*
- `GameDefinitionBuilder` and sub-builders (`KeywordBuilder`, `CardBuilder`, `ZoneBuilder`, `PhaseBuilder`, `RuleBuilder`, `ActionRuleBuilder`, `PlayerBuilder`, `ManifestBuilder`)
- `Kw` — static factory class for `KeywordNode` trees; one shorthand per built-in keyword
- Validation logic: type-checking `KeywordNode` trees, acyclicity checks, name-resolution against `BuiltInKeywords` (from `Core`)

The builder does not produce JSON and does not load JSON. It produces `GameDefinition` values directly.

*Used by:* the DSL tooling (authoring test cards), test projects (assembling in-code game definitions), and any host that wants to construct a game definition programmatically.

---

**`Archetype.Text`** — text rendering.

Depends on `Core` only. No engine dependency. WASM-safe.

*Contents:*
- `TextRenderer` — walks `KeywordNode` trees and `EffectBlockDef`s; produces `RenderNode` trees (D11)
- Locale file loading helpers (flat `Dictionary<string, string>` construction from JSON text, if the host does not want to manage locale dictionaries manually)

*Used by:* the DSL tooling (card text preview), the Godot host (card display, event log display), test projects. Because `Text` has no dependency on `Engine`, the tooling can render card text without instantiating a game session.

---

**`Archetype.Engine`** — runtime execution and public session API.

Depends on `Core` only. WASM-safe (no `Thread`, no `ThreadPool`, no raw file I/O; `System.Text.Json` is BCL in .NET 10).

*Public surface (accessible to the Godot host and tests):*
- `GameSession`, `GameSessionBuilder`
- `GameStateView` — read-only projection of `GameState`
- `GameDefinitionLoader.FromJson(Stream) → GameDefinition` — the engine-owned JSON deserialiser (D2); validates against `BuiltInKeywords` from `Core`
- `SeededRandom` — the default `IRandomSource` implementation (D9)

*Internal (not public; accessible to tests via `InternalsVisibleTo`):*
- `GameState` — mutable runtime state (atoms, contribution registry, active static effects)
- `ExecutionContext`, `TriggerEvaluationContext`
- `ActionResolver` — owns the post-action sequence (D7), calls `ExecuteBlock`, `RunStateBasedRules`, `CollectSatisfiedTriggers`, `CheckLifetimes`
- Block executor (`ExecuteBlock`) and keyword evaluator (`EvaluateNode`, `ApplyParameterModifications`, `DispatchMutation`, `DispatchProperty`)
- Built-in keyword implementations — registered at startup against names from `BuiltInKeywords`
- Contribution registry and active static effect list
- `AvailableActions` computation (activation condition evaluation, cost dry-run, target enumeration)

`[assembly: InternalsVisibleTo("Archetype.Tests")]` is the only mechanism by which test code accesses engine internals. Nothing in `Core`, `Build`, or `Text` has visibility into `Engine` internals.

---

**Consumer reference sets:**

| Consumer | Core | Build | Text | Engine |
|---|---|---|---|---|
| DSL tooling (desktop app) | ✓ | ✓ | ✓ | — |
| Godot host | ✓ | ✓ | ✓ | ✓ |
| Test projects | ✓ | ✓ | ✓ | ✓ (+ internals) |
| Game creator code (C# game def) | ✓ | ✓ | — | — |

Game creator code (the card/keyword/rule definitions for a specific game) only needs `Core` and `Build`. It does not need to reference `Engine` or `Text` — those are host concerns.

---

**Domain model gaps flagged by this decision:** None.

**Rationale:**
- Isolating `Core` as a pure-data assembly with no dependencies is the single most important structural decision: it keeps the WASM binary small, lets the tooling import types without pulling in the runtime, and provides a stable, dependency-free serialisation boundary (D2's JSON schema maps directly to `Core` types).
- No cross-dependencies between `Build`, `Text`, and `Engine` prevent accidental coupling. The tooling does not need the engine. The renderer does not need the builder. The engine does not need to know about rendering.
- `BuiltInKeywords` as metadata-only in `Core` (names + signatures, no implementations) lets `Build` validate keyword names at authoring time without depending on `Engine`. The engine registers C# implementations at startup by matching against the same names.
- Keeping `GameDefinitionLoader.FromJson` in `Engine` (not `Build`) honours D2's commitment that the engine owns the deserialiser and prevents `Build` from needing a JSON dependency.
- `InternalsVisibleTo("Archetype.Tests")` is the controlled white-box test seam. No other assembly can reach engine internals. External consumers program exclusively against the public session API.

**Consequences:**
- The `.sln` contains four projects: `Archetype.Core`, `Archetype.Build`, `Archetype.Text`, `Archetype.Engine`. Test projects are a fifth. The tooling is a separate solution (or a separate `.sln` file in the same repo) that references the four engine projects via project references or NuGet packages.
- `GameDefinition` is defined in `Core`. Both `GameDefinitionBuilder.Build()` (in `Build`) and `GameDefinitionLoader.FromJson()` (in `Engine`) return a `Core`-typed `GameDefinition`. The host does not need to know which path produced it.
- Built-in keyword implementations in `Engine` must be kept in sync with `BuiltInKeywords` metadata in `Core`. The implementer should enforce this with a startup assertion: on `ActionResolver` construction, verify that every name in `BuiltInKeywords.All` has a registered implementation and no extra names are registered.
- `Kw` shorthands in `Build` must also be kept in sync with `BuiltInKeywords` in `Core`. Same responsibility, same assertion pattern — a `Kw` shorthand that references an unknown built-in name should throw at the point of the `Kw.Invoke` call, not silently produce a broken `KeywordNode`.
- The old assembly names (`Archetype.Core`, `Archetype.Engine`, `Archetype.Builder`, `Archetype.Server`, `Archetype.Design`) are retired. The new names are `Archetype.Core`, `Archetype.Build`, `Archetype.Text`, `Archetype.Engine`. No `Server` assembly exists — the engine is embedded directly into the Godot host, not exposed over a network.
- **GDScript interop note.** The Godot host project will contain C# classes extending Godot's `Resource` (marked `[GlobalClass]`) that wrap engine types for GDScript consumption — e.g. `CardDefinitionResource`, `ZoneResource`, `PlayerStateResource`. These wrappers live entirely in the Godot host layer; the engine has no Godot types in its public surface (D1). The tooling will generate scaffolding for these wrapper classes as a tooling-phase feature, not an engine feature. The consequence for this phase is that `GameStateView` must be designed to be GDScript-friendly: plain C# types, no heavy generics or internal ID types leaking into the projection, so that the wrapper layer can adapt it without fighting the type system.

---

### D14 — Game Creator API

**Decision:** The game creator API centres on a single immutable type, `GameDefinition`, that aggregates all design-time game data. There are two authoring paths — a fluent C# builder and JSON loading from the DSL tooling — both producing identical `GameDefinition` instances. Session-specific initial state is declared via `InitManifest`, a K8s-style desired-state specification the engine provisions without game creators writing creation calls. The host owns the manifest at session creation time; `GameDefinition` provides a `DefaultInitManifest` the host may adopt unchanged or replace. `IPlayerStrategy` is the single per-player interface through which every player interaction flows. A third session-init path — `FromSavedState` — is reserved for the save/load feature (D17).

---

**`GameDefinition` — the immutable aggregate.**

```
GameDefinition {
  Keywords               : IReadOnlyDictionary<string, KeywordDefinition>
  CardDefinitions        : IReadOnlyDictionary<string, CardDefinition>
  ZoneDefinitions        : IReadOnlyDictionary<string, ZoneDefinition>
  CardSets               : IReadOnlyDictionary<string, CardSet>
  StateBasedRules        : IReadOnlyList<StateBasedRule>
  Phases                 : IReadOnlyList<PhaseDefinition>      // in turn order
  ActionRules            : IReadOnlyDictionary<string, IReadOnlyList<ActionRuleDefinition>>
  TriggerResolutionOrder : TriggerResolutionOrder
  PlayerDefinitions      : IReadOnlyDictionary<string, PlayerDefinition>   // named registry; minimum one (A15)
  DefaultInitManifest    : InitManifest?
  PlayableZoneNames      : IReadOnlyList<string>?   // D19: zone definition names from which cards may be played; null = no zone filter
}
```

Built-in keywords are pre-registered in `Keywords` by the engine at construction time. Game creator keywords are layered on top. No game creator keyword may shadow a built-in; the builder and deserialiser enforce this as a `DefinitionException`.

`PlayerDefinition` carries only design-time static properties (display name, avatar reference, etc.). Initial mutable state (health, resources) belongs in the `InitManifest`, not here.

```
PlayerDefinition {
  StaticProperties : IReadOnlyDictionary<string, object>
}
```

**`CardDefinition`:**

```
CardDefinition {
  Name                : string
  StaticProperties    : IReadOnlyDictionary<string, object>
  ActivationCondition : KeywordNode?    // D19: optional; evaluated pure before PlayCard is offered; null = always playable (subject to zone filter)
  PrimaryEffect       : EffectBlockDef
  AdditionalEffects   : IReadOnlyList<NamedEffectBlockDef>
  StaticEffects       : IReadOnlyList<StaticEffectDef>
}

NamedEffectBlockDef {
  Name                : string          // how the host routes player choice to this block
  ActivationCondition : KeywordNode?
  Cost                : EffectBlockDef?
  Body                : EffectBlockDef
}
```

**`ZoneDefinition`:**

```
ZoneDefinition {
  Name             : string
  StaticProperties : IReadOnlyDictionary<string, object>
}
```

**`PhaseDefinition`:**

```
PhaseDefinition {
  Name    : string
  Init    : EffectBlockDef?    // runs at phase start; may be null
  Cleanup : EffectBlockDef?    // runs at phase end; may be null
  // The player action window is implicit between Init and Cleanup
}
```

**`ActionRuleDefinition`:**

```
ActionRuleDefinition {
  Before : EffectBlockDef?
  After  : EffectBlockDef?
}
```

Multiple rules may be registered under the same action name; they are applied in registration order. `ActionRules` maps action names to lists, not single entries.

**`CardSet`:**

```
CardSet {
  Name  : string
  Cards : IReadOnlyList<string>    // card definition names
}
```

Card sets are informational groupings for the tooling and meta-game layer. The engine does not use them during execution.

---

**`InitManifest` — declarative desired state for fresh sessions.**

The host declares what should exist and the engine provisions it — no game creator code calls `create-zone` or `create-card` manually. This matches the meta-game use case naturally: the host knows exactly what should exist (from prior runs, loadouts, procedural generation computed outside the engine) and expresses it as data.

```
InitManifest {
  Zones        : IReadOnlyList<ZoneSpec>
  Cards        : IReadOnlyList<CardSpec>
  PlayerStates : IReadOnlyList<PlayerStateSpec>
}

ZoneSpec {
  LocalId      : string         // manifest-scoped reference ID, used by CardSpec.ZoneLocalId
  Owner        : string         // player name key in GameDefinition.PlayerDefinitions (A15)
  Definition   : string         // ZoneDefinition name
  Accumulators : IReadOnlyDictionary<string, double>?
  Conditions   : IReadOnlyList<string>?
}

CardSpec {
  Owner        : string         // player name (A15)
  ZoneLocalId  : string         // references ZoneSpec.LocalId
  Definition   : string         // CardDefinition name
  Accumulators : IReadOnlyDictionary<string, double>?
  Conditions   : IReadOnlyList<string>?
}

PlayerStateSpec {
  Player       : string         // player name (A15)
  Accumulators : IReadOnlyDictionary<string, double>?
  Conditions   : IReadOnlyList<string>?
}
```

**Provisioning order.** The engine provisions the manifest in this sequence before the first phase:
1. The session atom is created (engine-managed; no manifest entry required — see D14 addendum §2).
2. Player atoms are created from `PlayerDefinitions` in insertion order; each is assigned a fresh `AtomId`.
3. Zones are created in `Zones` list order; each is assigned a fresh `AtomId`. `LocalId` is a manifest-scoped reference only — it is not an engine `AtomId`.
4. Cards are created in `Cards` list order, placed in their declared zone, with their declared owner. Declarative static effects from the card definition are instantiated automatically (D6/D12).
5. Card mutable state overrides (`Accumulators`, `Conditions`) are applied.
6. Player mutable state overrides from `PlayerStates` are applied.

No events are logged during provisioning — the event log is empty when the first phase begins. The `InitManifest` is entirely a setup mechanism, not a game action.

**Relationship to `GameStateSnapshot` (D17).** `GameStateSnapshot` is the richer save/load type. A save may occur at any prompt suspension — including mid-block, mid-action — so the snapshot must capture:

- Atom state (zones, cards, players — accumulators, modifiers, conditions)
- Contribution registry and active static effects (with fire counts and high-water marks)
- Dormant declarative effects (D6) — required for correct re-activation after load
- Full finalized event log
- Current scope hierarchy (turn number, phase index, action scope ID)
- **In-progress block call stack** — each suspended frame carries its step index, its variable bindings, its in-progress event subtree (the unfinalized parent event stack from D4), and the pending prompt context. There may be multiple frames if nested blocks are executing (e.g. cost block inside a main block inside a phase init block)
- RNG position (how many calls into the `IRandomSource` sequence, for deterministic replay from the save point)

Because the only `await` points in the engine are `strategy.RespondToPromptAsync` calls (D3), a save point always occurs at a well-defined prompt suspension — not mid-computation. There is no need to serialize arbitrary C# execution state; only the per-frame block data above is required.

A snapshot of a freshly provisioned game before any moves would be structurally equivalent to what manifest provisioning produces — the manifest is simply the lighter authoring format for that initial case.

---

**Two authoring paths for `GameDefinition`.**

**Path 1 — C# builder (programmatic / testing):**

```
GameDefinition.CreateBuilder() → GameDefinitionBuilder

GameDefinitionBuilder
  .AddKeyword(name, Action<KeywordBuilder>)                → self
  .AddCard(name, Action<CardBuilder>)                      → self
  .AddZone(name, Action<ZoneBuilder>)                      → self
  .AddCardSet(name, params string[] cardNames)             → self
  .AddStateBasedRule(name, Action<RuleBuilder>)            → self
  .AddPhase(name, Action<PhaseBuilder>)                    → self
  .AddActionRule(actionName, Action<ActionRuleBuilder>)    → self
  .WithTriggerOrder(TriggerResolutionOrder)                → self
  .AddPlayer(string name, Action<PlayerBuilder>)           → self   // call once per player; minimum one (A15)
  .WithDefaultInitManifest(Action<ManifestBuilder>)        → self
  .Build() → GameDefinition    // validates; throws DefinitionException on failure
```

**`Kw` — static factory class for `KeywordNode` trees.**

Writing `new Invocation("take-damage", new ParameterRef("target"), new Literal(3))` is unreadable. `Kw` provides named factory methods:

```
static class Kw
  Param(name: string)                                → ParameterRef
  Literal(value: object)                             → Literal
  Invoke(keyword: string, params KeywordNode[] args) → Invocation
  // Built-in shorthands — one per built-in keyword, e.g.:
  And(a, b)  Or(a, b)  Not(p)
  Add(a, b)  Subtract(a, b)  Multiply(a, b)  Max(a, b)  Min(a, b)
  LessThan(a, b)  GreaterThan(a, b)  AtLeast(a, b)  AtMost(a, b)  EqualTo(a, b)
  GetState(atom, field)   GetProperty(atom, field)   InZone(atom, zone)
  OwnerOf(atom)   // argument must resolve to Card or Zone; enforced at Build() — see D2 addendum
  MoveCard(card, destination)   // card: Card, destination: Zone; moves card to zone — see D12
  Session()         // Literal shorthand that resolves to the session reserved reference
  // ... one shorthand per built-in keyword; the pattern is mechanical
```

`Kw` is the only API surface game creators touch when constructing `KeywordNode` expressions in C#. It is a thin facade with no engine logic.

**Path 2 — JSON loading (tooling output):**

```
GameDefinition.FromJson(Stream json) → GameDefinition   // throws DefinitionException on failure
GameDefinition.FromJson(string json) → GameDefinition
```

The JSON schema is the serialisation contract from D2. The deserialiser runs the same validation as the builder. Both paths produce a `GameDefinition` that is identical at runtime.

---

**`GameSession` and `GameSessionBuilder`.**

```
GameSession.Create(GameDefinition) → GameSessionBuilder

GameSessionBuilder
  .WithPlayerStrategy(string playerName, IPlayerStrategy) → self   // call once per player; all players required
  .WithRandomSource(IRandomSource)       → self   // required
  .WithObserver(IEngineObserver)         → self   // optional
  .UseDefaultInit()                      → self   // adopt GameDefinition.DefaultInitManifest
  .WithInitManifest(InitManifest)        → self   // custom manifest (meta-game, computed setup)
  .WithInitManifest(Action<ManifestBuilder>) → self  // fluent overload
  .FromSavedState(GameStateSnapshot)     → self   // D17 — deferred; reserved in the API
  .Build() → GameSession                 // throws if required fields missing
```

`.UseDefaultInit()`, `.WithInitManifest(...)`, and `.FromSavedState(...)` are mutually exclusive; the last call wins. If none is called the session begins with no atoms — valid for games that build state entirely through phase init blocks.

```
GameSession
  async Task<GameResult> RunAsync()

GameResult {
  Winner   : string?                   // null = draw; non-null = winning player name (A15)
  FinalLog : IReadOnlyList<GameEvent>
}
```

`RunAsync` provisions the init manifest (if any), then runs the phase sequence — phase init, action window, trigger and SBR resolution, phase cleanup — repeating until `GameState.GameIsOver` becomes true (set by `declare-winner` or `declare-draw`). After every `ResolveAction` call, `RunAsync` checks `GameIsOver` and exits the turn loop immediately if true. See D14 addendum (game outcome primitives) for the full propagation contract.

---

**`IPlayerStrategy` — the unified per-player interface.**

Every interaction a player can have with a running session flows through this interface. The engine calls it; the host implements it.

```
interface IPlayerStrategy

  // Called when the engine opens an action window for this player.
  // Return null to pass (close the action window; proceed to phase cleanup).
  Task<PlayerAction?> SelectActionAsync(
      AvailableActions  available,
      GameStateView     state)

  // Called for all engine-initiated prompts:
  //   — mid-effect target/value prompts (D3)
  //   — trigger ordering prompts (D8 PromptPlayer mode)
  //   — any future prompt variant added to PromptContext
  Task<PromptResponse> RespondToPromptAsync(
      PromptContext  context,
      GameStateView  state)
```

`GameStateView` is a read-only wrapper around `GameState` — a thin projection, not a deep copy. Both methods receive it so every implementation (human UI, AI, test harness) has a uniform access pattern.

`IPlayerStrategy` subsumes `IPromptChannel` from D3. Every call site that previously called `IPromptChannel.RequestAsync(ctx)` becomes `strategy.RespondToPromptAsync(ctx, snapshot)`. The suspension-and-resume mechanics (D3) are unchanged.

**`AvailableActions`** is computed by the engine before calling `SelectActionAsync`. It enumerates every legal action at that moment.

```
AvailableActions {
  PlayableCards        : IReadOnlyList<PlayableCardOption>
  ActivatableAbilities : IReadOnlyList<ActivatableAbilityOption>
  CanPass              : bool
}

PlayableCardOption {
  Card         : AtomId
  ValidTargets : IReadOnlyList<TargetSet>   // pre-validated combinations
}

ActivatableAbilityOption {
  Source       : AtomId
  EffectName   : string
  ValidTargets : IReadOnlyList<TargetSet>
}
```

**`PlayerAction`** is the discriminated union the strategy returns:

```
PlayerAction:
  | PlayCard {
      Card           : AtomId
      Targets        : IReadOnlyList<AtomId>
      CostChoices    : IReadOnlyDictionary<string, object>
      VariableValues : IReadOnlyDictionary<string, object>
    }
  | ActivateAbility {
      Source         : AtomId
      EffectName     : string
      Targets        : IReadOnlyList<AtomId>
      CostChoices    : IReadOnlyDictionary<string, object>
      VariableValues : IReadOnlyDictionary<string, object>
    }
  | Pass
```

---

**What the API deliberately does not cover:**
- AI strategy implementation — the engine defines `IPlayerStrategy`; game creators supply their own.
- Rendering — the host reads `GameStateView` independently; no render calls inside `GameSession`.
- Deck building, drafting, card pool management — pre-session concerns for the meta-game layer. The host provides the outcome as an `InitManifest`.
- Save/load (`GameStateSnapshot`) — reserved in the API surface; specified in D17.
- Multiplayer networking — the engine is single-process; `IPlayerStrategy` composition over the network is the host's responsibility.

---

---

**D14 Addendum — A15: Session atom and player registry generalization.**

A15 introduces two changes that affect this decision.

---

**1. Player registry (named dictionary replaces fixed two-slot pair).**

`GameDefinition.Player1Definition` / `Player2Definition` are replaced by a named registry. `PlayerSlot` enum is retired.

```
GameDefinition {
  ...
  PlayerDefinitions      : IReadOnlyDictionary<string, PlayerDefinition>
  ...
}
```

`Build()` enforces the minimum-one constraint: a `GameDefinition` with zero `PlayerDefinitions` entries is a `DefinitionException`.

`InitManifest` references players by name rather than `PlayerSlot`:

```
ZoneSpec {
  LocalId      : string
  Owner        : string         // player name key in GameDefinition.PlayerDefinitions
  Definition   : string
  Accumulators : IReadOnlyDictionary<string, double>?
  Conditions   : IReadOnlyList<string>?
}

CardSpec {
  Owner        : string         // player name
  ZoneLocalId  : string
  Definition   : string
  Accumulators : IReadOnlyDictionary<string, double>?
  Conditions   : IReadOnlyList<string>?
}

PlayerStateSpec {
  Player       : string         // player name
  Accumulators : IReadOnlyDictionary<string, double>?
  Conditions   : IReadOnlyList<string>?
}
```

`PlayerSlot { Player1, Player2 }` is removed from `Core`. All sites that referenced it use `string` player names.

`GameResult` no longer hard-codes two outcomes:

```
GameResult {
  Winner   : string?                   // null = draw; non-null = winning player name
  FinalLog : IReadOnlyList<GameEvent>
}
```

`ActionResolver` and `GameSessionBuilder` accept a strategy per named player:

```
ActionResolver(
  GameDefinition,
  IReadOnlyDictionary<string, IPlayerStrategy> playerStrategies,
  IRandomSource,
  IEngineObserver?)
```

`GameSessionBuilder` fluent API:

```
.WithPlayerStrategy(string playerName, IPlayerStrategy) → self
```

May be called once per player name registered in `GameDefinition`. Calling it for an unknown player name is an error at `Build()`. Calling it for fewer players than defined is also an error — every player must have a strategy.

**Consequences for `GameDefinitionBuilder`:**

```
GameDefinitionBuilder
  .AddPlayer(string name, Action<PlayerBuilder>) → self
  // replaces .WithPlayer1(...) / .WithPlayer2(...)
```

`DefaultInitManifest` player references use the player names established by `AddPlayer` calls.

---

**2. Session atom.**

The engine creates a singleton session atom before the first phase begins. No game creator API is needed to provision it — it is engine-managed.

**Engine behaviour:**

- At game start (before manifest provisioning), the engine allocates a fresh `AtomId` for the session atom and adds it to `GameState`. A reserved field `GameState.SessionAtomId : AtomId` holds this reference.
- The engine initialises two engine-managed accumulator fields on the session atom:
  - `turn-number` — set to `1` at game start; incremented by the engine at the start of each turn.
  - `phase-index` — set to `0` at game start; reset to `0` at each new turn; set to the current phase's 0-based ordinal at the start of each phase.
- The `session` reserved reference (§4.3 of domain model) resolves to `GameState.SessionAtomId` at execution time. `ParameterRef("session")` or the corresponding `Kw.Session()` literal resolves to this ID.
- The session atom is otherwise a normal atom: it has accumulators, modifiers, and conditions, all contribution-tracked. Game creators may extend it via the standard type declaration model.
- **Write protection.** The engine validates at `Build()` (and again at load time for JSON) that no game-creator keyword writes to `turn-number` or `phase-index` via `modify-accumulator`, `apply-modifier`, or `apply-condition` targeting the `session` atom with those reserved field names. This is a `DefinitionException` if detected statically, or a runtime `EngineException` for cases that can only be caught dynamically (e.g. if the field name is computed — though the type system is designed to make all field names statically resolvable).

**No owner.** The session atom has no owner. `owner-of` may not be called on it (enforced by `AtomKindRestriction` as described in the D2 addendum).

**`GameStateBuilder` gains `WithSession`** (for testing contexts that need specific session state):

```
.WithSession(out AtomId id,
             Dictionary<string, double>? accumulators = null,
             IReadOnlyList<string>? conditions = null) → self
```

The builder initialises `turn-number = 1` and `phase-index = 0` by default if `accumulators` does not override them. Most tests will not call `WithSession` — the session atom is provisioned automatically by `GameStateBuilder.Build()` if it hasn't been declared explicitly.

---

**3. `owner-of` in the `Kw` factory.**

```
static class Kw
  ...
  OwnerOf(atom: KeywordNode) → Invocation   // shorthand for Invoke("owner-of", atom)
```

The type-checker in `Build` enforces the `AtomKindRestriction { Card, Zone }` when validating the argument (see D2 addendum). `Kw.OwnerOf` itself is a thin factory — it does not validate at call time; validation occurs at `Build()`.

---

**Domain model gaps flagged by this decision:** None new.

**Rationale:**
- `InitManifest` as a desired-state declaration rather than an imperative init block means the host never has to know the engine's creation primitives. The meta-game layer constructs a data structure; the engine provisions it. Procedural generation happens outside the engine (the host randomises, then hands determined values to the manifest builder).
- Game-start operations that must run inside the engine (shuffling, dealing) belong in the first phase's init block — part of `GameDefinition` authored at design time, not session setup. This cleanly separates "what atoms exist" (manifest, host-owned) from "what happens at game start" (phase init, game-creator-owned).
- `DefaultInitManifest` on `GameDefinition` lets a simple, fixed-start game ship its starting configuration alongside its rules without requiring the host to supply anything beyond strategies and a random source.
- `IPlayerStrategy` as a single unified interface with `GameStateView` on every method ensures the AI always has full state access and the human implementation is never surprised by what information is available. Adding a new interaction type in the future requires adding one method here, not a new interface.

**Consequences:**
- `ActionResolver` constructor: `ActionResolver(GameDefinition, IReadOnlyDictionary<string, IPlayerStrategy> playerStrategies, IRandomSource, IEngineObserver?)`. `IPromptChannel` from D3 is retired as a standalone type. The per-name strategy lookup replaces the former `player1`/`player2` fixed arguments (A15).
- `AvailableActions` computation is the most complex single method in the engine: it must evaluate activation conditions, run cost dry-runs, and enumerate valid target sets for every candidate action, all against the current `GameState`. The implementer should plan for this explicitly.
- `LocalId` in `ZoneSpec` exists only during manifest processing. After provisioning, zones are referenced by their engine-assigned `AtomId`. The manifest builder is responsible for `LocalId` uniqueness within a manifest; the engine validates this at provisioning time.
- `GameDefinition.Keywords` includes built-ins pre-registered by the engine. The `Kw` shorthands must be kept in sync with the built-in registry; both are the implementer's responsibility to maintain together.
- `DefinitionException` is the error type for authoring failures (unknown keyword, cyclic definition, type mismatch, missing required field). It is a design-time error, not a runtime game error.
- `FromSavedState` is reserved in `GameSessionBuilder`'s public API. Until D17 is implemented, calling it throws `NotSupportedException`. Reserving it now prevents the API from diverging in a way that would break callers when D17 lands.

---

---

**D14 Addendum — Game Outcome Primitives.**

D14 originally described the termination condition of `RunAsync` as "repeat until a state-based rule produces an outcome" without specifying the signalling mechanism. The implementation resolved this gap with a terminal-flag pattern using three new built-in primitives. This addendum ratifies those decisions.

---

**Three new built-in primitives:**

| Keyword | Parameters | Returns | Description |
|---|---|---|---|
| `declare-winner` | `player: Player` | `void` | Terminates the game. Sets `GameState.GameIsOver = true` and `GameState.PendingWinner` to the name of `player`. First-call-wins (see below). |
| `declare-draw` | *(none)* | `void` | Terminates the game with no winner. Sets `GameState.GameIsOver = true` and `GameState.PendingWinner = null`. First-call-wins. |
| `player-by-name` | `name: PropertyName` | `Player` | Resolves a player atom at runtime from a name string registered during session provisioning. Returns the `AtomId` of the matching player atom. Throws `EngineException` if the name is not registered. |

`declare-winner` and `declare-draw` are mutation keywords (they mutate `GameState`). `player-by-name` is a property keyword (pure read, no event log entry). All three are registered in `BuiltInKeywords.All`.

**`Kw` shorthands:**
```
Kw.DeclareWinner(player: KeywordNode) → Invocation
Kw.DeclareDraw()                      → Invocation
Kw.PlayerByName(name: KeywordNode)    → Invocation
```

---

**`GameState` fields:**

```
GameState {
  ...
  GameIsOver     : bool       // false initially; set to true by the first DeclareOutcome call
  PendingWinner  : string?    // null = draw; non-null = winning player name
}
```

`DeclareOutcome(winner: string?)` is the internal method called by both primitives:
```
void DeclareOutcome(string? winner):
  if GameIsOver: return          // first-call-wins: all subsequent calls are no-ops
  GameIsOver    = true
  PendingWinner = winner
```

**First-call-wins invariant.** During a trigger cascade, multiple triggers may fire `declare-winner` before any `GameIsOver` check runs. The first call sets the outcome; all subsequent calls are silently ignored. This gives the highest-priority rule (first to fire by `StaticEffectId` ordering) authority over the outcome. Both `declare-winner` and `declare-draw` calls are logged to the event log regardless of whether they take effect, so post-hoc analysis can detect conflicts.

---

**`GameIsOver` propagation contract.** `GameIsOver` is checked at three points:

1. **`RunAsync` turn loop** — after every `ResolveAction` call returns, `RunAsync` checks `GameIsOver` and exits immediately if true. `GameResult` is populated from `PendingWinner`.
2. **Cascade loop in `ActionResolver`** — step 3 of the post-action sequence breaks the cascade loop before firing a new trigger batch when `GameIsOver` is true.
3. **`RunStateBasedRules` fixpoint loop** — at the top of each iteration, exits immediately if `GameIsOver` is true. This prevents an always-true terminal SBR from looping infinitely after it fires `declare-winner`.

---

**`player-by-name` as the authoring-time → runtime player reference bridge.** A card's `KeywordNode` tree is authored statically before atom IDs are assigned at runtime. There is no way to embed a concrete player atom ID in a keyword tree that says "player Alice wins." `player-by-name` bridges authoring-time names and runtime atoms, analogous to how zone definition names work in `move-card` targets. The canonical pattern for a state-based rule that declares Alice the winner is:

```
declare-winner(player-by-name("alice"))
```

`GameSessionBuilder.Build()` validates that every `PlayerDefinition` in `GameDefinition.PlayerDefinitions` has a matching player atom provisioned. An unregistered name at runtime throws `EngineException` — this indicates a game-definition bug detectable at build time.

---

### D17 — Save/Load (`GameStateSnapshot`)

**Decision:** Save points are at turn boundaries only — specifically, at the start of each new turn, after all previous-turn cleanup, trigger resolution, and lifetime checks have completed. `GameStateSnapshot` captures this clean inter-turn state. Three implementation concerns beyond routine field serialization are addressed below.

---

**Save point semantics.**

A save point is the moment after the engine has completed all end-of-turn processing for turn N — SBRs settled, all triggers resolved, all lifetime checks run, all turn-timer and trigger-count expirations processed — but before any phase init block for turn N+1 has executed. At this point:

- No block is executing. No `async` continuation is in-flight.
- `GameState` is fully settled: no partial mutations, no pending triggers, no active scope accumulators.
- `TriggerHighWaterMark` on every active static effect has advanced past every event in turn N.
- The turn accumulator has been merged into `FinalizedLog`.

This eliminates the need to capture any execution call stack, block bindings, prompt state, or scope accumulators. The snapshot is purely static state plus the finalized event log.

**Save API.** `IEngineObserver` gains one method:

```
interface IEngineObserver
  Task<CascadeDirective> OnTriggerCascade(int iterationCount)   // existing
  Task OnTurnStart(int turnNumber, GameStateSnapshot snapshot)   // new: called before turn N+1's first phase init
```

The engine constructs the snapshot and passes it to the observer at the start of each turn (including turn 1, before any play has occurred). The host persists or discards it. No separate `CreateSnapshot()` method on `GameSession` is needed.

**Trade-off.** Progress within a turn is not saved. If the game is interrupted mid-turn, the player resumes from the start of that turn. This is acceptable for card games where turns are short and deterministic given their prompt responses. Games requiring intra-turn saves are out of scope.

---

**Complication 1 — `object`-typed event args have no stable serialized form.**

`GameEvent.BoundArgs` is `Dictionary<string, object>`. The value types that can appear are: `double`, `bool`, `AtomId` (long), `ContributionId` (long), `string`, `EventRef`, and `Collection<T>`. JSON's default `object` deserialization loses type information.

**Resolution: `BoundValue` discriminated union, snapshot-layer only.**

```
BoundValue (discriminated union — used in snapshot ser/de only):
  | NumberValue    { Value : double }
  | BoolValue      { Value : bool }
  | StringValue    { Value : string }
  | AtomIdValue  { Id    : long }
  | ContribIdValue { Id    : long }
  | EventRefValue  { SequenceNumber : long }   // references finalized event by sequence number
  | CollectionValue { Items : IReadOnlyList<BoundValue> }
```

`BoundValue` is used **only during snapshot serialization/deserialization** — `GameEvent.BoundArgs` remains `Dictionary<string, object>` at runtime. The serializer converts `object → BoundValue` when writing and `BoundValue → object` when reading. `System.Text.Json` polymorphic serialization via `[JsonDerivedType]` handles the discriminated union; types live in `Core`.

Live execution bindings (`ExecutionContext.Bindings`) are not in the snapshot at all — turn-boundary saves guarantee no block is executing.

---

**Complication 2 — `System.Random` state is not reproducible across .NET versions.**

`System.Random`'s internal algorithm changed in .NET 6 and is not guaranteed stable across future versions. A snapshot serializing seed + call count could replay differently after a .NET update.

**Resolution: `SeededRandom` uses an engine-owned RNG.**

`SeededRandom` is reimplemented using a simple, engine-owned deterministic algorithm (the implementer documents the chosen algorithm — xoshiro128** or PCG32 are suitable — for save-file forward compatibility). The implementation is not tied to .NET's RNG. The `IRandomSource` interface is unchanged.

```
RngSnapshot {
  Seed      : long
  CallCount : long
}
```

At load time: construct a fresh engine RNG from `Seed` and advance it `CallCount` steps. For card-game lengths (hundreds of calls), this fast-forward is negligible.

`FromSavedState` reads the seed from the snapshot directly — the host does not supply a separate `WithRandomSource` call when loading. The `GameSessionBuilder` constructs `SeededRandom(snapshot.Rng.Seed)` and fast-forwards internally.

---

**Complication 3 — Dynamic static effects have no definition reference.**

Declarative static effects (Origin = Declarative) are identified by `(CardDefinitionName, EffectIndex)` and resolved from `GameDefinition` at load time. Dynamic static effects (Origin = Dynamic, created by `apply-modifier`/`apply-condition` with an inline lifetime) have no backing definition.

**Resolution: `StaticEffectSnapshot` carries either a reference or an inline definition.**

```
StaticEffectSnapshot {
  Id                   : StaticEffectId
  Origin               : Declarative | Dynamic
  OwnerAtomId        : AtomId
  LifetimeSpec         : LifetimeSpec        // serializable; already a KeywordNode-based structure
  TriggerFireCount     : int
  TriggerHighWaterMark : long
  OwnedContributions   : IReadOnlyList<ContributionId>

  // Exactly one of the following is non-null:
  DeclarativeRef       : StaticEffectDefRef?   // for Declarative — (CardDefinitionName, EffectIndex)
  DynamicTrigger       : TriggerDefinition?    // for Dynamic with a trigger; null for Dynamic without one
}

StaticEffectDefRef {
  CardDefinitionName : string
  EffectIndex        : int
}
```

For declarative effects the engine resolves the full `StaticEffectDef` from `GameDefinition` at load time using `DeclarativeRef`. For dynamic effects, `DeclarativeRef` is null; the lifetime is always inlined and any trigger is inlined via `DynamicTrigger`. The contributions owned by a dynamic effect are already in the contribution registry.

---

**Full `GameStateSnapshot` structure.**

```
GameStateSnapshot {
  // Format metadata
  Version          : int = 1
  GameDefinitionId : string      // must match the GameDefinition being loaded into

  // Allocation counters (restored to prevent ID collisions on resume)
  NextAtomId       : long
  NextContributionId : long
  NextStaticEffectId : long
  NextScopeId        : long

  // Atom state
  Atoms           : IReadOnlyList<AtomSnapshot>
  SessionAtomId    : AtomId

  // Contribution registry
  Contributions      : IReadOnlyList<ContributionSnapshot>

  // Static effects
  ActiveStaticEffects : IReadOnlyList<StaticEffectSnapshot>
  DormantEffects      : IReadOnlyList<DormantEffectSnapshot>

  // Event log (all turns completed; no accumulators)
  FinalizedLog        : IReadOnlyList<GameEvent>

  // RNG
  Rng                 : RngSnapshot
}

AtomSnapshot {
  Id           : AtomId
  Kind         : AtomKind       // Card | Zone | Player | Session
  RefName      : string?        // CardDefinition/ZoneDefinition name; player name for Player; null for Session
  OwnerName    : string?        // null for Player and Session
  ZoneId       : AtomId?      // non-null for Card only
  Accumulators : IReadOnlyDictionary<string, double>
  // Modifiers and conditions are fully described by Contributions; reconstructed on load
}

ContributionSnapshot (discriminated union):
  | ModifierContributionSnapshot {
      Id, SourceAtomId?, SourceStaticEffectId?,
      TargetAtom, PropertyName, Kind, Value, Lifetime
    }
  | ConditionContributionSnapshot {
      Id, SourceAtomId?, SourceStaticEffectId?,
      TargetAtom, ConditionName, Lifetime
    }

DormantEffectSnapshot {
  OwnerAtomId      : AtomId
  CardDefinitionName : string
  EffectIndex        : int
}

RngSnapshot {
  Seed      : long
  CallCount : long
}
```

At load time, `ModifierIndex` and `ConditionIndex` on each atom are reconstructed by iterating `Contributions` — they are derived, not stored. `TurnScopeId`, `ActionScopeId`, block accumulators, and the execution call stack are absent because no turn is in progress at a save point.

---

**Public API.**

```
// Save — received via IEngineObserver.OnTurnStart
class MyObserver : IEngineObserver
  async Task OnTurnStart(int turnNumber, GameStateSnapshot snapshot)
    string json = GameStateSnapshotSerializer.Serialize(snapshot)
    await File.WriteAllTextAsync("save.json", json)

// Load
GameStateSnapshot snapshot = GameStateSnapshotSerializer.Deserialize(
    await File.ReadAllTextAsync("save.json"))

GameSession session = GameSession.Create(gameDefinition)
  .WithPlayerStrategy("player1", strategy1)
  .WithPlayerStrategy("player2", strategy2)
  .WithObserver(myObserver)
  .FromSavedState(snapshot)   // seed comes from snapshot; no separate WithRandomSource needed
  .Build()
await session.RunAsync()   // begins at turn N, first phase init block
```

`GameStateSnapshotSerializer` lives in `Archetype.Engine`. It uses `System.Text.Json` with `[JsonDerivedType]` on `BoundValue`, `ContributionSnapshot`, and `StaticEffectSnapshot`.

`GameDefinition` gains `Id: string`. `Build()` rejects a definition without one. The loader rejects a snapshot whose `GameDefinitionId` does not match.

---

**Rationale:**
- Turn-boundary granularity eliminates the hardest serialization problems: no C# continuation capture, no execution call stack, no block idatom (`BlockRef`), no live binding serialization, no scope accumulator bookkeeping. The snapshot is pure settled state.
- `IEngineObserver.OnTurnStart` as the save point notification rather than a `CreateSnapshot()` method on `GameSession`: the engine constructs the snapshot at exactly the right moment, removing any ambiguity about when it is valid to save.
- `BoundValue` only in the snapshot layer keeps the runtime `Dictionary<string, object>` unchanged. The conversion happens once per save/load, not on every step.
- Engine-owned RNG keeps save files reproducible across .NET version upgrades. Seed + call count fast-forward is sufficient at card-game scales.

**Consequences:**
- `IEngineObserver` gains `Task OnTurnStart(int turnNumber, GameStateSnapshot snapshot)`. Existing implementations that don't need save/load provide an empty stub. `IEngineObserver` remains nullable; `null` → no-op for both methods.
- `SeededRandom` is reimplemented without `System.Random`. Algorithm must be documented by the implementer. `IRandomSource` interface is unchanged.
- `GameDefinition` gains `Id: string`. `GameDefinitionBuilder.Build()` and `GameDefinitionLoader.FromJson()` both require it.
- `GameStateSnapshotSerializer` is a new class in `Archetype.Engine`. D15 module boundaries are unchanged.
- `LifetimeSpec` and its `KeywordNode`-based conditions serialize using the existing keyword tree JSON schema — no new format.
- `ExecutionContext` does **not** gain a `BlockCallStack` field. No changes to `ExecuteBlock` for save/load support.

---

### D18 — Keyword Cross-References in Card Text

**Decision:** Card text may contain inline cross-references to keyword definitions. A new `RulesRef` `RenderNode` variant carries the keyword name and display text so the host knows to render the term as a link. `TextRenderer` gains a `Resolve` method the host calls to follow the link and retrieve the linked keyword's rendered definition tree. Cross-references are scoped to keywords only — phases, state-based rules, and action rules are not referenceable from card text.

---

**Motivation.** Without a typed cross-reference node, the host receives a flat `TextSpan` with no signal that a term is linkable. It would have to do brittle string matching to decide what to make interactive. `RulesRef` makes the linkability explicit and structured: the host knows exactly what is a link, and what keyword it resolves to, without any guesswork.

The scope restriction to keywords is deliberate. Every game mechanic a player needs explained (damage, sleep, delirium, trample) is a keyword-defined concept. Phase and rule cross-references are an authoring smell — if a card's behavior depends on a phase, the phase's effect on that card should be expressed in the card's own keyword definitions, not in a prose pointer to a phase description.

---

**`RulesRef` node (addition to `RenderNode` discriminated union):**

```
RenderNode (abstract record)
  ├── TextSpan(text: string)
  ├── CompositeNode(summary: RenderNode, body: RenderNode)
  ├── SequenceNode(items: IReadOnlyList<RenderNode>)
  └── RulesRef(key: string, displayText: string)          // NEW
        key         — keyword name; validated against GameDefinition.Keywords
        displayText — the text the host renders as a link label
```

`RulesRef` is a leaf node — it does not carry a pre-rendered body. The host calls `TextRenderer.Resolve` to obtain the body on demand, so the link is lazily resolved (only when the user actually activates it). This avoids eagerly expanding every cross-referenced keyword in every card's render tree on first render.

---

**`TextTemplate` tag syntax.** (See also D2 addendum.) The renderer's template parser recognizes two tag forms when processing any `TextTemplate` or locale string:

- **Short form:** `[take-damage]` → `RulesRef(key: "take-damage", displayText: "take-damage")`
- **Long form:** `[damage](take-damage)` → `RulesRef(key: "take-damage", displayText: "damage")`

Tags are parsed as part of the template expansion step — after `{paramName}` substitutions — and produce `RulesRef` leaf nodes that are spliced into the `SequenceNode` or `TextSpan` structure at their position. A template like `"Deal {amount} [damage](take-damage) to {target}"` renders as a `SequenceNode` of:
1. `TextSpan("Deal 3 ")` (after `{amount}` substitution)
2. `RulesRef(key: "take-damage", displayText: "damage")`
3. `TextSpan(" to Goblin")` (after `{target}` substitution)

---

**`TextRenderer.Resolve`:**

```
RenderNode? Resolve(string keywordName,
                    IReadOnlyDictionary<string, string>? localeStrings,
                    IReadOnlyDictionary<string, object>?  bindings)
```

- Looks up `keywordName` in the keyword registry. Returns `null` if the name is not found (should not occur for validated game definitions; the host treats `null` as "no details available" and may suppress the link affordance).
- Otherwise returns `Render(definition.Body, localeStrings, bindings)` using the same template resolution order as any other keyword render: locale → `TextTemplate` → structural. For primitives, whose `Body` is a sentinel, the renderer uses the primitive's registered `TextTemplate` and does not attempt structural recursion into the sentinel.
- The host calls this method when the player activates a link (tap, hover, click) — not at initial render time. The result is displayed as a tooltip, modal, sidebar, or however the host chooses.
- `Resolve` is stateless and re-entrant: it may itself produce `RulesRef` nodes if the resolved keyword's template contains further cross-reference tags, allowing multi-level drill-down (e.g. resolving `attack` produces text that itself links to `take-damage`). The host decides whether to support recursive expansion.

---

**Caching.** `RulesRef` nodes are stable: `key` and `displayText` do not depend on runtime bindings. They are always included in the definition-time `RenderNode` cache (D11). Resolved bodies from `Resolve` calls use the same locale-keyed cache as ordinary renders; no separate cache entry is needed.

---

**Validation (summary).** Three enforcement points:

| Point | What is checked | Error type |
|---|---|---|
| `GameDefinitionBuilder.Build()` | Every `[key]` / `[text](key)` in any `TextTemplate` or locale string resolves to a name in `Keywords` | `DefinitionException` |
| `GameDefinitionLoader.FromJson()` | Same check on deserialized templates | `DefinitionException` |
| DSL tooling parse time | Same check; editor highlights unknown keys inline | Authoring error |

---

**Tooling additions (extension of D11 tooling section):**

- The DSL editor provides autocomplete for `[` and `](` positions, suggesting keyword names from `GameDefinition.Keywords`.
- Locale file template strings are subject to the same tag validation as `TextTemplate` strings.
- The card text preview in the tooling renders `RulesRef` nodes as underlined/styled spans with a "click to expand" affordance, calling `Resolve` on activation to show the linked definition inline.

---

**What this decision does not cover:**

- Cross-references to phases, SBRs, action rules, or glossary entries — deliberately excluded (see Motivation above).
- `RulesRef` in invocation-time renders (with `bindings != null`) — `RulesRef` nodes are stable and appear regardless of render mode. The host may choose to suppress link affordances in event-log display where interactive expansion is inappropriate.

---

**Rationale:**
- `RulesRef` as a typed leaf node (rather than a styled `TextSpan`) gives the host structured information about what is linkable without string matching. The host's simplest implementation: walk the tree, render `RulesRef.displayText` as plain text, ignore the link affordance. More capable hosts render it as interactive.
- Lazy resolution via `Resolve` rather than an eagerly-expanded `body` on `RulesRef` avoids the cost of rendering every transitively-referenced keyword definition at card-display time. Card text for `attack` displays immediately; the `take-damage` definition is only rendered when the player asks for it.
- Keyword-only scope keeps the key namespace flat and unambiguous: every valid key resolves to exactly one `KeywordDefinition`. No disambiguation between "is this a keyword? a phase? an SBR?" is ever needed.
- The same `Resolve` path handles both built-in and game-creator-defined keywords identically. A cross-reference to `modify-accumulator` (built-in) works the same as a reference to `take-damage` (game-creator-defined).

**Consequences:**
- `RulesRef` is a new `RenderNode` variant in `Archetype.Core` (alongside `TextSpan`, `CompositeNode`, `SequenceNode`). The host's `RenderNode` visitor must handle it — existing visitors that don't need link affordances may treat it as a `TextSpan(displayText)`.
- `TextRenderer.Resolve` is a new public method on `TextRenderer` in `Archetype.Text`. It requires no new state on `TextRenderer`; the existing keyword registry and cache are sufficient.
- The template parser in the tooling (`Archetype.Build` or DSL tooling) gains a second parse pass for `[...]` and `[...](...)` tags. The engine's text renderer also gains this parsing step when expanding `TextTemplate` strings at render time.
- The `RulesRef` caching strategy is: include in the definition-time cache (locale-keyed, per D11). Invocation-time renders include `RulesRef` nodes unchanged — their `key` and `displayText` are unaffected by runtime bindings.
- `BuiltInKeywords` metadata in `Core` must include a `TextTemplate` for each primitive so that `Resolve` calls on built-in names produce meaningful output (D11 already requires this; D18 reinforces it).

---

---

### D19 — `ComputeAvailableActions` Contract

**Decision:** `ComputeAvailableActions` filters the active player's playable cards by zone membership (using `GameDefinition.PlayableZoneNames`) and then by per-card activation condition (using `CardDefinition.ActivationCondition`). Ability activation uses only the existing `ActivationCondition` on `NamedEffectBlockDef`; no separate zone restriction mechanism is needed. Cost pre-flight is explicitly deferred. `Pass` is always included. A new built-in property query — `get-atoms-in-zone` — supports zone-based filtering.

---

**`get-atoms-in-zone` built-in property keyword:**

| Keyword | Parameters | Returns | Notes |
|---|---|---|---|
| `get-atoms-in-zone` | `zone: Zone` | `Atom[]` | Pure read; no state mutation; no event log entry. Returns all atom IDs whose current `ZoneId` equals the given zone atom's ID. Throws `EngineException` if the argument is not a Zone atom. An empty zone returns an empty collection. |

`get-atoms-in-zone` is a property keyword. It is registered in `BuiltInKeywords.All` alongside the existing read primitives. Its `Kw` shorthand:
```
Kw.GetAtomsInZone(zone: KeywordNode) → Invocation
```

`ComputeAvailableActions` uses the equivalent internal read — iterating `GameState` atoms by `ZoneId` — rather than dispatching the keyword through the full interpreter, since it is pure C# code with direct state access. The built-in exists so game creators can also use zone queries in their own keyword trees (activation conditions, SBRs, trigger conditions).

---

**`CardDefinition.ActivationCondition`** (new field, added to D14):

```
CardDefinition {
  ...
  ActivationCondition : KeywordNode?    // optional; evaluated pure before PlayCard is offered; null = always playable (subject to zone filter)
  ...
}
```

Evaluated using the same pure condition evaluation path as `WhileCondition` and trigger conditions (`EvaluateCondition` / `BlockExecutor.EvaluateCondition`): no state mutation, no event logging. The evaluation context provides read-only `GameState` access. No variable bindings are in scope (the condition may only reference `GameState` reads and literals). `null` means the card is always playable (no per-card condition).

---

**`GameDefinition.PlayableZoneNames`** (new field, added to D14):

```
GameDefinition {
  ...
  PlayableZoneNames : IReadOnlyList<string>?   // zone definition names from which cards may be played; null = no zone filter
  ...
}
```

A list of zone definition names (matching `ZoneDefinition.Name`). `ComputeAvailableActions` filters the active player's owned card atoms to those whose current `ZoneId` resolves to a zone whose definition name appears in this list. `null` or empty list = no zone filter (all owned cards are zone-eligible candidates). Validated at `Build()`: every name in `PlayableZoneNames` must appear in `GameDefinition.ZoneDefinitions`; unknown names are a `DefinitionException`.

---

**`ComputeAvailableActions` algorithm:**

```
AvailableActions ComputeAvailableActions(string activePlayer, GameState state)

  result = new AvailableActions()

  // Step 1: PlayCard candidates
  ownedCards = atoms in state where Kind == Card AND OwnerName == activePlayer

  // Step 2: Zone filter
  if GameDefinition.PlayableZoneNames != null AND PlayableZoneNames.Count > 0:
    playableZoneIds = zones in state
                        where ZoneDefinition.Name ∈ PlayableZoneNames
                        AND OwnerName == activePlayer   // restrict to the active player's own zones
    ownedCards = ownedCards where ZoneId ∈ playableZoneIds

  // Step 3: Activation condition filter
  for each card in ownedCards:
    cardDef = GameDefinition.CardDefinitions[card.RefName]
    if cardDef.ActivationCondition == null
       OR EvaluateCondition(cardDef.ActivationCondition, state):
      result.PlayableCards.Add(PlayableCardOption { Card: card.Id })

  // Step 4: Ability candidates
  for each card in (all active player's cards, regardless of zone):
    cardDef = GameDefinition.CardDefinitions[card.RefName]
    for each ability in cardDef.AdditionalEffects:
      if ability.ActivationCondition == null
         OR EvaluateCondition(ability.ActivationCondition, state):
        result.ActivatableAbilities.Add(ActivatableAbilityOption {
          Source: card.Id, EffectName: ability.Name })

  // Step 5: Pass is always available
  result.CanPass = true

  return result
```

**Notes on ability zone filtering (step 4).** There is no separate zone restriction mechanism for abilities. If a game requires "abilities can only be activated from the battlefield," the game creator expresses this via `ActivationCondition` on the relevant `NamedEffectBlockDef` (e.g., `in-zone(source, battlefield-zone-id)`). The condition evaluation context for abilities provides `source` as the owning card atom (consistent with the `source` reserved binding in static effect evaluation — D13). The engine does not need a global ability-zone designation because ability restrictions vary too much per card and per ability to be usefully captured as a game-level policy.

**Cost pre-flight is deferred.** `ValidTargets` enumeration and cost dry-runs are not part of the minimum viable `ComputeAvailableActions`. When a game definition requires cost filtering, it will be specified as a separate architectural amendment. Until then, `PlayableCardOption.ValidTargets` and `ActivatableAbilityOption.ValidTargets` are empty lists — `IPlayerStrategy` implementations that rely on them must not assume they are populated.

---

**`GameDefinitionBuilder` additions:**

```
GameDefinitionBuilder
  .WithPlayableZones(params string[] zoneDefinitionNames) → self
```

`CardBuilder` gains:
```
CardBuilder
  .WithActivationCondition(KeywordNode condition) → self
```

---

**Rationale:**
- `PlayableZoneNames` as a game-level list (rather than per-player or per-card) handles the common case (all players play from "hand") in one setting and requires no special convention on `ZoneDefinition`. Games with asymmetric playable zones (different players, different zone definitions) use `ActivationCondition` on individual cards to add the extra restriction.
- `ActivationCondition` on `CardDefinition` follows the established pattern from `NamedEffectBlockDef` — same field name, same evaluation semantics. No new evaluation path is needed.
- `get-atoms-in-zone` as a named built-in keeps zone queries composable within keyword trees. `ComputeAvailableActions` uses the equivalent internal state read directly for performance, but the primitive's existence means game creators can write activation conditions and SBRs that query zones without a bespoke C# API.
- Reusing `ActivationCondition` on `NamedEffectBlockDef` for ability zone restrictions avoids a second zone-designation mechanism at the cost of slightly more verbose authoring for the common "battlefield only" case. This trade-off is acceptable given that ability activation restrictions are typically more varied than card-play restrictions.

**Consequences:**
- `CardDefinition` gains `ActivationCondition: KeywordNode?`. The `CardBuilder.WithActivationCondition` method and JSON deserialization both respect this field. Existing `CardDefinition` instances without the field are treated as `null` (always playable).
- `GameDefinition` gains `PlayableZoneNames: IReadOnlyList<string>?`. `GameDefinitionBuilder.Build()` validates all names against `ZoneDefinitions`. `null` is a valid value (no zone filter); the builder default is `null`.
- `BuiltInKeywords.All` gains `get-atoms-in-zone`. The startup assertion in `ActionResolver` (every built-in name has a registered handler) will catch any mismatch.
- `Kw` gains `GetAtomsInZone`. Kept in sync with `BuiltInKeywords` per the D15 consequence.
- `ComputeAvailableActions` is no longer a placeholder. The implementer must rewrite it to match the algorithm above. Existing tests that relied on the placeholder behaviour (all owned cards always available) should be updated to reflect zone and condition filtering.
- The `source` reserved name must be populated in the activation-condition evaluation context for `NamedEffectBlockDef` conditions. For `CardDefinition.ActivationCondition`, `source` is the card atom itself.

---

### D20 — `CostDef` type and extended `assert` built-in

**Decision:** `CostDef` is a first-class record with no separate evaluation function — affordability is expressed entirely through the `assert` built-in inside the cost body. The `assert` built-in signature is:

```
assert(condition: Boolean, on_fail: OnFail = continue, notify: NotifyFlag = on) → Void
```

`OnFail` and `NotifyFlag` are inline-literal-only enum types — game creators cannot declare keyword parameters of these types, but they can pass the literals `continue`, `stop`, or `panic` (for `OnFail`) and `on` or `off` (for `NotifyFlag`) at call sites.

**`assert` semantics:**

| Context | `on_fail` | `notify` | Effect on failure |
|---|---|---|---|
| Inside any cost body | hardwired `panic` | hardwired `off` | Raises `EngineException`; `OnDiagnostic` is NOT called |
| Other effects (default) | `continue` | `on` | Continues execution; calls `OnDiagnostic` |
| Other effects (explicit) | any | any | Per literal values at call site |

`on_fail` and `notify` are orthogonal:
- `notify: on` — calls `IEngineObserver.OnDiagnostic(DiagnosticEvent)` when the condition fails; this happens BEFORE raising `EngineException` when `on_fail: panic`
- `on_fail: stop` — halts the block gracefully (no exception, no further steps); distinct from `panic` (which raises `EngineException`) and `continue` (which does nothing and proceeds)
- `assert` NEVER appends to the event log under any outcome — assertion failure is not a state change

The hardwiring of cost-body assert semantics is enforced by the `BlockExecutor`: when executing a block that was opened as a cost body (a flag on `ExecutionContext`), any `assert` call within that scope ignores `on_fail` and `notify` arguments and behaves as `panic`/`off`. This avoids the game creator needing to remember the cost-body convention and prevents cost bodies from silently continuing past an unaffordable state.

**`CostDef` record:**
```
CostDef {
  Body         : EffectBlockDef    // mutating — pays the cost; assert() signals un-affordability
  Parameters   : ParameterDecl[]  // player-provided args (e.g. which card to discard)
  TextTemplate : string?           // localized cost description; {paramName} placeholders
}
```

`CostDef` cannot itself carry a `CostDef` (no recursive costs). `Body` is a normal `EffectBlockDef`.

**`CardDefinition` updated:**
```
CardDefinition {
  ...
  Cost                : IReadOnlyList<CostDef>   // empty = no cost
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

**Rationale:** A separate `EvaluationFunction` per cost introduces a novel interleaving of pure checks and mutations between cost body executions. `CheckLifetimes` cannot run between steps, creating a subtle inconsistency between validation and real execution. The `assert`-in-body approach uses the same single-block execution path for both validation and real execution. The extended `on_fail`/`notify` parameters make `assert` a general diagnostic tool without adding a new keyword — the cost-body restriction is a safety rail, not a semantic distinction.

**Consequences:**
- `BuiltInKeywords` gains `assert` with three parameters; the keyword descriptor carries `IsCostBodyOnly: false` (the hardwiring is enforced by execution context, not by registration).
- `Kw` gains `Assert(condition: KeywordNode, onFail: OnFail = OnFail.Continue, notify: NotifyFlag = NotifyFlag.On) → Invocation`.
- `OnFail` and `NotifyFlag` are C# enums in `Archetype.Core`; they are NOT `KeywordNode` parameter types (no `ParameterType.OnFail` or `ParameterType.NotifyFlag`).
- `ExecutionContext` gains a `bool IsCostBody` flag; `BlockExecutor` sets this true before executing any `CostDef.Body`.
- `NamedEffectBlockDef.Cost` changes from `EffectBlockDef?` to `IReadOnlyList<CostDef>` — this is a **breaking change** (see D25).
- `CardDefinition.Cost` is new; existing definitions without it default to empty list.

---

### D21 — Combined cost block validation via state clone

**Decision:** When `ValidateActionArgs` is invoked, all `CostDef.Body` blocks for the action are concatenated in declaration order into a single composite `EffectBlockDef`, then executed against a lightweight clone of `GameState` with `IsCostBody = true`. If the combined block completes without throwing `EngineException`, all costs are affordable and `ValidationResult.IsValid` is true. If `EngineException` is thrown (by an `assert` in a cost body or any other runtime failure), `IsValid` is false.

**Lightweight clone scope:**

| Included | Excluded |
|---|---|
| Atom table (mutable copy) | `EventLog` |
| Accumulator maps (mutable copy) | Active static effects |
| Zone membership | Contribution registries |
| Condition presence | Observer reference |

The clone is shallow-copy-safe because cost bodies that use `assert` only read accumulator state, zone membership, and condition presence — none of which depend on event log or contribution data.

**`ValidationResult`:**
```
ValidationResult {
  IsValid   : bool
  CostTexts : IReadOnlyList<string>   // one per CostDef, always populated; resolved from TextTemplate
}
```

`CostTexts` is always resolved (from `CostDef.TextTemplate` + locale parameters from `PlayerAction.CostChoices`) regardless of validation outcome, for player-facing display. The combined-block approach does not yield a per-cost failure index; `IsValid` is the sole pass/fail signal.

**Rationale:** Single-block execution against a clone is the minimum path consistent with real execution semantics. Sequential evaluation-function + body per cost requires a novel interleaving not present anywhere else in the engine. Full-clone including event log and effects is accurate but expensive and unnecessary for the cost affordability check.

**Consequences:**
- `GameState` gains an internal `CloneForValidation()` method; its result is an independent copy with no shared mutable references to the original.
- `CostValidator` (new class in `Archetype.Engine`) implements `Validate(IReadOnlyList<CostDef> costs, PlayerAction action, GameState state, GameDefinition def) → ValidationResult`.
- If a cost body calls a keyword that reads the event log (e.g. a trigger condition inside a cost), the clone's empty log will produce a different result than real execution. This is a known acceptable limitation; game creators must not write cost bodies that depend on event log state.

---

### D22 — `ValidateActionArgs` callback placement

**Decision:** `ValidateActionArgs` is a `Func<PlayerAction, ValidationResult>` field on `AvailableActions`. The host may call it as many times as needed before returning a `PlayerAction` from `IPlayerStrategy.SelectActionAsync`. It is synchronous (cost bodies contain no prompts and no async keywords).

```
AvailableActions {
  PlayableCards        : IReadOnlyList<PlayableCardOption>
  ActivatableAbilities : IReadOnlyList<ActivatableAbilityOption>
  CanPass              : bool
  ValidateActionArgs   : Func<PlayerAction, ValidationResult>   // NEW
}
```

The delegate is constructed by the engine at `ComputeAvailableActions` time. It captures a snapshot of `GameState` (the clone-for-validation) and `GameDefinition`. The host does not need a direct reference to `GameSession`.

**Rationale:** A delegate on `AvailableActions` keeps the strategy interface self-contained — the host receives everything it needs to reason about actions in one object. A public method on `GameSession` would require strategies to hold a session reference, increasing coupling. A static helper would have no access to current state.

**Consequences:**
- `AvailableActions` gains `ValidateActionArgs` — this is a **breaking change** for any code constructing `AvailableActions` directly (see D25).
- The delegate is not nullable; the engine always supplies a working implementation. If the action has no costs, the delegate returns `ValidationResult { IsValid = true, CostTexts = [] }` immediately.

---

### D23 — Cost execution sequencing at action time

**Decision:** When `ActionResolver` executes a `PlayCard` or `ActivateAbility` action, cost bodies run before the primary effect in declaration order. Each `CostDef.Body` runs as its own `EffectBlockDef` within the same action scope as the primary effect (not a separate action scope). Cost events therefore appear in `events.this_action`. The `BlockExecutor` sets `IsCostBody = true` for each cost body execution, enforcing the hardwired `panic`/`off` assert semantics.

If any cost body raises `EngineException`, the action fails and the exception propagates. It is the host's responsibility to call `ValidateActionArgs` before submitting an action; the engine does not perform automatic rollback on cost failure.

**Rationale:** A separate action scope per cost splits cost events from effect events, complicating event-log queries. Rollback on cost failure is expensive and semantically complex; the intended usage is the pre-validated path via `ValidateActionArgs`.

**Consequences:**
- `ActionResolver` (or `GameSession.TranslatePlayerAction`) inserts a loop over `CostDef.Body` blocks before dispatching the primary `EffectBlockDef`. The cost args from `PlayerAction.CostChoices` are bound into the execution context before each cost body runs.
- Event log ordering: cost events will always precede primary effect events within `events.this_action`. Tests that assert on event ordering within an action must account for this.

---

### D24 — `ComputeAvailableActions` ownership filter removal

**Decision:** The hard-coded `zone.OwnerId == activePlayer` predicate is removed from `ComputeAvailableActions`. Zone membership and `ActivationCondition` are the sole filtering mechanisms. The `activePlayer` parameter is retained as the canonical "who is acting" identifier, but it is no longer used as an ownership filter inside the engine.

**Updated algorithm:**
```
ComputeAvailableActions(string activePlayer, GameState state):

  // Step 1: PlayCard candidates — all card atoms (no owner filter)
  candidates = all card atoms in state
  if PlayableZoneNames is non-empty:
    candidates = candidates where zone.DefinitionName ∈ PlayableZoneNames
  for each candidate:
    source = candidate
    if cardDef.ActivationCondition == null
       OR EvaluateCondition(cardDef.ActivationCondition, state, {source}):
      add PlayableCardOption(Card: candidate.Id)

  // Step 2: Abilities — all card atoms, all zones (no owner filter)
  for each card atom in state:
    for each ability in cardDef.AdditionalEffects:
      source = card atom
      if ability.ActivationCondition == null
         OR EvaluateCondition(ability.ActivationCondition, state, {source}):
        add ActivatableAbilityOption(Source: card.Id, EffectName: ability.Name)

  // Step 3: Pass is always available
  result.CanPass = true

  // Step 4: Attach validator
  result.ValidateActionArgs = (action) => CostValidator.Validate(CostsFor(action), action, state, def)

  return result
```

**Migration helper — `Kw.OwnedByActivePlayer()`:**

Added to `Archetype.Build`. Expands to:
```
Kw.Eq(Kw.OwnerOf(Kw.Param("source")), Kw.GetState(Kw.Session(), "active-player"))
```

This helper requires the game to declare a session state field named `"active-player"` (a `string` value holding the current active player's identifier). Games that do not declare this field will receive a `DefinitionException` at `Build()` time. Document this requirement in the `Archetype.Build` XML doc and the game creator guide.

**Rationale:** Ownership is a game-specific concept, not an engine primitive. The D19 ownership filter was an implicit assumption about game structure. `ActivationCondition` is the principled place for game-specific playability constraints. `Kw.OwnedByActivePlayer()` makes migration a one-liner for the common case.

**Consequences:**
- `ComputeAvailableActions` no longer has an implicit ownership requirement — this is a **breaking behaviour change** for existing game definitions that relied on it. Existing tests must be audited; those that assumed only the active player's cards appear in `PlayableCards` must add an explicit `ActivationCondition` or update their assertions (see D25).
- `Kw.OwnedByActivePlayer()` is the only provided shorthand; games with more complex ownership semantics write their own `ActivationCondition` expression.
- `Kw.OwnerOf` and `Kw.GetState(Kw.Session(), ...)` must already exist in `Archetype.Build`; if they do not, add them as part of this change.

---

### D25 — Breaking changes catalogue for action-args-and-cost-model

**Decision:** The following interfaces and types change in ways that require mechanical migration. All changes are introduced together in one branch; no phased rollout.

| Component | Breaking change |
|---|---|
| `IEngineObserver` | New method `void OnDiagnostic(DiagnosticEvent e)` — existing implementations must add the method |
| `AvailableActions` | New field `ValidateActionArgs: Func<PlayerAction, ValidationResult>` — existing struct literals must supply the field |
| `NamedEffectBlockDef` | `Cost: EffectBlockDef?` → `Cost: IReadOnlyList<CostDef>` — all construction and pattern-match sites must update |
| `PlayCard` / `ActivateAbility` action handling | Cost sequencing is now enforced before the primary effect — tests that observed effect events without prior cost events must add cost definitions or update expectations |
| `ComputeAvailableActions` | Ownership filter removed — callers that expected only the active player's cards must add `ActivationCondition: Kw.OwnedByActivePlayer()` |

**`DiagnosticEvent` shape:**
```
DiagnosticEvent {
  Kind          : DiagnosticKind   // enum; AssertionFailed is the first value
  Message       : string
  ConditionNode : KeywordNode?     // the condition AST node that failed; null if not available
  OnFail        : OnFail           // the on_fail value in effect when the diagnostic was generated
  Location      : string           // human-readable, e.g. "energy_cost @ PlayCard"
}
```

`DiagnosticKind` is an extensible enum (int-backed, not a closed set) so future diagnostic kinds can be added without a breaking change to the observer interface.

**`IEngineObserver.OnDiagnostic`:**
- Signature: `void OnDiagnostic(DiagnosticEvent e)`
- Called synchronously by `BlockExecutor` when an `assert` fails with `notify: on`
- Called BEFORE raising `EngineException` when `on_fail: panic`
- A null observer reference means no-op (same guard pattern as existing observer methods)
- Does NOT write to the event log
- Must not throw; any exception propagates out of `BlockExecutor` and is treated as an engine error

**Rationale:** Cataloguing breaking changes in a single decision ensures the implementer audits every affected site before merging. Placing `DiagnosticEvent` and `OnDiagnostic` here (rather than in D20) keeps D20 focused on `assert` semantics and keeps the observer contract in one place.

**Consequences:**
- `DiagnosticEvent` and `DiagnosticKind` are new types in `Archetype.Core`.
- `OnFail` and `NotifyFlag` enums are in `Archetype.Core` (shared between `assert` descriptor and `DiagnosticEvent`).
- All existing `IEngineObserver` implementations (including test fakes) must add `OnDiagnostic`. If the project uses a base class or adapter, add the default no-op there.
- The implementer must search for all `new AvailableActions {` struct literals and add `ValidateActionArgs`.
- The implementer must search for all `Cost:` assignments on `NamedEffectBlockDef` and migrate from `EffectBlockDef?` to `IReadOnlyList<CostDef>`.

---

### D26 — Authoring Tool Platform and Process Architecture

**Decision:** The authoring tool is an Electron desktop application with a TypeScript/React frontend and a co-located .NET sidecar process. The Electron main process spawns the sidecar on startup and communicates with it over stdin/stdout using newline-delimited JSON-RPC. The sidecar owns all C# logic; the Electron renderer process owns all UI.

---

**Process architecture:**

```
┌─────────────────────────────────────────────┐
│  Electron app                               │
│                                             │
│  ┌──────────────┐   IPC    ┌─────────────┐ │
│  │  Renderer    │◄────────►│  Main       │ │
│  │  (React/TS)  │          │  process    │ │
│  └──────────────┘          └──────┬──────┘ │
│                                   │ stdin/stdout │
│                            ┌──────▼──────┐ │
│                            │  .NET       │ │
│                            │  sidecar    │ │
│                            │  (child     │ │
│                            │   process)  │ │
│                            └─────────────┘ │
└─────────────────────────────────────────────┘
```

**Three-layer responsibility split:**

| Layer | Technology | Responsibilities |
|---|---|---|
| Renderer process | TypeScript, React | All UI: DSL editor (Monaco), graph visualisation, form editors, accordion layouts, localization panel, card text preview display |
| Main process | TypeScript (Node.js) | File system (open/save project files, image loading/cropping), sidecar lifecycle, IPC bridge between renderer and sidecar |
| Sidecar | C# / .NET 10 console app | Validation (type-checking, acyclicity, name resolution), text rendering preview, game definition serialisation (export), Godot class generation |

---

**Sidecar (`Archetype.Tooling.Server`).**

A new assembly: a self-contained .NET 10 console application. It references `Archetype.Core`, `Archetype.Build`, and `Archetype.Text` — the same assemblies the Godot host references, minus `Archetype.Engine` (runtime not needed by the tool).

Communication protocol: newline-delimited JSON over stdin/stdout. Each line is a complete JSON object. The main process writes a request line; the sidecar writes a response line. Requests carry an `id` field for correlation; responses echo it. The sidecar is stateful — it holds the current in-memory `GameDefinition` graph and updates it incrementally as the game creator edits.

Example request/response shapes:

```
// Request: validate a keyword body
{ "id": "r1", "method": "ValidateKeyword", "params": { "name": "take-damage", "dsl": "modify-accumulator(target, \"damage\", amount)" } }

// Response
{ "id": "r1", "result": { "diagnostics": [] } }

// Request: render card text preview
{ "id": "r2", "method": "RenderCardText", "params": { "cardName": "goblin" } }

// Response
{ "id": "r2", "result": { "renderTree": { ... } } }
```

The sidecar exposes one method per authoring operation (see D27 for the full validation method surface). It never calls back into Electron unprompted — it is strictly request/response. This simplifies error handling and avoids event-ordering complexity.

---

**DSL editor: Monaco.**

The renderer process embeds Monaco Editor for all DSL text fields (keyword bodies, effect blocks, lifetime specs, activation conditions). Validation results from the sidecar are translated into Monaco `IMarkerData` objects and pushed to the editor via `monaco.editor.setModelMarkers`. Autocomplete is implemented as a Monaco `CompletionItemProvider` that calls the sidecar for context-sensitive suggestions.

Monaco is well-suited here: it handles multi-cursor editing, go-to-definition, hover docs, and inline error markers out of the box once the language extension is registered. The DSL is not a standard language, so the extension is custom — but Monaco's extension API is designed exactly for this.

---

**Keyword composition graph: React Flow (or equivalent).**

The graph visualisation (keyword dependency graph, set overview) is rendered in the renderer process using a React-based graph library (React Flow is the reference choice; alternatives are cytoscape.js or Dagre-D3). The graph data (nodes, edges) is derived in the renderer from the in-memory definition state, not from the sidecar. Graph layout is computed client-side.

---

**Bundling and distribution.**

The sidecar is published as a self-contained .NET single-file executable (one binary per platform, no .NET SDK required on the user's machine). Electron Builder packages it alongside the Electron app under `resources/`. The main process resolves the sidecar path via `process.resourcesPath` at runtime. Platform-specific binaries are placed in platform-specific sub-directories under `resources/` and selected at startup.

The resulting installer size is approximately 150–200 MB (Chromium + Node + sidecar), which is acceptable for a desktop authoring tool used infrequently by game creators, not distributed to end players.

---

**XAML exclusion.** Consistent with the requirements, no XAML-based framework (Avalonia, WPF, MAUI) appears anywhere in the tool stack. The C# sidecar is a headless console process with no UI framework dependency.

---

**Rationale:**
- The tool must call `Archetype.Build` validation logic (type-checker, acyclicity checker, `TextTemplate` tag validator) as the game creator types. These are C# and cannot be rewritten in TypeScript without duplicating the engine's type system. A .NET process is unavoidable; the sidecar model makes this explicit rather than hidden.
- Electron rather than Tauri: Tauri's Rust backend does not help with .NET interop. Both would need a .NET sidecar, but Electron's larger ecosystem (Monaco, React Flow) and more mature developer tooling outweigh its larger bundle size for an infrequently distributed desktop tool.
- Electron rather than a pure .NET UI: The requirements specify keyboard-first, graph visualisation, and fluid editor feel. Monaco provides the richest DSL editor experience available for a custom language with near-zero implementation cost. No .NET UI framework approaches it.
- Self-contained sidecar binary: game creators are not developers. Requiring .NET SDK installation is a friction barrier. A self-contained binary eliminates it.
- Request/response over stdin/stdout: simpler than a local TCP server (no port conflict, no firewall concern, no cleanup on crash — sidecar dies when main process dies). JSON-RPC is a well-understood protocol with libraries in both TypeScript and C#.

**Consequences:**
- A new `Archetype.Tooling.Server` project is added to the repository (a .NET console app). It references `Archetype.Core`, `Archetype.Build`, and `Archetype.Text`. It does not reference `Archetype.Engine`.
- A new `tooling/` directory at the repository root contains the Electron/TypeScript project. It is a separate `package.json` workspace from any Node tooling in the engine project.
- The `.sln` gains `Archetype.Tooling.Server`. D15's module boundary diagram gains a fifth box:

```
                      Archetype.Core
                  (pure data, interfaces)
             ↑         ↑         ↑         ↑
  ┌──────────┘  ┌──────┘  ┌──────┘  ┌─────┘
  │             │         │         │
Build         Text      Engine   Tooling.Server
(C# authoring) (renderer) (runtime) (sidecar — no Godot, no Engine dep)
```

- `Archetype.Tooling.Server` is the only assembly that has no WASM constraint — it runs only on desktop and may use file I/O, process spawning, and any .NET API freely.
- The sidecar's JSON-RPC protocol (method names, parameter shapes, response shapes) is an internal contract between the Electron main process and the sidecar. It is not a public engine API. It may evolve freely.
- Build tooling: `electron-builder` for packaging; `ts-node` or `tsx` for development; `esbuild` or `vite` for the renderer bundle. The specific choices are implementer-level; the constraint is that the build produces a single installable artifact per platform.

---

### D27 — Tooling Data Layer and Project File Format

**Decision:** Three related decisions govern how the authoring tool stores and manipulates game definition state.

1. **Sidecar-authoritative model.** The .NET sidecar is the single source of truth for the in-memory game definition. The renderer process is a pure view layer — it never holds a canonical copy of the definition, only the most recent data the sidecar returned. All mutations flow from renderer → main process → sidecar; all state reads are sidecar responses.

2. **Separate project file format.** The project file (`.archetype` — a JSON file) is a superset of the `GameDefinition` JSON schema, extended with a `tooling` metadata section. The sidecar uses a lenient project-file loader that reconstructs what it can from a definition containing validation errors and records broken fragments as diagnostics rather than throwing. The engine's `GameDefinitionLoader.FromJson` is not used for project files; it is used only for the strict export artifact that Godot loads at runtime.

3. **DSL text as the canonical form in the project file.** Keyword bodies, effect blocks, activation conditions, cost bodies, lifetime specs, and all other DSL-authored expressions are stored as raw DSL source strings in the project file. The parsed `KeywordNode` tree is always re-derived at load time (and on each edit) by the sidecar's parser. The exported game definition JSON (the engine's input) contains the tree, not the DSL text. The project file never contains both.

---

**Sidecar state model.**

The sidecar maintains one mutable `ProjectState` value in memory for the lifetime of a session. `ProjectState` is a lenient analogue of `GameDefinition` — it holds all the same fields but permits partially-parsed or structurally invalid fragments:

```
ProjectState {
  Id                     : string?
  Keywords               : Dictionary<string, KeywordEntry>
  Cards                  : Dictionary<string, CardEntry>
  Zones                  : Dictionary<string, ZoneEntry>
  Players                : Dictionary<string, PlayerEntry>
  CardSets               : Dictionary<string, CardSetEntry>
  Phases                 : List<PhaseEntry>
  ActionRules            : Dictionary<string, List<ActionRuleEntry>>
  StateBasedRules        : List<StateBasedRuleEntry>
  TriggerResolutionOrder : TriggerResolutionOrder
  InitManifest           : InitManifestEntry?
  PlayableZoneNames      : List<string>
  Localization           : LocalizationState
  Diagnostics            : List<ProjectDiagnostic>   // accumulated across all entries
}
```

Each `*Entry` type (e.g. `KeywordEntry`) carries the DSL source text for its expressions alongside the parse result — either a fully-parsed `KeywordNode` tree or a parse error that is stored as a diagnostic:

```
KeywordEntry {
  Name         : string
  Parameters   : List<ParameterDecl>
  ReturnType   : TypeName                // mandatory; no default; mirrors KeywordDefinition.ReturnType
  BodyDsl      : string                  // raw DSL text; canonical
  BodyNode     : KeywordNode?            // null if BodyDsl did not parse
  TextTemplate : string?
  Diagnostics  : List<ProjectDiagnostic>
}
```

`ReturnType` is mandatory. The `Validator` emits an error-severity `ProjectDiagnostic` for any keyword entry that has no return type declared. The exporter must read `ReturnType` from `KeywordEntry` when constructing a `KeywordDefinition` — hardcoding `TypeName.Atom` is incorrect. `ProjectFileLoader` must deserialise the `"returnType"` JSON field (case-insensitive enum parse, same pattern as parameter `"type"`).

This allows the sidecar to maintain a coherent, partially-valid state — keywords with parse errors are present in `ProjectState.Keywords` but have `BodyNode = null`. Downstream validation (e.g. type-checking call sites of a broken keyword) reports additional diagnostics referencing the broken entry by name rather than crashing.

**`CardEntry` schema:**

```
CardEntry {
  Name                   : string
  StaticProperties       : Dictionary<string, object>
  PrimaryEffectDsl       : string
  PrimaryEffectNode      : EffectBlockDef?
  AdditionalEffects      : List<NamedEffectEntry>
  StaticEffects          : List<StaticEffectEntry>
  ActivationConditionDsl : string?
  ActivationConditionNode: KeywordNode?
  Costs                  : List<CostEntry>
  FlavourText            : string?
  ArtPath                : string?
  ArtCropRegion          : float[4]?     // [x, y, width, height] normalised 0..1; null = full image
  Diagnostics            : List<ProjectDiagnostic>
}
```

`ArtCropRegion` must be serialised by `ProjectFileSerializer` (as a JSON array `[x, y, w, h]`) and deserialised by `ProjectFileLoader`. Omitting it from serialisation is a round-trip data loss bug — crop data would be silently discarded on every save.

**`StaticEffectEntry` schema:**

Static effects are not deferred. The exporter must produce correct `StaticEffectDef` output from `StaticEffectEntry`; generating an empty list is incorrect.

```
StaticEffectEntry {
  ContributionDsl      : string              // DSL for the state-contribution effect block (mandatory)
  ContributionNode     : EffectBlockDef?     // null on parse error
  TriggerEventKeyword  : string?             // keyword name the trigger subscribes to (e.g. "take-damage"); required when a trigger is defined
  TriggerScope         : TriggerScope        // event-log query window for condition evaluation (default: ThisAction)
  TriggerConditionDsl  : string?             // DSL for the event-log subscription condition (optional)
  TriggerConditionNode : KeywordNode?        // null when not set or on parse error
  TriggerBodyDsl       : string?             // DSL for the triggered effect block (optional)
  TriggerBodyNode      : EffectBlockDef?     // null when not set or on parse error
  LifetimeDsl          : string?             // DSL for the lifetime spec (optional; null = permanent)
  LifetimeNode         : LifetimeSpec?       // parsed lifetime; null when not set or on parse error
  Diagnostics          : List<ProjectDiagnostic>
}
```

`LifetimeSpec` is the parsed form of a lifetime spec string. The `LifetimeSpec` type is defined in `Archetype.Core` (D6). The sidecar parser must parse lifetime DSL strings into `LifetimeSpec` values; this is a distinct parse path from keyword DSL. A missing `LifetimeNode` when `LifetimeDsl` is non-empty must produce a diagnostic, the same as any other parse failure.

When `LifetimeDsl` is null or empty, the static effect is permanent — this matches `LifetimeSpec.Permanent` (D6).

The exporter maps `StaticEffectEntry` to `StaticEffectDef`:
- `ContributionNode` → `StaticEffectDef.Contribution` (required; skip entry if null with a diagnostic)
- `TriggerEventKeyword` + `TriggerScope` + `TriggerConditionNode` + `TriggerBodyNode` → `StaticEffectDef.Trigger` (optional; omit if `TriggerEventKeyword` is null or either node is null; emit a diagnostic if `TriggerConditionDsl` is set but `TriggerEventKeyword` is absent)
- `LifetimeNode ?? LifetimeSpec.Permanent` → `StaticEffectDef.Lifetime`

---

**Mutation protocol.**

The renderer sends fine-grained mutation commands. The sidecar applies the mutation, re-validates the affected scope, and returns an `UpdateResponse`:

```
// Request
{ "id": "r3", "method": "UpdateKeywordBody",
  "params": { "keywordName": "take-damage", "dsl": "modify-accumulator(target, \"damage\", amount)" } }

// Response
{ "id": "r3", "result": {
    "diagnostics": [],          // full current diagnostic list, scoped to this keyword and its call sites
    "globalDiagnosticCount": 0  // total error count across the whole project (for problems panel badge)
  }
}
```

Mutations are scoped: `UpdateKeywordBody`, `UpdateCardEffect`, `UpdateLifetimeSpec`, `AddCard`, `RemoveCard`, `RenameKeyword`, `UpdateInitManifest`, etc. Each mutation triggers re-validation of the affected entry and all entries that reference it (impact propagation). The sidecar computes the affected set from its internal reference graph and returns only the diagnostics relevant to the changed scope, plus the updated global count.

The full diagnostic list for the problems panel is fetched lazily via a separate `GetAllDiagnostics` request, not pushed on every keystroke.

---

**Project file format.**

File extension: `.archetype`. Encoding: UTF-8 JSON.

Top-level structure:

```json
{
  "version": 1,
  "id": "my-game",
  "keywords": {
    "take-damage": {
      "returnType": "Atom",
      "parameters": [
        { "name": "target", "type": "Card" },
        { "name": "amount", "type": "Number" }
      ],
      "body": "modify-accumulator(target, \"damage\", amount)",
      "textTemplate": "Deal {amount} damage to {target}"
    }
  },
  "cards": { ... },
  "zones": { ... },
  "players": { ... },
  "cardSets": { ... },
  "phases": [ ... ],
  "actionRules": { ... },
  "stateBasedRules": [ ... ],
  "triggerResolutionOrder": "OldestFirst",
  "initManifest": { ... },
  "playableZoneNames": [],
  "localization": {
    "sourceLanguage": "en",
    "strings": {
      "en": { "take-damage": "Deal {amount} damage to {target}" },
      "fr": { "take-damage": "Inflige {amount} blessure(s) à {target}" }
    }
  },
  "tooling": {
    "editorState": {
      "lastOpenedCard": "goblin",
      "expandedSections": ["keywords", "cards"]
    }
  }
}
```

All DSL-authored expressions appear as plain strings (e.g. `"body": "modify-accumulator(target, \"damage\", amount)"`). No `KeywordNode` tree appears in the project file.

The `tooling` section is opaque to the engine and to the sidecar's validation logic. Its schema is owned by the renderer; the sidecar round-trips it verbatim (reads on load, writes back on save without modification).

---

**Lenient project-file loader.**

`ProjectFileLoader` (in `Archetype.Tooling.Server`) reads a `.archetype` file into `ProjectState`. It proceeds field by field, recording parse and type errors as `ProjectDiagnostic` entries rather than throwing. A file with every keyword body containing a syntax error still loads — the sidecar reports all errors on startup and the game creator can fix them interactively.

Load sequence:
1. Parse top-level JSON. If the file is not valid JSON, report a single fatal diagnostic and return an empty `ProjectState`.
2. For each keyword: parse `body` DSL string. On success, populate `BodyNode`. On failure, set `BodyNode = null`, add diagnostic with source range from the DSL string.
3. After all entries are loaded, run the full cross-entry validation pass (name resolution, type checking, acyclicity). Add any further diagnostics.
4. Populate `ProjectState.Diagnostics` as the union of all per-entry and cross-entry diagnostics.

---

**`RenameEntry` must rewrite DSL source strings.**

DSL text is the canonical form in the project file (decision point 3 above). When `RenameEntry` renames a keyword, it must rewrite every DSL source string in every entry that references the old name — not only the in-memory `KeywordNode` trees. Rewriting only the in-memory trees causes a round-trip correctness bug: the project file saved to disk would contain stale DSL strings referencing the old name, which would produce unresolved-reference diagnostics on the next load.

Required rewrite scope for a keyword rename from `oldName` to `newName`:
- All `KeywordEntry.BodyDsl` fields that contain `oldName` as a keyword invocation
- All `CardEntry.PrimaryEffectDsl`, `CardEntry.ActivationConditionDsl`, and each `NamedEffectEntry.BodyDsl` and `CostEntry.BodyDsl` within that card
- All `StaticEffectEntry.ContributionDsl`, `TriggerConditionDsl`, `TriggerBodyDsl` within each card
- All `PhaseEntry.InitDsl`, `PhaseEntry.CleanupDsl`
- All `ActionRuleEntry.BeforeDsl`, `ActionRuleEntry.AfterDsl`
- All `StateBasedRuleEntry.ConditionDsl`, `StateBasedRuleEntry.BodyDsl`

The rewrite is a text substitution on the DSL string: replace occurrences of `oldName(` with `newName(` (the trailing `(` disambiguates a keyword invocation from a parameter or literal that happens to share the name). After rewriting DSL strings, re-parse all affected entries and update their `*Node` fields. Then rebuild the reference graph and run the validation pass as normal.

Card renames do not require DSL rewriting (card names do not appear in DSL expressions).

---

**ZoneSpec `definition` field.**

In `ProjectFileSerializer`, when serialising `InitManifestEntry` zones, the `"definition"` JSON field must hold the **zone definition name** — the key from `GameDefinition.ZoneDefinitions` — not the `LocalId`. These two values may differ. Writing `LocalId` as the `"definition"` value is incorrect and causes the loader to associate the zone with the wrong definition on reload.

Correct serialisation:
```json
{ "owner": "player1", "definition": "hand-zone", "localId": "p1-hand" }
```

where `"hand-zone"` is the `ZoneDefinition` key and `"p1-hand"` is the `ZoneSpec.LocalId`. The current `ProjectFileSerializer` implementation has this wrong — it writes `z.LocalId` for both `"definition"` and `"localId"`. The implementer must fix `ProjectFileSerializer.SerializeInitManifest` to write the correct definition name. `ZoneSpec` must therefore carry (or be derivable from) the zone definition name, not only the `LocalId`.

**Note on `ZoneSpec` shape:** `ZoneSpec(Owner, DefinitionName, LocalId)` — the definition name and local ID are separate fields. Confirm this matches the `Archetype.Core.ZoneSpec` constructor before fixing the serialiser.

---

**Save and autosave.**

Save is a `SaveProject` request. The sidecar serialises `ProjectState` to the `.archetype` JSON format (preserving `tooling` verbatim) and returns the JSON string to the main process, which writes it to disk. The sidecar does not perform file I/O directly — that is the main process's responsibility (D26).

Autosave fires every 60 seconds of inactivity (no mutation commands received). The main process triggers it by sending `SaveProject`; the response is handled silently. This is the primary mitigation for sidecar-crash data loss.

---

**Export (strict serialisation).**

Export is a separate `ExportGameDefinition` request. The sidecar:
1. Checks `ProjectState.Diagnostics` — if any errors exist, returns an error response (export blocked).
2. Constructs a strict `GameDefinition` from `ProjectState` by resolving all DSL strings to their `KeywordNode` trees (already parsed and cached).
3. Serialises using `System.Text.Json` to the `GameDefinition` JSON schema (the same format `GameDefinitionLoader.FromJson` reads). This is the tree-form JSON — no DSL source text.
4. Returns the JSON string to the main process, which writes the export artifact to disk.

The export artifact is not the project file. They are distinct files with distinct schemas. The export artifact is what the Godot project loads at runtime.

---

**Rationale:**
- Sidecar-authoritative: avoids maintaining a parallel TypeScript shadow of the C# `GameDefinition` type hierarchy. Every engine type change is reflected automatically without a TypeScript counterpart update.
- Separate project file: the engine's `GameDefinitionLoader.FromJson` validates strictly and throws on any error — it cannot represent the "save freely with errors" requirement. A lenient project-file loader is necessary. The superset approach keeps the two formats structurally close, minimising divergence.
- DSL text as canonical: the tree is a derived artefact. Storing the tree in the project file would mean re-parsing is never needed, but it would make the project file machine-generated and unreadable — defeating the intent of a text-first authoring format. DSL source strings are human-readable and diff-friendly. Re-parsing on load is fast (keyword bodies are short). The export converts to tree form once, at export time.
- Autosave every 60 seconds of inactivity: balances crash recovery against unnecessary disk writes. 60 seconds is a well-understood convention for desktop authoring tools.

**Consequences:**
- `Archetype.Tooling.Server` gains `ProjectState`, `KeywordEntry` (and parallel `*Entry` types for each definition kind), `ProjectDiagnostic`, `ProjectFileLoader`, and `ProjectFileSerializer`.
- `ProjectFileLoader` must be kept in sync with `GameDefinitionLoader.FromJson` in terms of what fields are recognised — new `GameDefinition` fields must be added to both loaders.
- `KeywordEntry.ReturnType` is mandatory. `ProjectFileLoader` must deserialise `"returnType"` from the keyword JSON object (case-insensitive enum parse). `Validator` must emit an error diagnostic for any keyword missing this field. `GameDefinitionExporter` must read `ReturnType` from `KeywordEntry`; hardcoding `TypeName.Atom` is incorrect.
- `CardEntry.ArtCropRegion` (`float[4]?`) must be serialised by `ProjectFileSerializer` as a JSON array and deserialised by `ProjectFileLoader`. Omitting it causes silent round-trip data loss.
- `StaticEffectEntry` must carry `TriggerEventKeyword : string?`, `TriggerScope : TriggerScope` (default `ThisAction`), `ContributionNode : EffectBlockDef?`, `TriggerConditionNode : KeywordNode?`, `TriggerBodyNode : EffectBlockDef?`, and `LifetimeNode : LifetimeSpec?`. `ProjectFileLoader` must parse all DSL fields and deserialise `TriggerEventKeyword` and `TriggerScope` from the project file. `GameDefinitionExporter` must pass `TriggerEventKeyword` and `TriggerScope` when constructing a `TriggerDefinition` — these fields are required to build the event-log subscription; emitting an empty trigger or an empty static-effect list is incorrect.
- `RenameEntry` for keywords must rewrite all `*Dsl` source strings across all affected entries (see "RenameEntry must rewrite DSL source strings" above), not only in-memory node trees. Failing to rewrite DSL strings is a round-trip correctness bug.
- `ProjectFileSerializer.SerializeInitManifest` must write the zone definition name into the `"definition"` field, not the `LocalId`. The current implementation has this wrong. See "ZoneSpec `definition` field" above for the correct shape.
- The sidecar's internal reference graph (for impact propagation) must be rebuilt incrementally on each mutation. The initial implementation may rebuild it fully on each mutation; optimisation to incremental rebuild is deferred.
- The `tooling.editorState` schema is a renderer-level concern. No schema validation is performed by the sidecar; it is stored and returned verbatim.
- File I/O (read project file, write project file, write export artifact) is always performed by the Electron main process. The sidecar receives file contents as strings and returns serialised strings. This maintains the D26 responsibility split cleanly.

---

### D29 — D14 Addendum: InitManifest Mandatory, HostManifest Append Layer, LocalId Uniqueness

**Decision:** `InitManifest` is required on every `GameDefinition` — it is non-nullable and renamed from `DefaultInitManifest`. The host may supply an optional `HostManifest` at session build time that appends zones, appends cards, and patches mutable state on any atom provisioned by `InitManifest`. `LocalId` uniqueness is enforced across the union of both manifests' zones. `CardSpec` gains an optional `LocalId` so host state overrides can target specific cards by name.

---

**1. `GameDefinition.InitManifest` — mandatory, non-nullable.**

`GameDefinition.DefaultInitManifest : InitManifest?` is replaced by:

```
GameDefinition {
  ...
  InitManifest : InitManifest    // required; non-nullable; renamed from DefaultInitManifest
  ...
}
```

`GameDefinitionBuilder.Build()` throws `DefinitionException` if `InitManifest` has not been set. An `InitManifest` with all empty lists is valid — it satisfies the requirement. There is no longer a code path that begins a session with no atoms unless the game creator explicitly provides an empty manifest.

`GameDefinitionBuilder` gains:

```
GameDefinitionBuilder
  .WithInitManifest(Action<ManifestBuilder>) → self   // replaces the old optional overload; now required
```

This call is now required before `Build()`. The `ManifestBuilder` API is otherwise unchanged.

---

**2. `GameSessionBuilder` — removal of manifest-choice methods.**

The session builder previously offered a manifest-selection step (choose one of: adopt default, provide custom, no-manifest escape hatch). With `InitManifest` mandatory on `GameDefinition`, this choice is gone. `InitManifest` is always applied.

Removed from `GameSessionBuilder`:
- `.UseDefaultInit()` — no longer needed; `InitManifest` is always used.
- `.WithInitManifest(InitManifest)` — replacement mode; removed. The host cannot replace the manifest.
- `.WithInitManifest(Action<ManifestBuilder>)` — replacement mode; removed.
- The escape hatch "if none is called the session begins with no atoms" — removed.

Updated `GameSessionBuilder`:

```
GameSessionBuilder
  .WithPlayerStrategy(string playerName, IPlayerStrategy) → self
  .WithRandomSource(IRandomSource)                        → self
  .WithObserver(IEngineObserver)                          → self
  .WithHostManifest(HostManifest)                         → self   // NEW — optional
  .WithHostManifest(Action<HostManifestBuilder>)          → self   // NEW — optional fluent overload
  .FromSavedState(GameStateSnapshot)                      → self   // D17 — deferred; unchanged
  .Build() → GameSession
```

`.WithHostManifest(...)` and `.FromSavedState(...)` are mutually exclusive; calling both is a `SessionException` at `.Build()` time.

---

**3. `HostManifest` — append and patch layer.**

```
HostManifest {
  Zones          : IReadOnlyList<ZoneSpec>           // appended after InitManifest zones
  Cards          : IReadOnlyList<CardSpec>           // appended after InitManifest cards
  StateOverrides : IReadOnlyList<AtomStateOverride>  // patched after all provisioning
}
```

`HostManifest` uses the same `ZoneSpec` and `CardSpec` shapes as `InitManifest` (including the extended `CardSpec` with optional `LocalId` — see §4). Omitting `.WithHostManifest(...)` entirely is equivalent to supplying an empty `HostManifest`; the session begins with only the atoms declared in `InitManifest`.

`HostManifestBuilder` (fluent API):

```
HostManifestBuilder
  .AddZone(string localId, string owner, string definition,
           Action<ZoneStateBuilder>? state = null)          → self
  .AddCard(string owner, string zoneLocalId, string definition,
           string? localId = null,
           Action<CardStateBuilder>? state = null)          → self
  .OverrideZoneState(string localId,
           Action<StateOverrideBuilder>)                    → self
  .OverrideCardState(string localId,
           Action<StateOverrideBuilder>)                    → self
  .OverridePlayerState(string playerName,
           Action<StateOverrideBuilder>)                    → self
```

---

**4. `CardSpec` — optional `LocalId`.**

`CardSpec` gains an optional `LocalId` field:

```
CardSpec {
  LocalId      : string?    // optional; required only if host state overrides target this card
  Owner        : string
  ZoneLocalId  : string
  Definition   : string
  Accumulators : IReadOnlyDictionary<string, double>?
  Conditions   : IReadOnlyList<string>?
}
```

Cards that the host never needs to target by name do not require a `LocalId`. When present, `LocalId` must be unique within `InitManifest.Cards` and, if also set on `HostManifest` cards, unique within `HostManifest.Cards`. Card `LocalId`s and zone `LocalId`s do not share a namespace — a zone and a card may carry the same `LocalId` string without conflict.

Cards with `LocalId = null` cannot be targeted by `AtomStateOverride.CardTarget`. Attempting to do so is a `SessionException` at `.Build()` time.

---

**5. `AtomStateOverride` — discriminated union.**

```
AtomStateOverride {
  Target       : OverrideTarget
  Accumulators : IReadOnlyDictionary<string, double>?
  Conditions   : IReadOnlyList<string>?
}

OverrideTarget (discriminated union):
  | ZoneTarget   { LocalId    : string }   // matches a ZoneSpec.LocalId in InitManifest or HostManifest
  | CardTarget   { LocalId    : string }   // matches a CardSpec.LocalId in InitManifest (non-null)
  | PlayerTarget { PlayerName : string }   // matches a PlayerDefinition name
```

**Accumulator overrides — merge (patch).** For each key in `AtomStateOverride.Accumulators`, the corresponding accumulator on the target atom is set to the override value. Keys absent from the override are left at their `InitManifest`-provisioned values. The override does not replace the entire accumulator map.

**Condition overrides — append.** Each condition name in `AtomStateOverride.Conditions` is applied additively to the target atom. Conditions already present from `InitManifest` are unaffected. No conditions are removed by a host override.

`CardTarget` and `PlayerTarget` may target only atoms provisioned by `InitManifest`. Targeting a `HostManifest`-added card via `CardTarget`, or a player (which is always engine-managed) via `PlayerTarget`, is a `SessionException` — those atoms are configured at construction time via their `ZoneSpec`/`CardSpec` `Accumulators` and `Conditions` fields directly.

`ZoneTarget` is an explicit exception: it may match a zone `LocalId` from either `InitManifest` or `HostManifest` zones. Zones are part of the shared game structure, not player-specific initial state, so patching a host-appended zone's mutable state via `StateOverrides` is coherent and permitted. Targeting a `LocalId` that does not exist in either manifest is still a `SessionException`.

---

**6. `LocalId` uniqueness rules.**

*Zone `LocalId`s* — enforced across the union of `InitManifest.Zones` and `HostManifest.Zones`:
- Duplicate within `InitManifest.Zones` → `DefinitionException` at `GameDefinitionBuilder.Build()`.
- `HostManifest` zone `LocalId` colliding with any `InitManifest` zone `LocalId` → `SessionException` at `GameSessionBuilder.Build()`.
- Duplicate within `HostManifest.Zones` → `SessionException`.

*Card `LocalId`s* (when non-null) — enforced per manifest separately:
- Duplicate non-null `LocalId` within `InitManifest.Cards` → `DefinitionException` at `GameDefinitionBuilder.Build()`.
- Duplicate non-null `LocalId` within `HostManifest.Cards` → `SessionException` at `GameSessionBuilder.Build()`.

`HostManifest` card and zone `ZoneLocalId` references may point to `LocalId`s from either `InitManifest` or `HostManifest` zones, subject to declaration order: a `HostManifest` card may reference a `HostManifest` zone declared earlier in `HostManifest.Zones`, but not one declared later in the same list.

---

**7. Updated provisioning order.**

The nine-step sequence replaces D14's original six-step sequence:

1. Session atom created (engine-managed).
2. Player atoms created from `PlayerDefinitions` in insertion order; `LocalId` → `AtomId` map entry added per player name.
3. `InitManifest` zones created in `Zones` list order; `LocalId` → `AtomId` map populated.
4. `InitManifest` cards created in `Cards` list order; placed in declared zone; declarative static effects instantiated (D6). Non-null `CardSpec.LocalId` values added to the `LocalId` → `AtomId` map.
5. `InitManifest` card mutable state (`Accumulators`, `Conditions`) applied.
6. `InitManifest` player state overrides (`PlayerStates`) applied.
7. *(new)* `HostManifest` zones created in `Zones` list order; `LocalId` → `AtomId` map extended.
8. *(new)* `HostManifest` cards created in `Cards` list order; placed in declared zone (referencing `LocalId`s from either manifest); declarative static effects instantiated.
9. *(new)* `HostManifest.StateOverrides` applied in list order: accumulators merged (patch), conditions appended.

No events are logged during any provisioning step (unchanged from D14). The `LocalId` → `AtomId` map is internal to the provisioning routine and discarded after step 9 completes.

---

**Rationale:**
- Mandatory `InitManifest` removes a class of silent bugs where a host omits the manifest and the game starts with an empty world that game rules assume to be populated. The empty-lists case is valid for games that build all state through phase init blocks.
- `HostManifest` as an append-and-patch layer (never a replacement) preserves the game designer's invariants: zones exist, player state is initialised, known cards are present. The host extends and patches but cannot remove or replace what the definition declares.
- Patch semantics for accumulator overrides: a host setting `damage: 3` on a carried-over card should not have to re-specify every other accumulator. Patch is the natural semantics for "set these specific values, leave the rest."
- Append semantics for condition overrides: conditions are presence flags. The engine uses `ConditionIndex[name].Count > 0`; adding a condition the atom already has raises the count harmlessly. Removing conditions via a host override is a design-time decision that belongs in the `InitManifest`, not in a session-time patch.
- Optional `LocalId` on `CardSpec`: most cards in an `InitManifest` are fixed background structure the host never needs to target individually. Requiring `LocalId` on every `CardSpec` would be noise. Opt-in keeps the common case uncluttered.

**Consequences:**
- `GameDefinition.DefaultInitManifest : InitManifest?` is a **breaking change** — field renamed to `InitManifest : InitManifest` (non-nullable). All existing read sites must be updated.
- `GameDefinitionBuilder` now requires `.WithInitManifest(...)` before `.Build()`. Existing builders that omit it will get a `DefinitionException` at build time.
- `GameSessionBuilder` loses three methods. Any caller using `.UseDefaultInit()`, `.WithInitManifest(InitManifest)`, or `.WithInitManifest(Action<ManifestBuilder>)` must be updated. The intent is now expressed via `GameDefinitionBuilder.WithInitManifest(...)` at definition time.
- `CardSpec` gains `LocalId : string?` — backward-compatible; null is the default at all existing construction sites (builders, JSON deserialiser).
- `HostManifest`, `AtomStateOverride`, `OverrideTarget`, and `HostManifestBuilder` are new types in `Archetype.Core` and `Archetype.Build` respectively.
- The provisioning routine in `Archetype.Engine` extends from 6 steps to 9. The `LocalId` → `AtomId` map must persist across all nine steps; it was previously discardable after step 4.
- D17 (`FromSavedState`) is unaffected — save points are at turn boundaries, after provisioning is complete. No snapshot captures `LocalId` state.
- The tooling (`Archetype.Tooling.Server`) must treat `InitManifest` as a required section in `ProjectState`. The authoring UI must communicate clearly that the field is mandatory before export is possible.

---

### D28 — Tooling Validation Approach

**Decision:** Validation is triggered on every DSL field content-change event, debounced at a configurable delay (default 200ms). The sidecar returns scoped diagnostics — covering the changed entry and its reverse-dependency closure — plus a global error count. Autocomplete queries are routed to the sidecar (not computed locally in the renderer). The initial sidecar implementation validates the full project on every mutation; the API surface is designed for incremental validation from day one so the implementation can be tightened without a protocol change.

---

**Trigger model.**

| Field kind | Trigger | Debounce |
|---|---|---|
| DSL text fields (keyword body, effect block, lifetime spec, activation condition, cost body, text template) | Every content-change event (keystroke) | Configurable delay, default 200ms |
| Non-DSL fields (name, type selector, dropdown, checkbox, list reorder) | Immediate on commit (blur / Enter / selection) | None |
| Structural mutations (add entry, remove entry, rename entry) | Immediate | None |

Debounce is implemented in the renderer: a change event starts or resets a timer; when the timer fires the mutation command is sent to the sidecar. While the timer is running, the editor shows diagnostics from the previous sidecar response (stale by at most one debounce period). This is the standard Monaco / VS Code validation pattern.

**Configurable debounce delay.** The debounce delay is stored in the tool's user settings (persisted in the `tooling.editorState` section of the project file — or, for global settings, in a separate user-preferences file managed by the Electron main process). The renderer reads it at startup and after any settings change. The default is 200ms. The valid range is 50ms–2000ms; values outside this range are clamped. No restart is required — the renderer applies the new delay immediately to the next debounce timer.

---

**Validation scope per mutation.**

The sidecar maintains an internal reference graph: `usedBy[entryName] = {set of entry names that reference entryName}`. On every mutation the affected set is computed as:

```
affectedSet(changed) =
  { changed }
  ∪ transitiveClosure(usedBy, changed)
```

The sidecar re-validates every entry in `affectedSet`, collects the resulting diagnostics, and returns them in the response. The renderer replaces its cached diagnostics for those entries and updates the global count badge.

**Initial implementation:** the affected set is always "all entries" — a full project re-validation on every mutation. The response shape is identical; only the set computed is larger than necessary. The optimisation to true incremental validation is deferred and requires no protocol change.

---

**Response shape for mutation commands.**

All mutation methods (`UpdateKeywordBody`, `UpdateField`, `AddEntry`, `RemoveEntry`, `RenameEntry`, and the per-kind DSL update methods) return:

```json
{
  "id": "r3",
  "result": {
    "affectedEntries": ["take-damage", "attack", "goblin"],
    "diagnostics": [
      {
        "entryKind": "keyword",
        "entryName": "take-damage",
        "severity": "error",
        "message": "Unknown keyword 'mmodify-accumulator'",
        "dslRange": { "start": 0, "end": 19 }
      }
    ],
    "globalErrorCount": 1,
    "globalWarningCount": 0
  }
}
```

`dslRange` is a character-offset range into the DSL source string for the affected entry. The renderer translates this to a Monaco `IRange` (line/column) using the Monaco model's `getPositionAt` method. Cross-entry diagnostics (e.g. a broken call site in `attack` caused by renaming `take-damage`) reference the call-site entry in `entryName` and provide the range within that entry's DSL text.

---

**Autocomplete.**

Autocomplete queries are routed to the sidecar. The renderer registers a Monaco `CompletionItemProvider` that fires on any character typed inside a DSL field. On trigger, it sends:

```json
{ "id": "r4", "method": "GetCompletions",
  "params": {
    "entryKind": "keyword", "entryName": "attack",
    "dsl": "take-damage(target, max(0, amount - ",
    "cursorOffset": 37
  }
}
```

The sidecar partially parses the DSL up to the cursor, determines the syntactic context (e.g. "inside the second argument of `max`"), and returns typed completion items:

```json
{ "id": "r4", "result": {
    "items": [
      { "label": "amount", "kind": "parameter", "detail": "Number", "insertText": "amount" },
      { "label": "get-state", "kind": "keyword", "detail": "(atom: Atom, field: PropertyName) → Number", "insertText": "get-state($1, $2)" }
    ]
  }
}
```

`kind` maps to Monaco `CompletionItemKind` values (Variable, Function, etc.) for icon display. `insertText` uses Monaco snippet syntax (`$1`, `$2` for tab stops) where appropriate.

The sidecar must respond within 100ms to avoid Monaco's autocomplete timeout. Given that the sidecar holds its parsed state in memory and a completion query is a partial parse + scope lookup (no I/O), this is reliably achievable.

No debounce is applied to `GetCompletions` — Monaco controls the autocomplete trigger timing.

---

**Problems panel (`GetAllDiagnostics`).**

The problems panel is populated lazily: when the panel is opened (or the game creator navigates to it), the renderer sends `GetAllDiagnostics`:

```json
{ "id": "r5", "method": "GetAllDiagnostics", "params": {} }
```

Response: the full `ProjectDiagnostic[]` sorted by severity then entry name. The renderer renders these as a navigable list. The global error/warning count badge in the toolbar is maintained via the `globalErrorCount`/`globalWarningCount` fields in every mutation response — the full list is only fetched when the panel is visible.

---

**Symbol navigation (`GetSymbolInfo`).**

Used for Cmd+Click ("go to definition") and hover tooltips in DSL fields. The renderer sends:

```json
{ "id": "r6", "method": "GetSymbolInfo",
  "params": { "entryKind": "keyword", "entryName": "attack", "cursorOffset": 3 }
}
```

The sidecar identifies the symbol at the cursor (e.g. the keyword name `take-damage` starting at offset 0), and returns:

```json
{ "id": "r6", "result": {
    "symbol": "take-damage",
    "kind": "keyword",
    "definition": { "entryKind": "keyword", "entryName": "take-damage" },
    "referencedBy": [
      { "entryName": "attack" },
      { "entryName": "goblin" }
    ]
  }
}
```

`referencedBy` items carry only `entryName` — the sidecar's reference graph tracks callers by name without recording the caller's kind. The renderer uses `definition` to navigate to the target entry (switching panels if necessary) and `referencedBy` to populate the "used by" list in the graph navigation sidebar.

---

**Full sidecar method surface.**

| Method | Trigger | Returns |
|---|---|---|
| `UpdateKeywordBody` | DSL field change (debounced) | Scoped diagnostics + global counts |
| `UpdateCardEffect` | DSL field change (debounced) | Scoped diagnostics + global counts |
| `UpdateLifetimeSpec` | DSL field change (debounced) | Scoped diagnostics + global counts |
| `UpdateActivationCondition` | DSL field change (debounced) | Scoped diagnostics + global counts |
| `UpdateCostBody` | DSL field change (debounced) | Scoped diagnostics + global counts |
| `UpdateField` | Non-DSL field change (immediate) | Scoped diagnostics + global counts |
| `AddEntry` | Add card / keyword / zone / player / phase / rule | New entry summary + scoped diagnostics |
| `RemoveEntry` | Remove any entry | Scoped diagnostics (call-site orphans) |
| `RenameEntry` | Rename any named entry | Impact diagnostics across all references |
| `GetCompletions` | Cursor in DSL field | `CompletionItem[]` |
| `GetAllDiagnostics` | Problems panel opened | Full `ProjectDiagnostic[]` |
| `GetSymbolInfo` | Hover / Cmd+Click on DSL token | Symbol definition + `referencedBy` list |
| `GetReferenceGraph` | Graph view opened | Nodes + edges for keyword composition graph |
| `RenderCardText` | Card text preview opened / card edited | `RenderNode` tree |
| `SaveProject` | Cmd+S / autosave | `.archetype` JSON string |
| `LoadProject` | File open | `ProjectState` summary + full diagnostics |
| `ExportGameDefinition` | Export (gated on 0 errors) | `GameDefinition` JSON string |
| `ExportGodotClasses` | Export (gated on 0 errors) | GDScript source map `{ filename → content }` |

---

**Rationale:**
- Debounced on every change (not on blur) directly satisfies the "errors are flagged immediately, not on save" requirement. Blur-only validation would satisfy it only loosely.
- Configurable debounce: 200ms is a well-established default (VS Code uses 300ms; many LSP servers target 200ms) but the right value depends on sidecar response latency on the specific hardware running the tool. Exposing it as a setting costs nothing and avoids a future support complaint.
- Scoped diagnostics per mutation: the renderer always has an up-to-date diagnostic set for everything that changed, without waiting for a full-list fetch. The global count badge updates on every response, so the game creator always knows whether the definition is clean.
- Autocomplete via sidecar: the sidecar is the only entity with a complete, type-checked view of what is in scope at any cursor position. A renderer-side approximation would require duplicating the DSL parser and type system in TypeScript — high cost, permanent maintenance burden.
- Lazy problems panel: fetching the full diagnostic list on every keystroke would generate unnecessary data over the pipe when the panel is not visible. The badge (from mutation responses) provides the necessary "at a glance" health signal.

**Consequences:**
- `Archetype.Tooling.Server` gains a partial DSL parser for completion context determination (parses up to the cursor, tolerates incomplete input). This is a subset of the full parser — it needs to identify the syntactic position at the cursor but not fully validate the fragment.
- The reference graph (`usedBy`) is maintained as a mutable field on `ProjectState` and rebuilt on every `AddEntry`, `RemoveEntry`, `RenameEntry`, and DSL mutation. Full rebuild is O(entries × references) — acceptable for card-game-scale definitions.
- Monaco `CompletionItemProvider` in the renderer is registered once per DSL editor instance. It routes all completion requests through the main process IPC to the sidecar. The 100ms response budget must be monitored during development; if it is routinely exceeded, the sidecar's partial parser must be profiled and optimised before release.
- User settings storage: the debounce delay and any other user-configurable tool settings are persisted in a platform-appropriate location (e.g. `app.getPath('userData')` in Electron, in a `settings.json` file managed by the main process). They are not stored in the project file — they are per-user, not per-project.

---

### D30 — Godot Export Pipeline

**Decision:** The export package is written as a folder of files directly into a game-creator-specified directory inside their Godot project (e.g. `res://archetype-export/`). GDScript signals are derived from every keyword referenced in any card effect block, cost body, or static effect in the game definition, with built-in primitives suppressed by default and a per-keyword `[NoSignal]` opt-out available. Signal delivery uses post-action event log polling via `GameStateView` — no engine API changes are required.

---

**Export package layout.**

The tool writes the following structure into the configured output directory on each export:

```
<output-dir>/
  game-definition.json          ← GameDefinition JSON (engine input; Archetype.Engine loads this)
  interop/
    ArchetypeInterop.gd         ← one-time C#→GDScript wrapper (regenerated only on engine API change)
  generated/
    ArchetypeCard.gd            ← per-game Card class with signals and static properties
    ArchetypeZone.gd            ← per-game Zone class
    ArchetypeSession.gd         ← per-game Session class
    ArchetypePlayer.gd          ← per-game Player class
    ArchetypeCardImporter.gd    ← card importer utility
  art/
    <card-definition-name>.png  ← cropped art asset per card, named by CardDefinition.Name
```

Generated files are always overwritten on export. The interop wrapper (`ArchetypeInterop.gd`) is overwritten only when the game creator explicitly triggers a "regenerate interop" action — it is not touched on routine game-definition exports.

The output directory is stored in `tooling.editorState` in the project file. The game creator configures it once (browse to Godot project root, specify subfolder). The tool validates that the directory exists before exporting.

---

**Signal derivation rules.**

The tool derives the signal set by scanning the game definition for keyword references and applying the following rules:

**Inclusion rule:** A keyword produces a signal if it is referenced (directly or via composition) in at least one of:
- A card's primary effect block
- A card's named effect block body or cost body
- A static effect's state contribution block or trigger fired block

Keyword definitions that exist in the definition but are never referenced in any card — pure utility keywords used only by other keywords — do not themselves produce signals. A signal is produced for a keyword only if an event carrying that `KeywordName` could plausibly be appended to the log as a result of a player action.

**Default suppression:** Built-in primitives (`modify-accumulator`, `apply-modifier`, `apply-condition`, `remove-modifier`, `create-card`, `copy-card`, `create-zone`, `move-card`, `declare-winner`, `declare-draw`, and all read primitives) are suppressed by default. The game creator explicitly opts a primitive in by adding a `[Signal]` annotation to a keyword definition in the DSL (or via a toggle in the authoring tool). The rationale: primitive events are implementation details; composite keywords carry semantic meaning the UI cares about.

**Opt-out suppression:** Any game-creator-defined keyword can be suppressed with a `[NoSignal]` annotation in its DSL definition or via the authoring tool. This covers internal scaffolding keywords that generate events the UI does not need to react to.

**Composition depth:** The inclusion rule uses direct reference only (not transitive). If `attack` calls `take-damage` and `take-damage` is the semantically interesting event, both appear in the signal set if both are referenced in card effect blocks. If `take-damage` is only ever called from within `attack` and never appears directly in a card effect block, it does not produce a signal — only `attack` does. This keeps the signal set at the level of granularity the game creator authored, not at the level of every primitive transitively invoked.

**`keyword-disabled` engine event:** Always suppressed from signal generation. The game creator can opt it in per-suppressed-keyword via the authoring tool if needed.

---

**Signal naming and shape.**

Each included keyword `foo-bar` produces a GDScript signal named `on_foo_bar` (hyphens converted to underscores, prefixed with `on_`). Signal parameters mirror the keyword's declared `ParameterDecl` list, with engine type vocabulary mapped to GDScript types:

| Engine type | GDScript parameter type |
|---|---|
| `Number` | `float` |
| `Boolean` | `bool` |
| `Atom` / `Card` / `Zone` / `Player` / `Session` | `int` (the `AtomId` long, truncated to int for GDScript) |
| `String` / `ConditionName` / `PropertyName` | `String` |
| `ContributionId` | `int` |

Signal parameters use the declared parameter names from `ParameterDecl`. Example for `take-damage(target: Card, amount: Number)`:

```gdscript
signal on_take_damage(target: int, amount: float)
```

---

**Per-game domain classes.**

Four classes are generated, one per atom kind. Each class:
- Extends `RefCounted` (no Godot node lifecycle required; pure data/signal carrier).
- Declares `static var` properties for each static property declared in the game's static property schema for that entity kind (Cards, Zones, Players respectively).
- Declares all signals derived for the relevant atom kind (Card signals on `ArchetypeCard`, etc.).
- Holds an `atom_id: int` property identifying which engine atom this instance corresponds to.

```gdscript
# ArchetypeCard.gd — generated; do not edit
class_name ArchetypeCard extends RefCounted

var atom_id: int

# Static properties (from game's static property schema for Cards)
var mana_cost: int = 0
var is_legendary: bool = false

# Signals (derived from keyword usage in card effect blocks)
signal on_take_damage(target: int, amount: float)
signal on_attack(attacker: int, target: int, amount: float)
```

Session and Player classes follow the same structure for their respective static property schemas. Zone class carries zone-schema properties.

---

**Signal delivery — post-action event log polling.**

The interop wrapper (`ArchetypeInterop.gd`) holds a reference to the running `GameSession` (via the C# `GameSession` public API, accessed through Godot's C# interop). After each `ResolveAction` call completes, the wrapper reads the newly appended events from `GameStateView` and emits the corresponding signals on the appropriate class instances.

Delivery sequence (per action):

1. `GameSession.ResolveAction(...)` awaits completion (all blocks, SBRs, triggers resolved).
2. The wrapper reads the events appended since the last delivery checkpoint from `GameStateView`. `GameStateView` exposes the finalized action's events — this is a new thin property on the existing `GameStateView`, not a new observer hook:

```
GameStateView {
  ...
  LastActionEvents : IReadOnlyList<GameEvent>   // events from the most recently completed action; reset each action
}
```

3. For each `GameEvent` whose `KeywordName` is in the derived signal set, the wrapper looks up which atom kind the primary `Atom`-typed argument resolves to, retrieves the matching `ArchetypeCard`/`ArchetypeZone`/etc. instance (keyed by `AtomId`), and emits the signal on it.
4. The delivery checkpoint advances. Events from this action are not re-delivered on the next action.

This approach requires one small addition to `GameStateView` (`LastActionEvents`) but no changes to the engine's core execution path, `IEngineObserver`, or `ActionResolver`.

**Ordering:** Signals fire in event `SequenceNumber` ascending order — the same order the engine appended them. This matches the game creator's intuition about "what happened first."

**Atom instance registry:** The wrapper maintains a `Dictionary<int, ArchetypeCard>` (and equivalent for other kinds) mapping `AtomId` → class instance. Instances are created during provisioning (before `RunAsync`) and destroyed when the session ends. The card importer populates this registry.

---

**`ArchetypeCardImporter.gd` — card importer utility.**

Generated as part of the per-game output. Responsibilities:

1. Loads `game-definition.json` and calls the C# `GameDefinitionLoader.FromJson` via `ArchetypeInterop.gd`.
2. After `GameSession` is built and provisioned, iterates all card atoms in `GameStateView`, creates an `ArchetypeCard` instance per atom, populates static properties from the card definition, and registers the instance in the wrapper's atom registry.
3. Loads art assets from `art/<card-definition-name>.png` (via Godot's `load()`) and attaches them to the corresponding `ArchetypeCard` instance as a `Texture2D` property.
4. Returns the populated registry to the caller (the Godot scene that owns the game session).

This is the only hand-wiring required for a new Godot prototype: call `ArchetypeCardImporter.setup(session, output_dir)` once after session creation.

---

**One-time interop wrapper (`ArchetypeInterop.gd`).**

Generated from the engine's public C# API surface, not from the game definition. It wraps:
- `GameSession` construction (`GameSession.Create(...)`, `GameSessionBuilder` fluent methods, `.Build()`, `.RunAsync()`)
- `GameStateView` read access (atoms, accumulators, conditions, computed properties)
- `IPlayerStrategy` bridging (GDScript implementations of strategy methods, routed back to C# via `Callable`)
- `LastActionEvents` access for signal delivery

Generated once and checked into the Godot project. The tool offers a "Regenerate Interop" action that re-derives it from the current engine assemblies. The game creator runs this action only after an engine update.

The interop wrapper is the only file in the export that deals with raw C# types. All other generated GDScript is pure GDScript.

---

**`[NoSignal]` and `[Signal]` annotations in the DSL.**

Annotations are line-level comments above a keyword definition, parsed by the sidecar:

```
// [NoSignal]
apply_shield(target, amount) = apply-modifier(target, "defense", additive, amount, permanent)

// [Signal]
modify-accumulator(atom, name, delta)  // opt a primitive in
```

The authoring tool surfaces these as checkboxes in the keyword editor ("Generate GDScript signal"). The DSL annotation and the UI control are kept in sync; changing one updates the other.

---

**Rationale:**
- Folder drop into the Godot project is the standard Godot asset workflow. Godot's file system dock refreshes automatically when files change. No unzip step, no manual import — the game creator exports and immediately sees updated files in their Godot editor.
- Level 1 signal derivation (card-referenced keywords) at composition depth 1 puts signals at the semantic level the game creator authored — the level they think about when writing Godot UI logic. Transitive inclusion would flood the signal set with implementation-level primitives.
- Post-action polling via `LastActionEvents` avoids any change to the engine's execution path or `IEngineObserver` contract. The one small addition (`LastActionEvents` on `GameStateView`) is a read-only projection of state the engine already computes — it adds no new logic, only a new accessor.
- `[NoSignal]` opt-out on composite keywords and `[Signal]` opt-in on primitives gives the game creator complete control without requiring them to touch code — both paths are available through the authoring tool UI.

**Consequences:**
- `GameStateView` gains `LastActionEvents : IReadOnlyList<GameEvent>` — a thin accessor that returns the events finalized during the most recently completed `ResolveAction` call. The engine resets this list at the start of each new action. This is the only engine change required by D30.
- `Archetype.Tooling.Server` gains `ExportGodotClasses` and the code-generation pipeline. GDScript is generated as plain strings and returned to the Electron main process (which writes the files).
- The output directory configuration must be validated at export time: the directory must exist, must be writable, and must be inside a valid Godot project (the tool checks for a `project.godot` file in the parent hierarchy). If validation fails, export is blocked with a clear error message.
- The `[NoSignal]` / `[Signal]` annotations extend the DSL grammar. The sidecar's DSL parser must handle them as optional line annotations. They have no effect on keyword execution — they are tooling-only metadata.
- `SignalBehaviour` is a tooling-only enum (`Default` | `Suppress` | `ForceInclude`) stored on `KeywordEntry` in the sidecar's `ProjectState` — not on `KeywordDefinition` in `Archetype.Core`. Placing a tooling-only field on a `Core` type would violate D15's module boundary (Core is pure engine data; it must not carry tooling metadata). The sidecar's project-file loader reads `[NoSignal]`/`[Signal]` annotations from DSL text and populates `KeywordEntry.SignalBehaviour`; the export step reads this field when generating GDScript but does not include it in the `GameDefinition` JSON written to `game-definition.json`.
- Art assets are written to `art/<card-definition-name>.png`. Card definition names must be valid file-system names (no path separators, no null bytes). The tool validates this at authoring time; the sidecar reports a diagnostic if a card name would produce an invalid filename on any supported platform.

---

### D31 — Missing-Translation Export Gate UX

**Decision:** Missing translations are classified as warnings (never errors) and never block saving. At export time, if any missing-translation warnings exist, the export flow surfaces a narrowly-scoped confirmation dialog before proceeding. Hard errors block export before this dialog is ever reached — by the time the dialog appears, the definition is clean of errors. The dialog is strictly about missing translations and contains no other diagnostic information. General error status belongs in the persistent problems panel and status bar, not in the export dialog.

---

**Warning classification.**

The sidecar classifies missing-translation diagnostics with `severity: "warning"`, not `"error"`. They appear in the problems panel under a "Localization" group. The global warning count badge in the toolbar reflects them. They do not increment `globalErrorCount` and do not block the `ExportGameDefinition` or `ExportGodotClasses` sidecar calls from proceeding past the hard-error gate.

A missing-translation warning is generated for each (keyword, locale) or (card-name, locale) pair where the target locale has no entry for a key that exists in the source language. If the game creator has added no additional locales, no warnings are generated — a single-language game is always complete.

---

**Export flow.**

Export is initiated by the game creator (keyboard shortcut or menu action). The sequence:

1. Renderer sends `ExportGameDefinition` to the sidecar.
2. Sidecar checks `globalErrorCount`. If > 0, returns an error response. Renderer shows an inline error banner ("Cannot export: X errors remain. Open the Problems panel to review."). Flow ends.
3. Sidecar checks for missing-translation warnings. If none exist, proceeds directly to step 5.
4. If missing-translation warnings exist, the sidecar returns a `MissingTranslationSummary` in the response:

```json
{
  "missingTranslations": [
    { "locale": "fr", "missingCount": 3 },
    { "locale": "de", "missingCount": 12 }
  ]
}
```

The renderer presents a modal dialog:

```
Export with incomplete translations?

The following locales have missing strings:
  French     — 3 missing
  German     — 12 missing

Missing strings will fall back to the source language at runtime.

[ Export anyway ]   [ Cancel ]
```

5. If the game creator chooses "Export anyway" (or no warnings existed), the renderer sends a second `ExportGameDefinition` request with `{ "force": true }` to proceed past the warning gate. The sidecar serialises the game definition and returns the artefacts.
6. Renderer sends artefacts to the main process for writing to the output directory.

**"Cancel"** returns the game creator to the editor with no side-effects. No preference or state is persisted.

**No persistent preference.** The dialog appears every time a partial-translation export is attempted. There is no "don't ask again" option. This is intentional: a game creator who routinely exports mid-translation during development will see the dialog frequently, but the friction is deliberate — it prevents a "always export anyway" setting from being forgotten and silently shipping an incomplete localisation.

---

**Missing-string fallback in the export.**

When a key is missing for locale X, the exported locale file for X omits that key entirely. At runtime, `TextRenderer`'s template resolution order (locale → `TextTemplate` → structural) means the engine automatically falls back to `TextTemplate` (the source-language text). The behaviour is correct by construction — no special handling is needed in the export or the engine. The dialog's explanatory line ("Missing strings will fall back to the source language at runtime") communicates this to the game creator without requiring them to understand the renderer internals.

---

**Scope boundary: what the export dialog does not contain.**

The export dialog contains only the missing-translation summary. It does not show:
- Hard error counts or descriptions (those block export before the dialog is reached).
- Warning counts for non-translation issues (those belong in the problems panel).
- A link or shortcut to the problems panel.

The rationale for this narrow scope: the dialog appears only when the definition is otherwise export-ready. The game creator has already navigated through the "zero errors" gate. Surfacing unrelated diagnostic information at this moment would be confusing — the game creator needs to make one decision (export now or go fix translations) with full context on that one question.

General error and warning status is always visible in the persistent status bar at the bottom of the tool window (format: `✗ 0 errors  ⚠ 15 warnings`). The status bar is the correct location for at-a-glance health information; the export dialog is a decision prompt, not a status dashboard.

---

**Rationale:**
- Warning-not-error classification for missing translations: the game creator may intentionally ship with a partial translation (e.g. a beta with English and French only). Classifying missing strings as errors and forcing resolution would block export for any game that supports multiple locales during development. Warnings communicate that something is incomplete without asserting that it is wrong.
- No persistent "export anyway" preference: the cost of showing the dialog is one click per export session. The cost of a forgotten preference is shipping an unintentionally incomplete localisation. The asymmetry favours the dialog.
- `{ "force": true }` on the second export request: the sidecar maintains the warning gate internally; the renderer cannot instruct it to skip validation entirely — it can only confirm that the game creator has acknowledged the warnings. This keeps the validation logic in the sidecar (where it belongs) while giving the renderer control over the user flow.

**Consequences:**
- `ExportGameDefinition` sidecar response gains a `missingTranslations` field (present and non-empty when warnings exist and `force` was not set). When `force: true` is included in the request, `missingTranslations` is omitted and the full export artefact is returned.
- The renderer gains an export modal component. It is the only modal in the export flow — other error conditions (hard errors, bad output directory) surface as inline banners, not modals.
- The status bar component (always visible) displays current `globalErrorCount` and `globalWarningCount`. These values are kept up-to-date from mutation response payloads (D28) and refreshed on `GetAllDiagnostics` calls. The status bar is not part of D31's scope but is noted here as the correct location for general health information.

---

## Open Items

- [x] Language and runtime — D1
- [x] Keyword representation — D2
- [x] Effect block execution model — D3
- [x] Event log structure — D4
- [x] Contribution tracking — D5
- [x] Static effect lifecycle management — D6
- [x] State-based rule runner — D7
- [x] Trigger resolution — D8
- [x] Randomness — D9
- [x] Card visibility — D10 (deliberate non-decision)
- [x] Text rendering pipeline — D11
- [x] Runtime atom creation — D12
- [x] Keyword parameter modifications — D13
- [x] Game creator API — D14
- [x] Module boundaries — D15
- [x] Testing strategy — D16
- [x] Save/load (`GameStateSnapshot`) — D17
- [x] Keyword cross-references in card text — D18
- [x] `ComputeAvailableActions` contract — D19
- [x] `CostDef` type and extended `assert` built-in — D20
- [x] Combined cost block validation via state clone — D21
- [x] `ValidateActionArgs` callback placement — D22
- [x] Cost execution sequencing at action time — D23
- [x] `ComputeAvailableActions` ownership filter removal — D24
- [x] Breaking changes catalogue for action-args-and-cost-model — D25
- [x] Authoring tool platform and process architecture — D26
- [x] Tooling data layer and project file format — D27
- [x] Tooling validation approach (trigger model, debounce, sidecar protocol surface) — D28
- [x] D14 addendum — InitManifest mandatory, HostManifest append-only, LocalId uniqueness — D29
- [x] Godot export pipeline (signal derivation rules, export package format, GDScript generation) — D30
- [x] Missing-translation export gate UX — D31
