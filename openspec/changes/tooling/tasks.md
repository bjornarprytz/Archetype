---
status: in-progress
owner: implementer
last-updated: 2026-03-11
depends-on:
  - docs/architecture.md
  - openspec/changes/tooling/requirements.md
  - docs/implementation-status.md
---

# Tooling Change — Task List

Covers D26–D31 plus the D29 engine breaking changes. Tasks are ordered by
dependency: engine changes land first, then the sidecar, then the Electron
shell. All tasks in a numbered group may proceed independently of other groups
at the same level unless a dependency is called out explicitly.

---

## Group 0 — Engine breaking changes (D29) ✅ COMPLETE

These tasks modify existing C# source and tests. They must land before the
sidecar is scaffolded, because the sidecar will reference the updated Core
types.

### 0.1  Rename `DefaultInitManifest` → `InitManifest` on `GameDefinition` ✅
  - reads: `docs/architecture.md#D29`, `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Core/GameDefinition.cs`
  - Make `GameDefinition.InitManifest : InitManifest` non-nullable (was
    `DefaultInitManifest : InitManifest?`). Update constructor positional
    parameter accordingly. Update all `GameDefinition` construction sites
    (tests, `GameDefinitionBuilder`, JSON loader if it exists).

### 0.2  Add `CardSpec.LocalId : string?` ✅
  - reads: `docs/architecture.md#D29`, `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Core/GameDefinition.cs`
  - Add optional `LocalId` property to `CardSpec`. Null by default; existing
    callers are backward-compatible.

### 0.3  Add `HostManifest`, `AtomStateOverride`, `OverrideTarget` types ✅
  - reads: `docs/architecture.md#D29`, `src/Archetype.Core/GameDefinition.cs`
  - writes: `src/Archetype.Core/HostManifest.cs` (new file)
  - New record `HostManifest { Zones, Cards, StateOverrides }` using the same
    `ZoneSpec`/`CardSpec` shapes. New discriminated union `OverrideTarget`
    (`ZoneTarget`, `CardTarget`, `PlayerTarget`). New record `AtomStateOverride
    { Target, Accumulators?, Conditions? }`.

### 0.4  `GameDefinitionBuilder` — enforce mandatory `InitManifest` ✅ (enforced at type level)
  - reads: `docs/architecture.md#D29`, `src/Archetype.Build/`
  - writes: `src/Archetype.Build/GameDefinitionBuilder.cs`
  - `Build()` must throw `DefinitionException` if `InitManifest` was not set.
    Rename the method that sets it from `WithDefaultInitManifest` (or
    `UseDefaultInit`) to `WithInitManifest(Action<ManifestBuilder>)` (this
    call is now required). Validate unique zone `LocalId`s within
    `InitManifest.Zones`; throw `DefinitionException` on collision.
    Validate unique non-null card `LocalId`s within `InitManifest.Cards`.

### 0.5  `GameSessionBuilder` — remove manifest-choice methods, add `HostManifest` ✅
  - reads: `docs/architecture.md#D29`, `src/Archetype.Engine/GameSession.cs`
  - writes: `src/Archetype.Engine/GameSession.cs`
  - Remove: `.UseDefaultInit()`, `.WithInitManifest(InitManifest)`,
    `.WithInitManifest(Action<ManifestBuilder>)`.
  - Add: `.WithHostManifest(HostManifest)`, `.WithHostManifest(Action<HostManifestBuilder>)`.
  - `Build()` validates: (a) `HostManifest` zone `LocalId`s do not collide with
    `InitManifest` zone `LocalId`s, (b) no duplicate zone `LocalId` within
    `HostManifest.Zones`, (c) no duplicate non-null card `LocalId` within
    `HostManifest.Cards`, (d) `WithHostManifest` and `FromSavedState` are
    mutually exclusive.

