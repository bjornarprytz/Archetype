/**
 * IPC channel contracts (D26, D28).
 *
 * This file is the single source of truth for every channel name and
 * request/response shape that crosses the renderer ↔ main process boundary.
 * It is imported by both sides — main process registers handlers, renderer
 * calls invoke.  No `any` is permitted here; every shape is explicitly typed.
 *
 * Channel naming convention: PascalCase method name matching the sidecar RPC
 * method name exactly. File I/O channels use a "File:" prefix to distinguish
 * them from sidecar-forwarded channels.
 */

// ---------------------------------------------------------------------------
// Shared primitive types
// ---------------------------------------------------------------------------

/** Diagnostic severity, mirroring the sidecar's ProjectDiagnostic.Severity. */
export type DiagnosticSeverity = "error" | "warning";

/** Position within a DSL string. */
export interface DslPosition {
  readonly offset: number;
}

/** A span within a DSL string, used for inline error markers. */
export interface DslRange {
  readonly start: DslPosition;
  readonly end: DslPosition;
}

/** A single project diagnostic, mirroring Archetype.Tooling.Server.ProjectDiagnostic. */
export interface ProjectDiagnostic {
  readonly entryKind: string;
  readonly entryName: string;
  readonly severity: DiagnosticSeverity;
  readonly message: string;
  readonly dslRange?: DslRange;
}

/** Standard response shape for all sidecar mutation methods (D28). */
export interface MutationResponse {
  readonly affectedEntries: readonly string[];
  readonly diagnostics: readonly ProjectDiagnostic[];
  readonly globalErrorCount: number;
  readonly globalWarningCount: number;
}

// ---------------------------------------------------------------------------
// Sidecar method parameter and result types
// ---------------------------------------------------------------------------

// --- LoadProject ---
export interface LoadProjectParams {
  readonly json: string;
}
export interface LoadProjectResult {
  readonly id: string | null;
  readonly keywordCount: number;
  readonly cardCount: number;
  readonly diagnostics: readonly ProjectDiagnostic[];
  readonly globalErrorCount: number;
  readonly globalWarningCount: number;
}

// --- SaveProject ---
export interface SaveProjectResult {
  readonly json: string;
}

// --- UpdateKeywordBody ---
export interface UpdateKeywordBodyParams {
  readonly entryName: string;
  readonly dsl: string;
}

// --- UpdateCardEffect ---
export interface UpdateCardEffectParams {
  readonly cardName: string;
  readonly effectName: string | null; // null = primary effect
  readonly dsl: string;
}

// --- UpdateLifetimeSpec ---
export interface UpdateLifetimeSpecParams {
  readonly cardName: string;
  readonly staticEffectIndex: number;
  readonly dsl: string;
}

// --- UpdateActivationCondition ---
export interface UpdateActivationConditionParams {
  readonly cardName: string;
  readonly dsl: string;
}

// --- UpdateCostBody ---
export interface UpdateCostBodyParams {
  readonly cardName: string;
  readonly costIndex: number;
  readonly dsl: string;
}

// --- UpdateField ---
export interface UpdateFieldParams {
  readonly entryKind: string;
  readonly entryName: string;
  readonly field: string;
  readonly value: unknown;
}

// --- AddEntry ---
export interface AddEntryParams {
  readonly entryKind: string;
  readonly entryName: string;
}

// --- RemoveEntry ---
export interface RemoveEntryParams {
  readonly entryKind: string;
  readonly entryName: string;
}

// --- RenameEntry ---
export interface RenameEntryParams {
  readonly entryKind: string;
  readonly oldName: string;
  readonly newName: string;
}

// --- GetAllDiagnostics ---
export interface GetAllDiagnosticsResult {
  readonly diagnostics: readonly ProjectDiagnostic[];
}

// --- GetSymbolInfo ---
export interface GetSymbolInfoParams {
  readonly entryKind: string;
  readonly entryName: string;
  readonly dslField: string;
  readonly offset: number;
}
export interface SymbolDefinition {
  readonly name: string;
  readonly kind: string;
  readonly description: string | null;
}
export interface GetSymbolInfoResult {
  readonly symbol: SymbolDefinition | null;
  readonly referencedBy: readonly { readonly entryName: string }[];
}

// --- GetReferenceGraph ---
export interface GraphNode {
  readonly id: string;
  readonly kind: string;
}
export interface GraphEdge {
  readonly source: string;
  readonly target: string;
}
export interface GetReferenceGraphResult {
  readonly nodes: readonly GraphNode[];
  readonly edges: readonly GraphEdge[];
}

