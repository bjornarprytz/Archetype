# Implementation Status

> Last updated: 2026-03-13 (Group 5 complete — Main process sidecar lifecycle + IPC bridge: sidecarManager.ts, ipcHandlers.ts, fileHandlers.ts, autosave.ts; main.ts updated to wire all; 27 new tests; 32 TypeScript tests total passing)
> Branch: `impl/text-renderer`
> All source in `src/` (5 assemblies) + `tests/Archetype.Tests/`. Electron tooling in `tooling/`.

---

## Assembly Map

| Assembly | Role | Status |
|---|---|---|
| `Archetype.Core` | Immutable data types, interfaces, `BuiltInKeywords` registry | ✅ Complete (Tier 1 subset) |
| `Archetype.Build` | `Kw` factory shorthands for authoring effect blocks | ✅ Complete |
| `Archetype.Engine` | Runtime executor, `GameState`, `EventLog`, `LifetimeChecker`, `TriggerResolver`, `ActionResolver`, `GameSession`, `GameSessionBuilder` | ✅ Tier 1–4 complete |
| `Archetype.Text` | Card text renderer | ✅ Complete (Tier 4) |
| `Archetype.Tooling.Server` | JSON-RPC sidecar for Electron authoring tool — `ProjectState`, DSL parser, reference graph, validator, 18 RPC handlers, export pipeline | ✅ Groups 3–4 complete |
| `tooling/` (Electron) | Desktop authoring tool — Electron main process, preload contextBridge, React renderer, Zustand stores | ✅ Groups 2 + 5 complete (Groups 6–7 pending) |

---

## Tier 1 — Core Types

### `KeywordNode` tree ✅
- `ParameterRef`, `Literal`, `Invocation` — immutable records in `Archetype.Core/Keywords.cs`
- `KeywordDefinition`: `Name`, `Parameters` (`ParameterDecl[]`), `ReturnType`, `Description`, `Body`, `PrimitiveSentinel`, `TextTemplate`
- `ParameterDecl` with `TypeName` enum (Atom, Card, Zone, Player, Session, Number, Boolean, ConditionName, PropertyName, ContributionId, Lifetime, EffectBlock, EventRef)
- `EffectBlockDef` / `EffectBlockStep` for block-level composition

### `BuiltInKeywords` registry ✅
- 35 entries: all mutation primitives, property queries, arithmetic, logic, `move-card`, `event-arg`, `declare-winner`, `declare-draw`, `player-by-name`, `get-atoms-in-zone`, `assert` (D20)
- `assert(condition: Boolean, on_fail: Number, notify: Number) → void` — never appends to event log; IsCostBody-aware panic/off override
- `BuiltInKeywords.All` list enforces sync invariant with Engine dispatch

### `EventLog` and `GameEvent` ✅
- `GameEvent` in `Archetype.Core/Events.cs`: `KeywordName`, `BoundArgs`, `SequenceNumber`, `Children` (tree structure), `SelfAndDescendants()` recursive traversal
- `EventRef` in `Archetype.Core/Events.cs`: first-class runtime value wrapping a `GameEvent`; used as `trigger_event` binding in triggered blocks
- `EventLog` in `Archetype.Engine/EventLog.cs`: append-only, frame-stack scope hierarchy (`OpenBlock`/`CloseBlock`, `OpenAction`/`CloseAction`, `OpenTurn`/`CloseTurn`); exposes `ThisBlock`, `ThisAction`, `ThisTurn`, `ThisGame` (all recursive via `SelfAndDescendants`)
- Composite parent stack (`PushCompositeParent`/`PopCompositeParent`): nests child events inside composite keyword wrappers

### `GameState` ✅
- `AtomSnapshot` (internal): mutable runtime atom state — `Kind`, `ZoneId`, `OwnerId`, `Accumulators`, `Modifiers`, `ConditionIndex`
- `GameState` (internal): atom registry, `NextContributionId`, `NextStaticEffectId`, `ActiveStaticEffects`, `DormantDeclarativeEffects`; implements `IGameStateReadable` (public interface in Core)

### Contribution types ✅
- `ContributionId`, `StaticEffectId`, `ModifierContribution`, `ConditionContribution`
- `LifetimeSpec` with `TurnTimer`, `TriggerCount`, `WhileCondition` lifetime conditions
- `StaticEffectDef`, `TriggerDefinition`, `TriggerScope`, `EventParamDecl`, `EventBinding`, `StaticEffect`, `DormantDeclarativeEffect`

### `GameDefinition` ✅
- `CardDefinition`, `ZoneDefinition`, `PhaseDefinition`, `ActionRuleDefinition`, `StateBasedRule`, `CardSet`, `PlayerDefinition`, `GameDefinition`
- `CardDefinition.ActivationCondition: KeywordNode?` (D19) — optional pure condition evaluated before offering PlayCard; card atom injected as `source`
- `CardDefinition.Cost: IReadOnlyList<CostDef>?` (D20) — cost definitions; empty = no cost
- `GameDefinition.PlayableZoneNames: IReadOnlyList<string>?` (D19) — zone definition names from which cards may be played; `null` = no zone filter
- `CostDef` record (D20) — `Body: EffectBlockDef`, `Parameters: ParameterDecl[]`, `TextTemplate: string?`; no separate EvaluationFunction
- `NamedEffectBlockDef.Cost` changed from `EffectBlockDef?` to `IReadOnlyList<CostDef>?` (D20/D25 breaking change)
- `ValidationResult` record (D21/D22) — `IsValid: bool`, `CostTexts: IReadOnlyList<string>`, static `Empty` and `Invalid` factories
- `OnFail` enum (`Continue | Stop | Panic`) and `NotifyFlag` enum (`On | Off`) — inline-literal-only (D20)
- `DiagnosticKind` enum and `DiagnosticEvent` record (D25)
- **D29**: `InitManifest` renamed from `DefaultInitManifest`; now non-nullable on `GameDefinition`; `InitManifest.Empty` static property; `CardSpec.LocalId: string?` optional field

### `HostManifest` ✅ (D29)
- `HostManifest(Zones, Cards, StateOverrides)` in `Archetype.Core/HostManifest.cs` — session-time append+patch layer
- `AtomStateOverride(Target, Accumulators?, Conditions?)` — patches atom state after provisioning
- `OverrideTarget` discriminated union: `ZoneTarget(LocalId)`, `CardTarget(LocalId)`, `PlayerTarget(PlayerName)`
- `SessionException` in `Archetype.Core/Exceptions.cs` — session-build-time contract violations (distinct from `DefinitionException`)