### 0.6  Add `HostManifestBuilder` to `Archetype.Build` ✅
  - reads: `docs/architecture.md#D29`, `src/Archetype.Build/`
  - writes: `src/Archetype.Build/HostManifestBuilder.cs` (new file)
  - Fluent builder with `AddZone`, `AddCard`, `OverrideZoneState`,
    `OverrideCardState`, `OverridePlayerState` per the D29 API table.
    `StateOverrideBuilder` nested type with `SetAccumulator` /
    `AddCondition` methods.

### 0.7  Extend `GameSession.ProvisionManifest` for 9-step sequence (D29) ✅
  - reads: `docs/architecture.md#D29`, `src/Archetype.Engine/GameSession.cs`
  - writes: `src/Archetype.Engine/GameSession.cs`
  - Extend provisioning from 6 steps to 9. Steps 7–9 apply `HostManifest`:
    create host zones, create host cards (resolving `ZoneLocalId` across
    both manifests), apply `AtomStateOverride` patches (accumulator merge,
    condition append). `LocalId → AtomId` map must persist through all 9
    steps. Validate `AtomStateOverride` targets against `InitManifest`-only
    atoms (D29 §5).

### 0.8  D29 engine tests ✅
  - reads: `docs/architecture.md#D29`, `tests/Archetype.Tests/GameSession/`
  - writes: `tests/Archetype.Tests/HostManifest/HostManifestTests.cs` (new file)
  - Tests:
    - `InitManifest_Required_BuildThrows_WhenAbsent` — `GameDefinitionBuilder.Build()` throws without `WithInitManifest`
    - `CardSpec_LocalId_Optional_NullByDefault` — existing `CardSpec` construction
    - `ZoneLocalId_Duplicate_InInitManifest_ThrowsDefinitionException`
    - `ZoneLocalId_Duplicate_AcrossManifests_ThrowsSessionException`
    - `HostManifest_ZonesAndCards_Provisioned_AfterInitManifest`
    - `HostManifest_StateOverride_AccumulatorMerge_LeavesOthersIntact`
    - `HostManifest_StateOverride_ConditionAppend_DoesNotDuplicate`
    - `HostManifest_StateOverride_TargetsHostAtom_ThrowsSessionException`
    - `HostManifest_And_FromSavedState_MutuallyExclusive_ThrowsSessionException`

---

## Group 1 — `GameStateView.LastActionEvents` (D30) ✅ COMPLETE

Small engine addition; depends only on existing engine code.

### 1.1  Add `LastActionEvents` to `GameStateView` ✅
  - reads: `docs/architecture.md#D30`, `src/Archetype.Engine/GameSession.cs`,
    `src/Archetype.Core/Interfaces.cs`
  - writes: `src/Archetype.Core/Interfaces.cs`, `src/Archetype.Engine/GameSession.cs`
  - `GameStateView` gains `LastActionEvents : IReadOnlyList<GameEvent>` — events
    from the most recently completed `ResolveAction` call; reset at the start
    of each new action. The engine resets this at the top of `ResolveAction`
    and populates it after all blocks, SBRs, and triggers have resolved.

### 1.2  Test `LastActionEvents` ✅
  - reads: `docs/architecture.md#D30`, `tests/Archetype.Tests/GameSession/`
  - writes: `tests/Archetype.Tests/GameSession/LastActionEventsTests.cs` (new file)
  - Tests:
    - `LastActionEvents_PopulatedAfterAction_ContainsActionEvents`
    - `LastActionEvents_ResetBetweenActions_DoesNotCarryOver`
    - `LastActionEvents_IncludesTriggerFiredEvents`

---

## Group 2 — Electron project scaffold (D26)

Create the `tooling/` directory with a working Electron + TypeScript + React
skeleton. No sidecar integration yet — this group establishes the project
structure, build pipeline, and IPC skeleton.