// --- GetCompletions ---
export interface GetCompletionsParams {
  readonly entryKind: string;
  readonly entryName: string;
  readonly dslField: string;
  readonly offset: number;
  readonly prefix: string;
}
export interface CompletionItem {
  readonly label: string;
  readonly kind: string;
  readonly detail: string | null;
}
export interface GetCompletionsResult {
  readonly items: readonly CompletionItem[];
}

// --- RenderCardText ---
export interface RenderCardTextParams {
  readonly cardName: string;
  readonly locale: string | null;
}
// RenderNode is a recursive discriminated union; use unknown here and narrow
// at the use-site, since the renderer only displays the tree as a preview.
export type RenderNode = unknown;
export interface RenderCardTextResult {
  readonly renderTree: RenderNode;
}

// --- ExportGameDefinition ---
export interface ExportGameDefinitionParams {
  readonly force?: boolean;
}
export interface MissingTranslationSummary {
  readonly locale: string;
  readonly missingCount: number;
}
export type ExportGameDefinitionResult =
  | { readonly kind: "success"; readonly json: string }
  | { readonly kind: "errors"; readonly globalErrorCount: number }
  | {
      readonly kind: "missingTranslations";
      readonly summary: readonly MissingTranslationSummary[];
    };

// --- ExportGodotClasses ---
export interface ExportGodotClassesResult {
  readonly files: Record<string, string>;
}

// ---------------------------------------------------------------------------
// File I/O channel types (handled by main process directly, not forwarded to
// the sidecar — D26)
// ---------------------------------------------------------------------------

export interface ShowOpenDialogOptions {
  readonly title?: string;
  readonly defaultPath?: string;
  readonly filters?: ReadonlyArray<{ readonly name: string; readonly extensions: readonly string[] }>;
  readonly properties?: ReadonlyArray<"openFile" | "openDirectory" | "multiSelections">;
}

export interface ShowSaveDialogOptions {
  readonly title?: string;
  readonly defaultPath?: string;
  readonly filters?: ReadonlyArray<{ readonly name: string; readonly extensions: readonly string[] }>;
}

// ---------------------------------------------------------------------------
// Channel name constants — the single source of truth for every channel name.
// Both the preload contextBridge and ipcMain.handle use these values.
// ---------------------------------------------------------------------------

export const IPC_CHANNELS = {
  // Sidecar-forwarded channels
  LoadProject: "LoadProject",
  SaveProject: "SaveProject",
  UpdateKeywordBody: "UpdateKeywordBody",
  UpdateCardEffect: "UpdateCardEffect",
  UpdateLifetimeSpec: "UpdateLifetimeSpec",
  UpdateActivationCondition: "UpdateActivationCondition",
  UpdateCostBody: "UpdateCostBody",
  UpdateField: "UpdateField",
  AddEntry: "AddEntry",
  RemoveEntry: "RemoveEntry",
  RenameEntry: "RenameEntry",
  GetAllDiagnostics: "GetAllDiagnostics",
  GetSymbolInfo: "GetSymbolInfo",
  GetReferenceGraph: "GetReferenceGraph",
  GetCompletions: "GetCompletions",
  RenderCardText: "RenderCardText",
  ExportGameDefinition: "ExportGameDefinition",
  ExportGodotClasses: "ExportGodotClasses",

  // File I/O channels (main process only)
  ReadFile: "File:ReadFile",
  WriteFile: "File:WriteFile",
  ShowOpenDialog: "File:ShowOpenDialog",
  ShowSaveDialog: "File:ShowSaveDialog",
  GetUserDataPath: "File:GetUserDataPath",
} as const;

/** Union of all valid channel names. */
export type IpcChannel = (typeof IPC_CHANNELS)[keyof typeof IPC_CHANNELS];

// ---------------------------------------------------------------------------
// The API surface exposed by the preload to the renderer via contextBridge.
// The renderer accesses this as `window.archetype`.
// ---------------------------------------------------------------------------

/**
 * The Archetype API exposed to the renderer process.
 * Defined here as a TypeScript interface so both preload and renderer agree
 * on the exact shape — no runtime discovery needed.
 */
export interface ArchetypeApi {
  /**
   * Send a request to the main process (and typically on to the sidecar).
   * Returns a promise resolving to the result, or rejecting on error.
   */
  invoke<T = unknown>(channel: IpcChannel, payload?: unknown): Promise<T>;

  /**
   * Subscribe to push notifications from the main process.
   * Returns an unsubscribe function.
   */
  onNotification(channel: IpcChannel, handler: (payload: unknown) => void): () => void;
}

// Extend the Window interface so the renderer can use `window.archetype`
// with full type safety.
declare global {
  interface Window {
    archetype: ArchetypeApi;
  }
}