### Interfaces ✅
- `IPlayerStrategy`, `IRandomSource`, `IEngineObserver` (`OnTurnStart` + `OnTriggerCascadeAsync` → `CascadeDirective` + `OnDiagnostic(DiagnosticEvent)` D25)
- Player action types: `PlayCard` (+ `CostChoices`, `Targets`), `ActivateAbility` (+ `CostChoices`, `Targets`), `Pass`
- `AvailableActions` now carries `ValidateActionArgs: Func<PlayerAction, ValidationResult>` (D22)
- Prompt types: `PromptContext`, `PromptResponse`, `ChoicePrompt`, `TriggerOrderPrompt`
- `GameStateView` (public read-only view), `IGameStateReadable`
- **D30**: `GameStateView.LastActionEvents: IReadOnlyList<GameEvent>` — events from most recently completed `ResolveAction` (including all recursive descendants); `SetLastActionEvents` called by engine after each action
- **D30**: `EventLog.LastActionEvents` — captured in `CloseAction()` before accumulator is cleared; exposes recursive `SelfAndDescendants()` flat list

---

## Tier 2 — Execution

### `ExecutionContext` ✅
- Internal; carries `GameState`, `EventLog`, `Bindings`, `Strategies`, `RandomSource`, `Definition`, `ActivePlayerName`
- `CreateChildActionContext(Dictionary<string,object> extraBindings)` for trigger firing
- `TriggerEvaluationContext` removed (was dead code; replaced by plain `Dictionary<string,object>` with `"source"` binding)

### `BlockExecutor` ✅
- `ExecuteBlock(EffectBlockDef, ExecutionContext) → Task`
- Manages block-scope open/close on `EventLog`
- Dispatches each step through `KeywordEvaluator`
- `EvaluateCondition(KeywordNode, GameState, Dictionary<string,object>) → bool` — pure-mode evaluation for while-conditions and trigger conditions

### `KeywordEvaluator` ✅
- `EvaluateNode` — resolves `ParameterRef`, `Literal`, `Invocation` recursively
- `EvaluateInvocation` — dispatches through `MutationDispatch` or `PropertyDispatch`
- `EvaluateComposite` — expands `KeywordDefinition.Body` with parameter bindings; uses composite parent stack so child events nest correctly
- `ApplyParameterModifications` — applies active `ParameterModification` static effects

### Built-in dispatch ✅
- `MutationDispatch` / `PropertyDispatch` in `Archetype.Engine/Dispatch.cs`
- `BuiltInHandlers` in `Archetype.Engine/BuiltInHandlers.cs`: all 30 built-ins registered
- **`move-card`** handler: validates card/zone kinds, captures `origin`, updates `card.ZoneId`, logs event
- **`event-arg`** handler: extracts a named arg from an `EventRef`; used in trigger-fired blocks
- `AssertSync()` startup check: every `BuiltInKeywords.All` name has a handler; no extra handlers

### `Kw` factory ✅
- `Archetype.Build/Kw.cs`: typed shorthand for every built-in keyword
- `Kw.MoveCard(card, destination)`, `Kw.EventArg(ev, name)` added

### `PromptChannel` ✅ Contract complete — integration deferred
- `IPlayerStrategy`-based prompt dispatch wired in `ExecutionContext`
- `TaskCompletionSource<T>` suspension/resume mechanics (D3) are correct by construction: `RespondToPromptAsync` is awaited on the single game thread with no blocking calls; the pattern is structurally identical to every other `await` in the engine
- End-to-end suspension testing (a strategy that truly suspends mid-block) is a host integration concern — it requires a Godot-side `IPlayerStrategy` implementation. **Ratified as a known deferred gap** (2026-03-06): the engine contract is specified and wired; host integration tests own the proof of the suspension round-trip.

---

## Tier 3 — Rules Engine

### `TriggerResolver` ✅
- `TriggerFiring` record: `(StaticEffect Effect, GameEvent Event)`
- `CollectSatisfiedTriggers(GameState, EventLog)`: high-water-mark scan, condition eval, advances mark past ALL candidates (matched or not)
- `BuildEventParamBindings`: always includes `["source"] = ownerAtom` so trigger conditions can reference `ParameterRef("source")` (D8/D13 addendum); also maps game-creator `EventParams`
- `OrderTriggerFirings(firings, TriggerResolutionOrder)`: `OldestFirst` (StaticEffectId ASC, seq ASC), `OldestLast` (desc), `PromptPlayer` falls back to `OldestFirst`
- `FireTrigger(TriggerFiring, ExecutionContext, currentTurn)`: populates `trigger_event` + `EventBindings`, increments `TriggerFireCount` **before** `ExecuteBlock` (D8), manages own action scope, calls `CheckLifetimes`

### `ActionResolver` ✅
- `ResolveAction(EffectBlockDef? primaryBlock, GameState, EventLog, string playerName, int turn)`: full D7 post-action sequence — primary block → CheckLifetimes → RunStateBasedRules → cascade loop (observer check → collect → fire each → CheckLifetimes → RunStateBasedRules → repeat)
- `RunStateBasedRules`: fixpoint loop — all conditions snapshotted (`.ToList()`) before any blocks fire in a pass; each SBR block runs in its own action scope to prevent `OpenAction` from discarding uncommitted events
- `ActionResolver.Create(strategies, random, def, observer?)`: convenience factory that creates two `BlockExecutor` instances (one shared with `TriggerResolver`, one for primary/SBR blocks); `AssertSync` runs twice per `Create` call
- `IEngineObserver.OnTriggerCascadeAsync` wired; `null` → always `Continue`

### `LifetimeChecker` ✅
- Two-phase `CheckLifetimes(GameState, int currentTurn)`:
  - **Phase 1**: expire active effects whose `TurnTimer`, `TriggerCount`, or `WhileCondition` are satisfied
  - **Phase 2**: re-activate dormant declarative effects whose `WhileCondition` is now true
- `InstantiateStaticEffect` helper used by `GameSession` provisioning
- `ProvisionDeclarativeEffect(StaticEffectDef, AtomId, GameState)`: evaluates while-condition; activates immediately if absent/true, adds to `DormantDeclarativeEffects` if false