### 2.1  Scaffold `tooling/` Electron project
  - reads: `docs/architecture.md#D26`
  - writes: `tooling/package.json`, `tooling/tsconfig.json`,
    `tooling/tsconfig.main.json`, `tooling/tsconfig.preload.json`,
    `tooling/.eslintrc.json`, `tooling/vite.config.ts`
  - Electron 30+ with `contextIsolation: true`, `nodeIntegration: false`.
    Separate TS configs for main / preload / renderer processes.
    Vite for renderer bundle; `tsx` for main/preload dev run.
    Vitest for renderer tests; add `vitest.config.ts`.

### 2.2  Main process entry point + window management
  - reads: `docs/architecture.md#D26`, `tooling/package.json`
  - writes: `tooling/src/main/main.ts`, `tooling/src/main/windowManager.ts`
  - Create `BrowserWindow` with `contextIsolation: true`. Preload script path
    configured. Dev: load Vite dev server URL. Prod: load `index.html` from
    dist. No sidecar spawn yet.

### 2.3  Preload bridge (`contextBridge`)
  - reads: `docs/architecture.md#D26`
  - writes: `tooling/src/preload/preload.ts`, `tooling/src/shared/ipc.ts`
  - `contextBridge.exposeInMainWorld('archetype', ...)` exposes:
    - `invoke(channel, payload)` — wraps `ipcRenderer.invoke`.
    - `onNotification(channel, handler)` — wraps `ipcRenderer.on`.
  - `src/shared/ipc.ts` defines all IPC channel names and request/response
    types as TypeScript interfaces (strict, no `any`).

### 2.4  Renderer entry point (React)
  - reads: `docs/architecture.md#D26`, `tooling/src/shared/ipc.ts`
  - writes: `tooling/src/renderer/index.tsx`, `tooling/src/renderer/App.tsx`,
    `tooling/index.html`
  - Minimal React app mounting in `#root`. App shell: sidebar nav + main
    content area. No real content yet — placeholder panels.

### 2.5  State management setup (Zustand)
  - reads: `docs/architecture.md#D26`, `docs/architecture.md#D27`
  - writes: `tooling/src/renderer/store/projectStore.ts`,
    `tooling/src/renderer/store/diagnosticsStore.ts`
  - `projectStore`: tracks sidecar connection status, current project name,
    pending mutation state. `diagnosticsStore`: tracks global error/warning
    counts (updated from every sidecar mutation response).

### 2.6  Renderer unit test skeleton
  - reads: `tooling/src/renderer/store/projectStore.ts`
  - writes: `tooling/src/renderer/__tests__/App.test.tsx`
  - Smoke test: App renders without crashing. Establishes that Vitest +
    React Testing Library are correctly wired.

---

## Group 3 — Sidecar scaffold (D26, D27)

Create `src/Archetype.Tooling.Server/` .NET project. Depends on Groups 0 and 1
(uses updated Core types).

### 3.1  Add `Archetype.Tooling.Server` .NET project
  - reads: `docs/architecture.md#D26`, `src/Archetype.slnx` (or equivalent)
  - writes: `src/Archetype.Tooling.Server/Archetype.Tooling.Server.csproj`,
    `src/Archetype.Tooling.Server/Program.cs`
  - Console app targeting .NET 10. References `Archetype.Core`,
    `Archetype.Build`, `Archetype.Text`. Does NOT reference `Archetype.Engine`.
    Add to `.slnx`.

### 3.2  JSON-RPC stdio loop
  - reads: `docs/architecture.md#D26`, `src/Archetype.Tooling.Server/Program.cs`
  - writes: `src/Archetype.Tooling.Server/RpcServer.cs`,
    `src/Archetype.Tooling.Server/RpcRequest.cs`,
    `src/Archetype.Tooling.Server/RpcResponse.cs`
  - Read newline-delimited JSON from `stdin`; dispatch on `method` field; write
    JSON response to `stdout`. Each `RpcRequest` has `id`, `method`, `params`.
    Each `RpcResponse` has `id`, `result` (on success) or `error` (on failure).
    Unknown method → error response. Malformed JSON → error response. Loop until
    stdin is closed.

### 3.3  `ProjectState` and `*Entry` types
  - reads: `docs/architecture.md#D27`
  - writes: `src/Archetype.Tooling.Server/ProjectState.cs`,
    `src/Archetype.Tooling.Server/KeywordEntry.cs`,
    `src/Archetype.Tooling.Server/CardEntry.cs`,
    `src/Archetype.Tooling.Server/ZoneEntry.cs`,
    `src/Archetype.Tooling.Server/PlayerEntry.cs`,
    `src/Archetype.Tooling.Server/CardSetEntry.cs`,
    `src/Archetype.Tooling.Server/PhaseEntry.cs`,
    `src/Archetype.Tooling.Server/ActionRuleEntry.cs`,
    `src/Archetype.Tooling.Server/StateBasedRuleEntry.cs`,
    `src/Archetype.Tooling.Server/InitManifestEntry.cs`,
    `src/Archetype.Tooling.Server/LocalizationState.cs`,
    `src/Archetype.Tooling.Server/ProjectDiagnostic.cs`
  - Each `*Entry` type carries the DSL source string(s) for its expressions
    alongside parse results (`KeywordNode?`, null when parse failed) and a
    per-entry `List<ProjectDiagnostic>`. `ProjectDiagnostic` carries
    `entryKind`, `entryName`, `severity` ("error" | "warning"), `message`,
    optional `dslRange { start, end }`.

### 3.4  `ProjectFileLoader` — lenient load
  - reads: `docs/architecture.md#D27`, `src/Archetype.Tooling.Server/ProjectState.cs`
  - writes: `src/Archetype.Tooling.Server/ProjectFileLoader.cs`
  - Parse `.archetype` JSON into `ProjectState`. On invalid JSON: single fatal
    diagnostic, return empty state. Per-keyword: parse `body` DSL; on failure
    record diagnostic, set `BodyNode = null`. After all entries loaded: run
    cross-entry name-resolution pass; record unresolved-reference diagnostics.
    Note: full type-checking and acyclicity checking are deferred to 3.6.

### 3.5  `ProjectFileSerializer` — save
  - reads: `docs/architecture.md#D27`, `src/Archetype.Tooling.Server/ProjectState.cs`
  - writes: `src/Archetype.Tooling.Server/ProjectFileSerializer.cs`
  - Serialise `ProjectState` to the `.archetype` JSON format. DSL source
    strings are written (not `KeywordNode` trees). `tooling.editorState` is
    round-tripped verbatim from the raw `JsonElement` stored on
    `ProjectState`.

### 3.6  Reference graph and full validation pass
  - reads: `docs/architecture.md#D27`, `docs/architecture.md#D28`,
    `src/Archetype.Tooling.Server/ProjectState.cs`
  - writes: `src/Archetype.Tooling.Server/ReferenceGraph.cs`,
    `src/Archetype.Tooling.Server/Validator.cs`
  - `ReferenceGraph`: maintains `usedBy[entryName] = Set<entryName>` built from
    all parsed `KeywordNode` trees. Rebuilt fully on every mutation (deferred
    incremental optimisation per D28). `Validator`: runs the full
    cross-entry validation pass (name resolution, type checking, acyclicity).
    Populates `ProjectState.Diagnostics` with error/warning `ProjectDiagnostic`
    entries. Missing-translation warnings generated here (severity "warning"
    per D31).

### 3.7  Sidecar unit tests
  - reads: `src/Archetype.Tooling.Server/ProjectFileLoader.cs`,
    `src/Archetype.Tooling.Server/ProjectFileSerializer.cs`,
    `src/Archetype.Tooling.Server/Validator.cs`
  - writes: `tests/Archetype.Tests/Tooling/ProjectFileLoaderTests.cs` (new file),
    `tests/Archetype.Tests/Tooling/ValidatorTests.cs` (new file)
  - `ProjectFileLoader` tests:
    - `Load_ValidProject_ReturnsPopulatedState`
    - `Load_InvalidJson_ReturnsFatalDiagnostic`
    - `Load_KeywordBodySyntaxError_BodyNodeNull_DiagnosticRecorded`
    - `Load_UnresolvedKeywordReference_DiagnosticRecorded`
    - `Load_ToolingSection_RoundTrippedVerbatim`
  - `Validator` tests:
    - `Validate_CleanProject_NoDiagnostics`
    - `Validate_MissingTranslation_WarningSeverity`
    - `Validate_DuplicateKeywordName_ErrorDiagnostic`