### Static effect lifecycle manager ✅
- Active→Dormant and Dormant→Active transitions in `LifetimeChecker`
- `TriggerCount` expiry: `TriggerFireCount` incremented by `TriggerResolver.FireTrigger` before block execution; next `CheckLifetimes` call expires the effect

---

## Tier 4 — API Surface

### `GameSessionBuilder` ✅
- Fluent builder in `Archetype.Engine/GameSession.cs`
- `WithPlayerStrategy(name, strategy)` — registers one strategy per defined player; validated at `Build()` time
- `WithRandomSource(source)` — required; `Build()` throws if absent
- `WithObserver(observer)` — optional cascade observer
- `WithHostManifest(HostManifest)` (D29) — session-time append layer; mutually exclusive with `FromSavedState`
- `FromSavedState(GameStateSnapshot snapshot)` — load path; `Build()` skips `WithRandomSource` requirement, validates `GameDefinitionId`, derives RNG from snapshot
- `Build()` — validates `GameDefinition.Id` non-empty; validates all players have strategies; no extra strategies for undefined players; D29 LocalId uniqueness validation (InitManifest zones/cards, HostManifest zones/cards, cross-manifest collision); CardTarget in `StateOverrides` must target InitManifest cards only

### `GameSession` ✅
- `static GameSessionBuilder Create(GameDefinition)` — factory entry point
- `async Task<GameResult> RunAsync(CancellationToken)` — provisions manifest, drives the phase/turn loop
- **Turn loop**: advances `turn-number` and `phase-index` accumulators; each phase runs Init → ActionWindow → Cleanup
- **`GameIsOver` propagation**: checks `_state.GameIsOver` after every `ResolveAction` call; also short-circuits mid-cascade in `ActionResolver`
- **Round-robin active player**: `(turn-1) % playerCount` over `PlayerDefinitions` insertion order
- **`ProvisionSession()`**: creates session atom (turn-number=1, phase-index=0), player atoms, calls `ProvisionManifest(InitManifest)` then optionally `ProvisionHostManifest`
- **`ProvisionManifest(InitManifest, out Dictionary<string, AtomId>)`** (D29): zones (LocalId keyed) → cards (with `ProvisionDeclarativeEffect`, card LocalId keyed with `"card:{LocalId}"` prefix) → card overrides → player overrides
- **`ProvisionHostManifest(HostManifest, existingLocalIds)`** (D29): zones → cards (LocalId keyed `"host-card:{LocalId}"`) → `AtomStateOverride` application; `CardTarget` may only reference InitManifest cards; `PlayerTarget` resolved by player name
- **`ComputeAvailableActions`**: two independent passes — Pass 1 (PlayCard, zone-filtered + activation condition, no ownership predicate — D24); Pass 2 (abilities, all card atoms regardless of zone or owner — D24); `ValidateActionArgs` delegate captures state clone at compute time (D22)
- **`TranslatePlayerAction`**: `PlayCard` → definition's `PrimaryEffect`; `ActivateAbility` → named `AdditionalEffects` body; `Pass` → null
- **D30 `LastActionEvents`**: `RunActionWindowAsync` calls `stateView.SetLastActionEvents(_eventLog.LastActionEvents)` after each `ResolveAction` (both card play and pass paths)

### `get-atoms-in-zone` built-in ✅
- `get-atoms-in-zone(zone: Zone) → Collection` — pure read; returns all atoms whose `ZoneId` matches the given zone atom
- Used by `ComputeAvailableActions` (via equivalent internal `GetAllAtoms()` read) and composable in keyword trees
- `GetAllAtoms()` added to `GameState` to support the handler
- `BlockExecutor.EvaluateProperty(node, state, bindings?)` added (internal) for testing non-boolean primitive results

### `ComputeAvailableActions` ✅
- Zone filter (D19 step 2): checks zone definition name ∈ `PlayableZoneNames` **and** zone owner == active player; prevents cross-player zone exploitation
- Activation condition (D19): `source` injected manually per D13 (no `StaticEffect` wrapper on this path)
- Ability loop (D19 step 4): independent second pass over all owned cards, ignores zone membership entirely; zone restrictions expressed via ability's own `ActivationCondition`
- Tests: 9 tests covering get-atoms-in-zone (3), zone filter, activation condition, source injection, ability-in-non-playable-zone, zone-owner restriction, pass-always-present
- **Open gap (pre-existing)**: runtime-created atoms (`create-card`/`create-zone`) are invisible to `ComputeAvailableActions` — `_atomDefinitionNames` is only populated during `ProvisionManifest`
- **`CostValidator`**: combined-block validation against lightweight `GameState` clone (D21); `IsCostBody=true` enforces panic/off assert semantics; always resolves `CostTexts` regardless of outcome
- **Cost execution (D23)**: `ActionResolver.ResolveAction` accepts optional `costBlocks`; each cost body runs with `IsCostBody=true` before the primary block within the same action scope
- **`Kw.Assert`** (D20): `Kw.Assert(condition, onFail, notify)` shorthand; encodes `OnFail`/`NotifyFlag` as `double` literals
- **`Kw.OwnedByActivePlayer()`** (D24): expands to `EqualTo(OwnerOf(Param("source")), GetState(Session(), "active-player"))`; XML doc states `"active-player"` session state requirement

### `declare-winner` / `declare-draw` built-ins ✅
- Architecture gap: D14 says "repeat until state-based rule produces outcome" but didn't specify mechanism
- **Resolution**: added `declare-winner(player)` and `declare-draw()` as primitives (33 total built-ins)
- `GameState.DeclareOutcome(string?)`: first-call-wins; sets `GameIsOver` and `PendingWinner`
- `GameState.RegisterPlayerName` / `TryGetPlayerName` / `TryGetPlayerAtomByName`: bidirectional player name registry
- `RunStateBasedRules` exits immediately at loop top when `GameIsOver` is true (prevents infinite loop when an always-true SBR fired the terminal rule)
- Cascade loop breaks when `GameIsOver` is true; per-trigger `GameIsOver` check stops mid-batch firing