---

## Group 4 — Sidecar RPC method implementations (D27, D28)

Implement the 18 RPC methods. Depends on Group 3.

### 4.1  `LoadProject` and `SaveProject` methods
  - reads: `docs/architecture.md#D27`, `src/Archetype.Tooling.Server/RpcServer.cs`
  - writes: `src/Archetype.Tooling.Server/Handlers/LoadProjectHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/SaveProjectHandler.cs`
  - `LoadProject`: accept JSON string param, parse via `ProjectFileLoader`,
    replace in-memory `ProjectState`, return state summary + full diagnostics.
  - `SaveProject`: serialise via `ProjectFileSerializer`, return JSON string
    to caller (Electron main process writes to disk).

### 4.2  Mutation methods (DSL field updates)
  - reads: `docs/architecture.md#D27`, `docs/architecture.md#D28`
  - writes: `src/Archetype.Tooling.Server/Handlers/UpdateKeywordBodyHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/UpdateCardEffectHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/UpdateLifetimeSpecHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/UpdateActivationConditionHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/UpdateCostBodyHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/UpdateFieldHandler.cs`
  - Each handler: apply mutation to `ProjectState`, re-validate affected set
    (initially full re-validation), return scoped diagnostics +
    `globalErrorCount` + `globalWarningCount`.

### 4.3  Structural mutation methods
  - reads: `docs/architecture.md#D27`, `docs/architecture.md#D28`
  - writes: `src/Archetype.Tooling.Server/Handlers/AddEntryHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/RemoveEntryHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/RenameEntryHandler.cs`
  - `AddEntry`: create a new `*Entry` with empty DSL, add to `ProjectState`,
    re-validate, return new entry summary + diagnostics.
  - `RemoveEntry`: remove entry, collect orphan call-site diagnostics, return.
  - `RenameEntry`: rename entry, update all references in `ProjectState`,
    re-validate, return impact diagnostics.

### 4.4  Query methods
  - reads: `docs/architecture.md#D28`
  - writes: `src/Archetype.Tooling.Server/Handlers/GetAllDiagnosticsHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/GetSymbolInfoHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/GetReferenceGraphHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/GetCompletionsHandler.cs`
  - `GetAllDiagnostics`: return full `ProjectDiagnostic[]` sorted by severity
    then entry name.
  - `GetSymbolInfo`: identify symbol at cursor offset, return definition +
    `referencedBy` list from reference graph.
  - `GetReferenceGraph`: return nodes + edges for keyword composition graph.
  - `GetCompletions`: partially parse DSL to cursor; return typed completion
    items (keyword names, in-scope parameters, built-in keywords). Must
    respond within 100ms.

### 4.5  `RenderCardText` method
  - reads: `docs/architecture.md#D28`, `src/Archetype.Text/`
  - writes: `src/Archetype.Tooling.Server/Handlers/RenderCardTextHandler.cs`
  - Look up card entry by name; if `PrimaryEffect` is parsed, call
    `TextRenderer.RenderBlock`; serialise resulting `RenderNode` tree to JSON.
    Return the render tree. Return empty/error node if card is broken.