### `player-by-name` built-in ✅
- `player-by-name(name: PropertyName) → Player` — reverse-looks up a player atom from its registered name string
- The idiomatic way to pass a player reference to `declare-winner` from a static `KeywordNode` tree (where the atom ID is not known at definition time)
- Added to `BuiltInKeywords.All` (33 total), `BuiltInHandlers`, and `Kw.PlayerByName`
- End-to-end test `DeclareWinner_ViaPlayerByName_ReturnsCorrectWinnerName` asserts `GameResult.Winner == "p1"` (resolves PR #4 blocker)

### Text renderer ✅
- **`RenderNode` discriminated union** in `Archetype.Core/RenderNode.cs`:
  - `TextSpan(Text)` — leaf, literal string fragment
  - `CompositeNode(Summary, Body)` — keyword invocation; summary from locale/template, body always full recursive structural expansion
  - `SequenceNode(Items)` — ordered list of nodes (block steps, static effect parts)
  - `RulesRef(Key, DisplayText)` — D18 cross-reference leaf; host calls `Resolve` to expand
- **`TextRenderer`** in `Archetype.Text/TextRenderer.cs`:
  - `Render(KeywordNode, locale?, bindings?)` — definition-time (bindings=null) or invocation-time modes
  - `RenderBlock(EffectBlockDef, locale?, bindings?)` — always returns `SequenceNode` of one `RenderNode` per step (including single-step blocks — D11 API contract)
  - `RenderStaticEffect(StaticEffectDef, locale?, bindings?)` — contribution block + trigger + lifetime
  - `RenderLifetimeSpec(LifetimeSpec, locale?)` — uses reserved `engine.lifetime.*` keys with OR-separation
  - `Resolve(keywordName, locale?, bindings?)` — D18 link resolution; returns null for unknown keywords
  - `FlattenToText(RenderNode)` — public static helper for collapsing trees to strings
- **Template resolution order**: locale → `TextTemplate` → structural `"keyword(arg, arg)"` fallback
- **`TextTemplate` values** added to all 34 built-in keyword definitions in `BuiltInKeywords.cs`
- **Definition-time caching**: body renders cached per `(KeywordDefinition, locale)` via `ConditionalWeakTable`; invocation-time not cached
- **D18 cross-reference tag parsing**: `[display](key)` (long form) and `[key]` (short form) in template strings produce `RulesRef` leaf nodes
- **28 tests** covering: ParameterRef def/inv modes, Literal, Invocation+TextTemplate, structural fallback, locale override, cross-ref tags (both forms), RenderBlock single/multi (single now asserts `SequenceNode` with one item — D11 fix), RenderLifetimeSpec (all condition types + OR join + locale override + permanent), RenderStaticEffect (trigger + contribution block + non-permanent lifetime), Resolve (primitive/composite/unknown, with locale), caching (cached/uncached), dual-use invariant (D2 §1.1, Layer 2)

---

## Tier 5 — Authoring Tooling Sidecar (Groups 3–4)

### `Archetype.Tooling.Server` ✅

Console app (JSON-RPC over stdio, D26). Built as a self-contained single-file
executable for Electron resource bundling.

**DSL Parser** (`DslParser.cs`):
- `Parse(string) → ParseResult` — single expression (returns `KeywordNode?`)
- `ParseBlock(string) → BlockParseResult` — semicolon-separated block steps
- Tolerant of partial input (missing close-parens) for `GetCompletions` use
- Grammar: function-call syntax only; args are invocations, `ParameterRef`s, or literals

**Project State** (`ProjectState`, `*Entry` types):
- `KeywordEntry`, `CardEntry`, `ZoneEntry`, `PlayerEntry`, `CardSetEntry`, `PhaseEntry`, `ActionRuleEntry`, `StateBasedRuleEntry`, `InitManifestEntry`, `LocalizationState`
- Each entry carries raw DSL source + parsed `KeywordNode?` (null on parse error)
- `ProjectDiagnostic` with `EntryKind`, `EntryName`, `Severity`, `Message`, `DslRange?`
- `SignalBehaviour` enum (`Default`, `Suppress`, `ForceInclude`) on `KeywordEntry` for D30

**Loader/Serializer**:
- `ProjectFileLoader.Load(json)` — lenient; parse errors → diagnostics, never throws; calls `ReferenceGraph.Build` then `Validator.Validate`
- `ProjectFileSerializer.Serialize(state)` — writes DSL source strings; round-trips `EditorState` verbatim

**Reference Graph + Validator**:
- `ReferenceGraph.Build(state)` — rebuilds `state.UsedBy` (reverse-ref map) from all parsed trees
- `Validator.Validate(state)` — clears and repopulates `state.Diagnostics`; checks built-in name conflicts, unresolved keyword refs, missing translations (D31 → warnings only)

**18 RPC Handlers** (all in `Handlers/`):
- Mutation: `LoadProject`, `SaveProject`, `UpdateKeywordBody`, `UpdateCardEffect`, `UpdateField`, `UpdateLifetimeSpec`, `UpdateActivationCondition`, `UpdateCostBody`, `AddEntry`, `RemoveEntry`, `RenameEntry`
- Query: `GetAllDiagnostics`, `GetSymbolInfo`, `GetReferenceGraph`, `GetCompletions`
- Render: `RenderCardText`
- Export: `ExportGameDefinition`, `ExportGodotClasses`

All mutation handlers call `MutationHelpers.RevalidateAndBuildResponse` → returns scoped `MutationResponse` with `AffectedEntries`, `Diagnostics`, `GlobalErrorCount`, `GlobalWarningCount`.

**Export pipeline**:
- `GameDefinitionExporter.Export(state, force)` — hard-error gate → missing-translation gate (D31) → build `GameDefinition` → serialize JSON
- `GodotClassGenerator.Generate(state)` → `filename → content` map; `DeriveSignalSet` implements D30 signal inclusion rules
- Generates: `ArchetypeCard.gd`, `ArchetypeZone.gd`, `ArchetypePlayer.gd`, `ArchetypeSession.gd`, `ArchetypeCardImporter.gd`, `ArchetypeInterop.gd`
- `GameDefinitionJsonOptions` (in `Archetype.Core`) — `KeywordNodeConverter` + `StripKeywordNodePolymorphism` modifier; works around `[JsonDerivedType]` + `[JsonConverter]` incompatibility in .NET 8+ for full `GameDefinition` serialization

**D27/D28 bug fixes (2026-03-11)**:
- **Fix 1** — `KeywordEntry.ReturnType : TypeName?` added; `ProjectFileLoader` reads `"returnType"`; `GameDefinitionExporter` and `RenderCardTextHandler` use `entry.ReturnType ?? TypeName.Atom`; `Validator` emits error for null `ReturnType`; `LiteralConverter` promoted to `public`
- **Fix 2** — `ArtCropRegion : float[]?` (already in `CardEntry`); `ProjectFileSerializer` now serializes it; `ProjectFileLoader` reads it via `LoadArtCropRegion`
- **Fix 3** — `StaticEffectEntry` gains `LifetimeNode : LifetimeSpec?`, `TriggerEventKeyword`, `TriggerScope`; `LifetimeDsl` parser+serializer added; `ProjectFileLoader` parses all static effect fields; `GameDefinitionExporter` maps entries to real `StaticEffectDef` (not empty stub)
- **Fix 4** — `RenameEntryHandler.RewriteKeywordRefs` now also rewrites all `*Dsl` source strings across all entry kinds using token-boundary-aware `RewriteDsl`
- **Fix 5** — `ProjectFileSerializer.SerializeInitManifest` zone `"definition"` field corrected to `z.Definition` (was `z.LocalId`); `ProjectFileLoader` constructor args corrected to use named params
- **Fix 6** — `GetSymbolInfoHandler.referencedBy` already returns only `{ entryName }` (confirmed by test; no code change needed)

**Reviewer BLOCKER fixes (2026-03-11)**:
- **BLOCKER 1** — `UpdateLifetimeSpecHandler` now calls `LifetimeDsl.Parse` immediately after storing `LifetimeDsl`, populating `LifetimeNode` in-session so the exporter sees the updated spec without a save/reload cycle (D27). Invalid DSL records a diagnostic and leaves `LifetimeNode` null.
- **BLOCKER 2** — `RenameEntryHandler.RewriteKeywordRefs` extended to rewrite `PhaseEntry.InitDsl`/`CleanupDsl`, `ActionRuleEntry.BeforeDsl`/`AfterDsl`, and `StateBasedRuleEntry.ConditionDsl`/`BodyDsl` — previously these three entry types were silently skipped, leaving stale keyword references after a rename (D27).

**Remaining** (Groups 5–7): IPC bridge wiring to sidecar, UI panels, and packaging — TypeScript/React work outside the C# assemblies.

---

## Tier 5 — Electron Authoring Tool (Group 2)

### Project scaffold ✅ (`tooling/`)

**Configuration files:**
- `tooling/package.json` — Electron 30, TypeScript 5.4, React 18, Zustand 4, Vite 5, Vitest 1, `@testing-library/react`. Dev deps include `tsx` for main/preload dev run and `eslint` with TS + React plugins.
- `tooling/tsconfig.json` — base config; `strict: true`, `noUncheckedIndexedAccess: true`, `exactOptionalPropertyTypes: true`; `@shared/*` path alias.
- `tooling/tsconfig.main.json` — main process: CommonJS, `rootDir: src`, `outDir: dist`.
- `tooling/tsconfig.preload.json` — preload script: CommonJS (required for contextBridge), same root/out layout.
- `tooling/vite.config.ts` — renderer bundle; React plugin; `@shared` alias; `outDir: dist/renderer`; dev server on port 5173.
- `tooling/vitest.config.ts` — jsdom environment; `setupFiles: [setup.ts]`; includes `*.test.{ts,tsx}` only.
- `tooling/.eslintrc.json` — `@typescript-eslint/recommended-requiring-type-checking`; `no-explicit-any: error`.

**Main process (`src/main/`):**
- `main.ts` — app lifecycle; creates `BrowserWindow` via `windowManager`; macOS re-open handling; quit on all-windows-closed (non-macOS). Sidecar spawn deferred to Group 5.
- `windowManager.ts` — creates `BrowserWindow` with `contextIsolation: true`, `nodeIntegration: false`, `sandbox: true`. Loads Vite dev server URL in dev (via `VITE_DEV_SERVER_URL`) or `dist/renderer/index.html` in production.

**Preload (`src/preload/preload.ts`):**
- `contextBridge.exposeInMainWorld("archetype", ...)` exposing:
  - `invoke(channel, payload)` — wraps `ipcRenderer.invoke`; typed to `IpcChannel`.
  - `onNotification(channel, handler)` — wraps `ipcRenderer.on`; returns unsubscribe function.
- No raw Electron APIs leaked to renderer.

**Shared IPC contracts (`src/shared/ipc.ts`):**
- All 18 sidecar method channel names + 5 file I/O channel names as `IPC_CHANNELS` const object.
- Full request/response TypeScript interfaces for all 23 channels (strict, no `any`).
- `ArchetypeApi` interface — the shape of `window.archetype` (used by both preload and renderer).
- `Window` interface augmentation so renderer gets full type safety on `window.archetype`.

**Renderer (`src/renderer/`):**
- `index.tsx` — React 18 concurrent root mounting `<App />` into `#root`.
- `App.tsx` — shell layout: 200px sidebar nav + flex-grow main content + 28px status bar. Sidebar drives `activePanel` state; main area renders named `PanelPlaceholder` components (replaced by real panels in Group 6).
- `store/projectStore.ts` — Zustand store: `sidecarStatus`, `projectPath`, `projectName`, `isDirty`, `isMutating`. Actions: `setSidecarStatus`, `setProject`, `clearProject`, `markDirty`, `markClean`, `setMutating`.
- `store/diagnosticsStore.ts` — Zustand store: `globalErrorCount`, `globalWarningCount`, `diagnostics` (full list; null when not loaded). Actions: `updateCounts`, `setDiagnostics`, `clearDiagnostics`, `reset`.

**Tests:**
- `__tests__/setup.ts` — mocks `window.archetype` with `vi.fn()` stubs; imports `@testing-library/jest-dom`. Guarded with `typeof window !== "undefined"` so it loads safely in Node environment.
- `__tests__/App.test.tsx` — 5 tests: renders without crashing, sidebar present, default Keywords panel active, navigation to Cards panel, status bar present.

---

## Tier 5 — Electron Authoring Tool (Group 5)

### Sidecar lifecycle + IPC bridge ✅ (`tooling/src/main/`)

**`sidecarManager.ts` (task 5.1):**
- Resolves sidecar binary via `process.resourcesPath` (prod) or `src/Archetype.Tooling.Server/bin/Debug/net10.0/` (dev)
- Spawns sidecar via `child_process.spawn` with `stdio: pipe`
- Readline interface on stdout for newline-delimited JSON response parsing
- `Map<string, PendingEntry>` correlates requests to responses by `id`
- Crash handling: rejects all pending, attempts one restart; calls `_onError` if restart fails
- `startSidecar(onError)`, `invoke(method, params)`, `stopSidecar()` public API

**`fileHandlers.ts` (task 5.3):**
- `readFile(path)` — reads UTF-8 file; validates path inside allowed roots (home, userData, temp, documents, desktop)
- `writeFile(path, content)` — writes UTF-8 file; creates intermediate dirs; path validated
- `showOpenDialog(opts)` → `string | null` — wraps `dialog.showOpenDialog`; returns null on cancel
- `showSaveDialog(opts)` → `string | null` — wraps `dialog.showSaveDialog`; returns null on cancel
- `getUserDataPath()` → `string` — returns `app.getPath("userData")`
- Path validation: `path.isAbsolute` check + `path.normalize` + allowed-root prefix check prevents directory traversal

**`ipcHandlers.ts` (task 5.2):**
- `registerIpcHandlers(win)` — single registration entry point called from `main.ts`
- Mutation channels (`UpdateKeywordBody`, `AddEntry`, etc.) call `notifyMutation()` then forward to sidecar
- Non-mutating sidecar channels (`GetAllDiagnostics`, `RenderCardText`, etc.) forwarded directly
- File I/O channels (`File:ReadFile`, etc.) call fileHandlers implementations
- `SaveProject` not counted as a user mutation (does not reset autosave timer)

**`autosave.ts` (task 5.4):**
- 60-second inactivity timer; reset on every `notifyMutation()` call
- `performAutosave()`: calls sidecar `SaveProject` → `writeFile`; all errors swallowed silently
- Active only when `_projectPath` is set (`setProjectPath` / `clearProjectPath`)
- `shutdownAutosave()` for graceful app quit

**`main.ts` (updated):**
- Calls `registerIpcHandlers(mainWindow)` before renderer loads
- Calls `startSidecar(onError)` with callback that does `win.webContents.send("SidecarError", msg)`
- `before-quit` handler calls `shutdownAutosave()` + `stopSidecar()` for clean exit

**`src/shared/ipc.ts` (updated):**
- Added `ReadFileParams` and `WriteFileParams` typed interfaces for file I/O payloads
- Added `IPC_CHANNELS.SidecarError` notification channel constant

**`tsconfig.main.json` (updated):**
- Added `exclude` for `src/main/**/__tests__/**/*.ts` so test files are not compiled into the main process bundle

**`vitest.config.ts` (updated):**
- Added `src/main/**/*.test.ts` to `include`
- Added `environmentMatchGlobs: [["src/main/**/*.test.ts", "node"]]` so main tests run in Node, not jsdom

**Tests (27 new):**
- `src/main/__tests__/sidecarManager.test.ts` — 9 tests: request serialisation, id correlation, error envelope, concurrent requests, crash rejection, restart attempt, onError callback, `stopSidecar` stdin.end, malformed JSON tolerance
- `src/main/__tests__/fileHandlers.test.ts` — 12 tests: readFile success/traverse-rejection/relative-rejection/ENOENT, writeFile success/mkdir/rejection, showOpenDialog success/cancel, showSaveDialog success/cancel, getUserDataPath
- `src/main/__tests__/autosave.test.ts` — 6 tests: fires after 60s, resets on each mutation, no-op without project, cancelled on clearProjectPath, errors swallowed, shutdownAutosave cancels

---

## Tier 5 — Persistence

### D17 Save/Load ✅ PASS (BLOCKER 1 fixed 2026-03-08)

- **`GameDefinition.Id`**: required non-empty string; `GameSessionBuilder.Build()` throws `DefinitionException` if absent.
- **`IEngineObserver.OnTurnStart(int turnNumber, GameStateSnapshot snapshot)`**: called before each turn's first phase init block; host persists snapshot.
- **`SeededRandom`** (new): xoshiro128** implementation, independent of `System.Random`. Seeded via splitmix64. `Snapshot()` / `FromSnapshot(RngSnapshot)` for deterministic replay. `_callCount` tracks every raw `NextRaw()` invocation (including rejection-sampled values) for bit-for-bit replay.
- **`GameStateSnapshot`** type hierarchy in `Archetype.Core/Snapshot.cs`: `BoundValue` (7 subtypes), `RngSnapshot`, `StaticEffectDefRef`, `DormantEffectSnapshot`, `AtomSnapshotData`, `ContributionSnapshot` (2 subtypes), `StaticEffectSnapshot`, `GameStateSnapshot`.
- **`GameState.ToSnapshot()`**: captures atoms, contributions, active static effects (declarative by ref / dynamic inlined), dormant effects, player names, finalized log, RNG state.
- **`GameState.LoadFromSnapshot()`**: restores ID counters, atoms, player registries, player order, contributions (reconstructs `ModifierIndex`/`ConditionIndex` — Decision 7), active static effects, dormant effects.
- **`GameStateSnapshotSerializer`** in `Archetype.Engine`: `Serialize(GameStateSnapshot) → string` / `Deserialize(string) → GameStateSnapshot`. Uses `System.Text.Json` + `[JsonDerivedType]`. `GameEvent.BoundArgs` round-trips via `GameEventDto` + `BoundValue` union. `EventRefValue` resolved via sequence-number index. Custom `LiteralConverter` handles `Literal.Value : object` (tags: `d`/`b`/`s`/`atom`). Custom struct converters for `AtomId`, `ContributionId`, `StaticEffectId`.
- **`GameSessionBuilder.FromSavedState(GameStateSnapshot)`**: sets load path; `Build()` skips `WithRandomSource` requirement, validates `GameDefinitionId`, derives `SeededRandom.FromSnapshot(snapshot.Rng)`.
- **`GameSession.RunAsync`**: on load path, calls `LoadFromSnapshot` then starts loop at `snapshot.TurnNumber`; calls `OnTurnStart` before each turn's first phase init.
- **`[JsonDerivedType]` added** to: `ContributionSource`, `LifetimeCondition`, `KeywordNode`, `ParameterModification`.
- **`AtomIdCounter.PeekNext()/Restore(long)`** and **`ContributionIdCounter.PeekNext()/Restore(long)`** added for snapshot capture/restore.
- **`StaticEffect.CardDefinitionName`/`EffectIndex`** and **`DormantDeclarativeEffect.CardDefinitionName`/`EffectIndex`** stored at instantiation time so snapshot capture can produce `StaticEffectDefRef` without scanning definitions.
- **`LifetimeChecker.ProvisionDeclarativeEffect`/`InstantiateStaticEffect`** updated to accept optional `cardDefinitionName`/`effectIndex` and propagate through the effect lifecycle (`ActivatePass`, `Expire`).

---

## Test Coverage

| File | Tests | Status |
|---|---|---|
| `MoveCard/MoveCardLayer1Tests.cs` | 7 | ✅ All passing |
| `MoveCard/MoveCardLayer2Tests.cs` | 3 | ✅ All passing |
| `TriggerResolution/TriggerResolutionTests.cs` | 10 | ✅ All passing |
| `StateBasedRules/StateBasedRuleTests.cs` | 4 | ✅ All passing |
| `GameSession/GameSessionTests.cs` | 12 | ✅ All passing |
| `ComputeAvailableActions/ComputeAvailableActionsTests.cs` | 12 | ✅ All passing (3 new D24 tests: 9.13, 9.14, 9.15) |
| `TextRenderer/TextRendererTests.cs` | 28 | ✅ All passing |
| `SaveLoad/SaveLoadTests.cs` | 13 | ✅ All passing (T13 added for BLOCKER 1 regression) |
| `BuiltIns/AssertTests.cs` | 7 | ✅ All passing (new — D20 assert semantics) |
| `CostModel/CostModelTests.cs` | 7 | ✅ All passing (new — D21 cost validation + D23 sequencing) |
| `HostManifest/HostManifestTests.cs` | 9 | ✅ All passing (new — D29 HostManifest + InitManifest enforcement) |
| `GameSession/LastActionEventsTests.cs` | 4 | ✅ All passing (new — D30 LastActionEvents) |
| `Tooling/ProjectFileLoaderTests.cs` | 8 | ✅ All passing (3 new: returnType round-trip, ArtCropRegion round-trip, ZoneSpec definition/localId) |
| `Tooling/ValidatorTests.cs` | 5 | ✅ All passing (2 new: missing-ReturnType error, ReturnType-present no error) |
| `Tooling/RpcHandlerTests.cs` | 23 | ✅ All passing (14 new: export ReturnType, export static effect, rename DSL rewrite, rename round-trip, GetSymbolInfo shape; BLOCKER 1: UpdateLifetimeSpec 2 tests; BLOCKER 2: RenameEntry phase/actionRule/SBR rewrite 6 tests) |

| `tooling/src/renderer/__tests__/App.test.tsx` | 5 | ✅ All passing (new — Group 2 scaffold smoke test) |
| `tooling/src/main/__tests__/sidecarManager.test.ts` | 9 | ✅ All passing (new — Group 5 sidecar lifecycle) |
| `tooling/src/main/__tests__/fileHandlers.test.ts` | 12 | ✅ All passing (new — Group 5 file I/O channels) |
| `tooling/src/main/__tests__/autosave.test.ts` | 6 | ✅ All passing (new — Group 5 autosave timer) |

**Total: 150 C# tests passing + 32 TypeScript/React tests passing.**

### Layer 1 (unit, isolated state)
- `MoveCard_UpdatesCardZoneId_ToDestination`
- `MoveCard_LogsEvent_WithCorrectCardOriginAndDestination`
- `MoveCard_OriginReflectsZoneAtCallTime_NotAfterSubsequentMove`
- `MoveCard_SelfMove_CompletesWithoutError_AndLogsEvent`
- `MoveCard_InvalidDestination_NonExistentAtom_ThrowsEngineException`
- `MoveCard_CardArgIsZone_ThrowsEngineException`
- `MoveCard_DestinationIsCard_NotZone_ThrowsEngineException`

### Layer 2 (block integration)
- `CheckLifetimes_ExpiresActiveEffect_WhenCardLeavesWhileConditionZone`
- `CheckLifetimes_ActivatesDormantEffect_WhenCardEntersWhileConditionZone`
- `CompositeKeyword_CallingMoveCard_ProducesNestedEventTree_AndTriggerFires`

### GameSession end-to-end (Layer 3)
- `Builder_ThrowsWhenRandomSourceMissing`
- `Builder_ThrowsWhenPlayerStrategyMissing`
- `Builder_ThrowsForUnknownPlayerName`
- `DeclareWinner_InPhaseInit_ReturnsResultWithCorrectWinner` — SBR fires draw on turn 1
- `DeclareDraw_ViaSBR_ReturnsDrawResult` — always-true SBR with declare-draw; validates GameIsOver terminates the loop
- `FinalLog_ContainsEvents_FromWinningTurn` — phase Init block runs, SBR fires draw; events present in FinalLog
- `Manifest_ProvisionesPlayerAndZones_GameStateHasCorrectAtoms` — zone + player state provisioning; SBR ends game
- `DeclareWinner_PlayerNameResolvedCorrectly` — card provisioning + manifest + declare-draw
- `MultiTurn_GameRunsTwoTurns_BeforeDeclareDraw` — SBR condition on turn-number ≥ 2; player queues 2 passes
- `PlayCard_Action_ExecutesPrimaryEffect` — LambdaPlayerStrategy plays card; SBR fires when score ≥ 1; event logged
- `DeclareWinner_ViaPlayerByName_ReturnsCorrectWinnerName`

### Trigger resolution (full D7/D8 lifecycle)
- `TriggerFires_WhenEventMatchesKeywordAndNoCondition`
- `TriggerDoesNotFire_WhenConditionIsFalse`
- `TriggerDoesNotFire_WhenNoMatchingEvents`
- `HighWaterMark_PreventsTriggerDoubleFiring`
- `TriggerChain_SecondTriggerFiresInNextBatch`
- `TriggerFiring_FireCount_CausesExpiry_ViaTriggerCount`
- `OldestFirst_OrdersMultipleTriggers`
- `TriggerEvent_Binding_IsAvailableInFiredBlock` *(rewritten: fired block calls `event-arg` and uses AtomId as modify-accumulator target)*
- `NullObserver_DoesNotHaltCascade`
- `TriggerCondition_CanReferenceSourceBinding` *(new: verifies `["source"]` binding in condition evaluation)*

### State-based rules (fixpoint loop, D7)
- `SBR_Fires_WhenConditionIsTrue`
- `SBR_DoesNotFire_WhenConditionIsFalse`
- `SBR_FixpointLoop_SBR2_FiresInSecondPass`
- `SBR_AllConditionsSnapshotted_BeforeAnyBlockFires`

---

## Open Gaps / Known Issues

1. **`PromptChannel` untested** — the `IPlayerStrategy` prompt dispatch exists but no test exercises a mid-block prompt (`choose`, target selection).
2. **Text renderer** — `Archetype.Text` project is scaffolded; no implementation.
3. **`ComputeAvailableActions` cost pre-flight deferred** — zone filtering and activation-condition evaluation are implemented (D19). Cost pre-flight (checking whether a card's cost can be paid) remains deferred per the D19 design doc; no current game definition requires it.
4. **`declare-winner` architecture gap** — D14 didn't specify how a game-ending primitive signals `GameSession`. Resolved by adding `declare-winner(player)` / `declare-draw()` built-ins; decision documented here and in source XML docs. Architecture doc should be updated to ratify this (open item for architect).
5. **Runtime-created atoms invisible to `ComputeAvailableActions`** — `_atomDefinitionNames` is populated only during `ProvisionManifest`; cards/zones created at runtime via `create-card`/`create-zone` would be silently skipped. Pre-existing gap, not introduced by D19.

---

## Blocked Modules

| Module | Blocked By |
|---|---|
| (none) | — |

---

## Resolved Issues

### action-args-and-cost-model (D20–D25) — 102 tests — review verdict: PASS (2026-03-09)

All ten reviewer checks (10.1–10.10) passed with no blockers. Two minor issues fixed directly by reviewer:
- `CloneForValidation` XML doc inaccurately listed "Player name registry, session atom" as excluded; corrected to "Also copied (required for cost-body keyword resolution)" (`GameState.cs`).
- Dead variable `sessionEnergyBefore` removed from `ComputeAvailableActionsTests.cs:512` (CS0219 warning eliminated).

No functional changes. 102/102 tests pass with zero compiler warnings.

### SaveLoad tests (D17) — 13 tests — review verdict: PASS (2026-03-08)
- `SeededRandom_SameSeed_ProducesSameSequence`
- `SeededRandom_FastForward_ProducesCorrectNextValue`
- `Snapshot_RoundTrip_PreservesAtomState`
- `Snapshot_RoundTrip_PreservesActiveStaticEffects`
- `Snapshot_RoundTrip_BoundArgs_AtomIdPreservesType`
- `Snapshot_RoundTrip_BoundArgs_EventRefResolvesCorrectly`
- `FromSavedState_ResumesAtCorrectTurn`
- `FromSavedState_GameDefinitionIdMismatch_ThrowsDefinitionException`
- `FromSavedState_DoesNotRequireWithRandomSource`
- `OnTurnStart_CalledBeforeFirstPhaseInit`
- `GameDefinitionBuilder_Build_ThrowsWhenIdMissing`
- `ModifierIndex_ReconstructedCorrectly_AfterLoad`
- `ManifestProvisionedCondition_SurvivesSnapshotRoundTrip` (T13 — BLOCKER 1 regression test)

### Text renderer review — PASS (2026-03-07)
- ✅ BLOCKER 1 resolved (`1a49699`): `RenderBlock` now always returns `SequenceNode` (D11 API contract)
- ✅ MINOR 1 resolved (fixed directly in initial review): `RegexOptions.Compiled` removed (D1 WASM constraint)
- ✅ Three additional tests added: T8h (StateContributionBlock), T8i (non-permanent lifetime), T9d (Resolve with locale)
- ✅ 73/73 tests passing. Review verdict: **PASS**

### Tier 4 — GameSession / GameSessionBuilder (2026-03-05) — rework complete
- ✅ `GameSessionBuilder`: fluent builder with full player-strategy validation
- ✅ `GameSession.RunAsync`: turn/phase loop, `GameIsOver` propagation, manifest provisioning
- ✅ `declare-winner` / `declare-draw` end-to-end verified; `GameResult.Winner == "p1"` asserted
- ✅ `player-by-name(name)` primitive added — resolves the "how to pass a player atom to declare-winner" gap
- ✅ `LifetimeChecker.ProvisionDeclarativeEffect`: while-condition-aware provisioning used by manifest
- ✅ `RunStateBasedRules` exits early on `GameIsOver` (prevents infinite loop from always-true terminal SBRs)
- ✅ Cascade loop and per-trigger checks also break on `GameIsOver`
- ✅ `Kw.DeclareWinner` / `Kw.DeclareDraw` / `Kw.PlayerByName` shorthands added to `Archetype.Build`
- ✅ `GameStateView` constructor made `public` (was `internal`; needed cross-assembly from Engine)
- ✅ 36/36 tests passing

### implement-trigger-resolver rework (2026-03-04)
- ✅ **BLOCKER resolved**: `RunStateBasedRules` now has 4 tests covering all D7 fixpoint invariants
- ✅ **MINOR.1 resolved**: `source` binding always populated in `BuildEventParamBindings`; `TriggerEvaluationContext` removed
- ✅ **MINOR.2 resolved**: Test 6.8 now calls `event-arg(trigger_event, "card")` and uses the result as modify-accumulator's atom arg
- ✅ **MINOR.3 resolved**: `ActionResolver.Create` doc comment correctly describes two-executor behaviour

### implement-trigger-resolver (2026-03-04)
- ✅ `TriggerResolver` (`CollectSatisfiedTriggers`, `OrderTriggerFirings`, `FireTrigger`)
- ✅ `ActionResolver` (`ResolveAction`, `RunStateBasedRules`)
- ✅ `event-arg` built-in + `EventRef` type
- ✅ `GameEvent.SelfAndDescendants()` — recursive event tree traversal
- ✅ Composite parent stack in `EventLog` — child events nest, not duplicated
- ✅ Layer 2 test 6.7 uses real `ActionResolver`

### add-move-card-primitive (2026-03-04)
- ✅ `MoveCard` uses `RequireAtomOfKind` for consistent validation
- ✅ Test covers Zone-as-card guard (`MoveCard_CardArgIsZone_ThrowsEngineException`)
- ✅ Test 6.7b asserts `draw-card` has exactly one child (`move-card`)
- ✅ `EvaluateComposite` uses push/pop composite stack (no event duplication)