### 4.6  Export methods (`ExportGameDefinition`, `ExportGodotClasses`)
  - reads: `docs/architecture.md#D27`, `docs/architecture.md#D30`,
    `docs/architecture.md#D31`
  - writes: `src/Archetype.Tooling.Server/Handlers/ExportGameDefinitionHandler.cs`,
    `src/Archetype.Tooling.Server/Handlers/ExportGodotClassesHandler.cs`,
    `src/Archetype.Tooling.Server/Export/GameDefinitionExporter.cs`,
    `src/Archetype.Tooling.Server/Export/GodotClassGenerator.cs`
  - `ExportGameDefinition`:
    1. If `globalErrorCount > 0`, return error response.
    2. Check for missing-translation warnings; if any and `force` not set,
       return `missingTranslations` summary (D31).
    3. If `force: true` or no warnings, construct strict `GameDefinition` from
       `ProjectState`, serialise to `GameDefinition` JSON, return.
  - `ExportGodotClasses`:
    - Derive signal set per D30 rules (inclusion, suppression, opt-in/opt-out).
    - Generate `ArchetypeCard.gd`, `ArchetypeZone.gd`, `ArchetypeSession.gd`,
      `ArchetypePlayer.gd`, `ArchetypeCardImporter.gd`.
    - Return `{ filename → content }` map; main process writes files.

### 4.7  Sidecar RPC handler tests
  - reads: `src/Archetype.Tooling.Server/Handlers/`
  - writes: `tests/Archetype.Tests/Tooling/RpcHandlerTests.cs`
  - Tests:
    - `UpdateKeywordBody_ValidDsl_ReturnsDiagnosticsEmpty`
    - `UpdateKeywordBody_InvalidDsl_ReturnsDiagnosticWithRange`
    - `AddEntry_NewKeyword_AppearsInProjectState`
    - `RemoveEntry_KeywordUsedByCard_ReturnsOrphanDiagnostic`
    - `RenameEntry_UpdatesAllCallSites`
    - `ExportGameDefinition_WithErrors_ReturnsErrorResponse`
    - `ExportGameDefinition_MissingTranslations_NoForce_ReturnsSummary`
    - `ExportGameDefinition_MissingTranslations_Force_ReturnsExport`
    - `ExportGodotClasses_DerivesSignalSet_PerD30Rules`

---

## Group 5 — Main process: sidecar lifecycle + IPC bridge (D26)

Connect Electron main process to sidecar. Depends on Group 2 (scaffold) and
Group 3 (sidecar exists and builds).

### 5.1  Sidecar process manager
  - reads: `docs/architecture.md#D26`, `tooling/src/main/main.ts`
  - writes: `tooling/src/main/sidecarManager.ts`
  - Spawn sidecar via `child_process.spawn`. Resolve binary path via
    `process.resourcesPath` (prod) or build output path (dev). Handle stdout
    as newline-delimited JSON (use a readline interface). Correlate responses
    to requests by `id` using a pending-requests `Map<string, (resolve, reject)>`.
    On sidecar crash: reject all pending requests; attempt one restart; if
    restart fails, emit error to renderer.

### 5.2  IPC bridge — main process handlers
  - reads: `docs/architecture.md#D26`, `tooling/src/shared/ipc.ts`,
    `tooling/src/main/sidecarManager.ts`
  - writes: `tooling/src/main/ipcHandlers.ts`
  - Register `ipcMain.handle` for each channel defined in `src/shared/ipc.ts`.
    Each handler: validate request shape, forward to `sidecarManager.invoke`,
    return response. File I/O channels (`ReadFile`, `WriteFile`,
    `ShowOpenDialog`, `ShowSaveDialog`) call Node's `fs` / Electron's
    `dialog` APIs directly — not forwarded to sidecar.

### 5.3  File I/O channels
  - reads: `docs/architecture.md#D26`
  - writes: `tooling/src/main/fileHandlers.ts`
  - Channels: `ReadFile(path) → string`, `WriteFile(path, content) → void`,
    `ShowOpenDialog(options) → string | null`,
    `ShowSaveDialog(options) → string | null`,
    `GetUserDataPath() → string`.
  - All file paths validated to prevent directory traversal.

### 5.4  Autosave timer
  - reads: `docs/architecture.md#D27`, `tooling/src/main/ipcHandlers.ts`
  - writes: `tooling/src/main/autosave.ts`
  - 60-second inactivity timer: reset on every mutation IPC call; fires
    `SaveProject` → `WriteFile` silently. Only active when a project is open
    (path known). Errors swallowed silently (autosave is best-effort).

---

## Group 6 — Renderer UI panels (D26, D27, D28, D31)

React UI panels. Depends on Group 2 (scaffold) and conceptually on Group 4
(sidecar methods exist), but UI can be built against mocked sidecar responses
in tests.

### 6.1  Monaco DSL editor component
  - reads: `docs/architecture.md#D26`, `docs/architecture.md#D28`
  - writes: `tooling/src/renderer/components/DslEditor.tsx`,
    `tooling/src/renderer/components/DslEditor.test.tsx`
  - Wraps `monaco-editor/react`. Registers a custom `archetype-dsl` language.
    On content change: debounces at configurable delay (read from store,
    default 200ms), then calls `invoke('UpdateKeywordBody', ...)` (or the
    relevant update channel based on `entryKind` prop). Receives
    `IMarkerData[]` prop and calls `monaco.editor.setModelMarkers`.
    Registers `CompletionItemProvider` that calls `invoke('GetCompletions', ...)`.

### 6.2  Keyword editor panel
  - reads: `docs/architecture.md#D26`, `tooling/src/renderer/store/projectStore.ts`
  - writes: `tooling/src/renderer/panels/KeywordEditorPanel.tsx`,
    `tooling/src/renderer/panels/KeywordEditorPanel.test.tsx`
  - List of keywords (sidebar) + detail view per keyword:
    name, parameters (type-dropdown per parameter), `DslEditor` for body,
    text template field, signal behaviour checkbox (`[Signal]` / `[NoSignal]`).
    "Create new keyword" action available from this panel and from the
    unresolved-reference inline quickfix.

### 6.3  Card editor panel
  - reads: `docs/architecture.md#D26`, `docs/architecture.md#D30`
  - writes: `tooling/src/renderer/panels/CardEditorPanel.tsx`,
    `tooling/src/renderer/panels/CardEditorPanel.test.tsx`
  - Fields: name, static properties (schema-driven), primary effect
    (`DslEditor`), additional effects (accordion), static effects, activation
    condition (`DslEditor`), cost definitions, flavour text, art (file picker +
    crop region selector). Card text preview sub-panel (read-only, calls
    `RenderCardText`).

### 6.4  Game rules panel (phases, action rules, state-based rules,
         trigger resolution order)
  - reads: `docs/architecture.md#D26`
  - writes: `tooling/src/renderer/panels/GameRulesPanel.tsx`,
    `tooling/src/renderer/panels/GameRulesPanel.test.tsx`
  - Phases: ordered list with drag handles; per-phase init/cleanup `DslEditor`.
    Action rules: accordion by action type; per-rule before/after `DslEditor`;
    reorderable within type section. State-based rules: ordered list with drag
    handles; per-rule condition + body `DslEditor`. Trigger resolution order:
    radio group.

### 6.5  InitManifest editor panel
  - reads: `docs/architecture.md#D26`, `docs/architecture.md#D29`
  - writes: `tooling/src/renderer/panels/InitManifestPanel.tsx`,
    `tooling/src/renderer/panels/InitManifestPanel.test.tsx`
  - Player-scoped accordion with neutral/shared zones section at top. Zone
    rows with inline accumulator/condition editing. Card rows nested under
    zone, reorderable. All mutations send `UpdateInitManifest` to sidecar.

### 6.6  Localization panel
  - reads: `docs/architecture.md#D26`, `docs/architecture.md#D31`
  - writes: `tooling/src/renderer/panels/LocalizationPanel.tsx`,
    `tooling/src/renderer/panels/LocalizationPanel.test.tsx`
  - Source language selection. Translation view: source strings on left,
    editable target strings on right, per locale. Missing strings flagged
    visually (warning state, not error).

### 6.7  Problems panel
  - reads: `docs/architecture.md#D28`
  - writes: `tooling/src/renderer/panels/ProblemsPanel.tsx`,
    `tooling/src/renderer/panels/ProblemsPanel.test.tsx`
  - On panel open: calls `GetAllDiagnostics`. Sorted list: errors first, then
    warnings. Each row: severity icon + entry kind + entry name + message.
    Clicking a row navigates to the relevant panel and entry.

### 6.8  Status bar + export flow (D31)
  - reads: `docs/architecture.md#D31`
  - writes: `tooling/src/renderer/components/StatusBar.tsx`,
    `tooling/src/renderer/components/ExportModal.tsx`,
    `tooling/src/renderer/components/StatusBar.test.tsx`,
    `tooling/src/renderer/components/ExportModal.test.tsx`
  - `StatusBar`: always-visible footer bar showing `X errors  Y warnings`.
    Counts sourced from `diagnosticsStore`. Clicking error/warning count opens
    Problems panel.
  - `ExportModal`: shown when `ExportGameDefinition` returns
    `missingTranslations`. Shows per-locale missing counts, "Export anyway" /
    "Cancel" buttons. On "Export anyway" sends second request with
    `{ "force": true }`. No "don't ask again" option.

### 6.9  Keyword composition graph view
  - reads: `docs/architecture.md#D26`
  - writes: `tooling/src/renderer/panels/GraphPanel.tsx`,
    `tooling/src/renderer/panels/GraphPanel.test.tsx`
  - Calls `GetReferenceGraph` on open. Renders nodes (keywords) and edges
    (composition relationships) using React Flow. Node click navigates to
    keyword editor. Layout computed client-side (Dagre algorithm via
    `@dagrejs/dagre`).

### 6.10  Set overview panel
  - reads: `docs/architecture.md#D26`
  - writes: `tooling/src/renderer/panels/SetOverviewPanel.tsx`,
    `tooling/src/renderer/panels/SetOverviewPanel.test.tsx`
  - Card set selector. Keyword distribution: horizontal bar chart per keyword
    showing count of cards in selected set that use it. Uses data from
    `GetReferenceGraph` filtered to selected set. No external charting library
    required — CSS bar chart is sufficient.

---

## Group 7 — Packaging and distribution (D26)

Wire Electron Builder. Depends on Groups 2, 3, 5 (app and sidecar exist).

### 7.1  Electron Builder configuration
  - reads: `docs/architecture.md#D26`, `tooling/package.json`
  - writes: `tooling/electron-builder.config.ts`
  - Platform targets: Windows (NSIS), macOS (DMG), Linux (AppImage).
    Sidecar binaries in `resources/<platform>/Archetype.Tooling.Server`.
    App icon assets referenced.

### 7.2  Sidecar publish script
  - reads: `docs/architecture.md#D26`,
    `src/Archetype.Tooling.Server/Archetype.Tooling.Server.csproj`
  - writes: `tooling/scripts/publish-sidecar.sh`
  - `dotnet publish` for win-x64, linux-x64, osx-x64 and osx-arm64.
    Self-contained, single-file. Outputs to `tooling/resources/<platform>/`.

### 7.3  Dev startup script
  - reads: `docs/architecture.md#D26`
  - writes: `tooling/scripts/dev.sh`, `tooling/package.json` (`scripts.dev` entry)
  - Concurrently: run Vite dev server for renderer + run sidecar from build
    output + run Electron (pointing at Vite URL). Hot module replacement for
    renderer; sidecar restart on C# rebuild.

---

## Completion criteria

- All 102 existing tests still pass (Groups 0–1 may update existing tests).
- All new tests (Groups 0–4) pass.
- `Archetype.Tooling.Server` builds with no warnings.
- Electron app launches in dev mode, connects to sidecar, and the Monaco
  editor sends a `GetCompletions` request that the sidecar handles.
- `ExportGameDefinition` returns a JSON string that round-trips through
  `GameDefinitionLoader.FromJson` without error.
- D31 export modal appears when missing translations exist and export is
  attempted without `force`.
