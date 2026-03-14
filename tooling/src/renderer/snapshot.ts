/**
 * Project snapshot types and fetch helper.
 *
 * The sidecar is the authoritative source for all project data. Panels that
 * need to display content (keyword bodies, card effects, etc.) call
 * fetchSnapshot() on mount to get a parsed copy of the current project state.
 * This is ephemeral display state — not a second canonical store.
 *
 * Types mirror the sidecar's camelCase JSON serialisation format.
 */
import { IPC_CHANNELS } from "@shared/ipc";
import type { SaveProjectResult, ProjectDiagnostic } from "@shared/ipc";

// ---------------------------------------------------------------------------
// Snapshot types
// ---------------------------------------------------------------------------

export interface ParameterSpec {
  readonly name: string;
  readonly type: string;
}

export interface KeywordSnapshot {
  readonly name: string;
  readonly body: string;
  readonly parameters: readonly ParameterSpec[];
  readonly textTemplate: string;
  readonly noSignal: boolean;
}

export interface EffectSnapshot {
  readonly name: string | null; // null = primary effect
  readonly dsl: string;
}

export interface CostSnapshot {
  readonly dsl: string;
}

export interface StaticEffectSnapshot {
  readonly lifetimeDsl: string;
  readonly triggerDsl: string | null;
  readonly effectDsl: string;
}

export interface CardSnapshot {
  readonly name: string;
  readonly primaryEffect: EffectSnapshot;
  readonly additionalEffects: readonly EffectSnapshot[];
  readonly staticEffects: readonly StaticEffectSnapshot[];
  readonly activationCondition: string | null;
  readonly costs: readonly CostSnapshot[];
  readonly flavourText: string;
  readonly artPath: string | null;
  readonly staticProperties: Readonly<Record<string, unknown>>;
}

export interface PhaseSnapshot {
  readonly name: string;
  readonly initDsl: string;
  readonly cleanupDsl: string;
}

export interface ActionRuleSnapshot {
  readonly actionType: string;
  readonly beforeDsl: string;
  readonly afterDsl: string;
}

export interface StateBasedRuleSnapshot {
  readonly conditionDsl: string;
  readonly bodyDsl: string;
}

export interface ZoneSnapshot {
  readonly name: string;
  readonly ownedBy: string | null;
  readonly accumulator: string;
  readonly condition: string | null;
}

export interface InitCardEntry {
  readonly cardName: string;
}

export interface InitZoneEntry {
  readonly zoneName: string;
  readonly cards: readonly InitCardEntry[];
}

export interface PlayerInitSnapshot {
  readonly playerId: string | null; // null = shared/neutral
  readonly zones: readonly InitZoneEntry[];
}

export interface LocaleTranslations {
  readonly locale: string;
  readonly translations: Readonly<Record<string, string>>;
}

export interface ProjectSnapshot {
  readonly id: string | null;
  readonly keywords: readonly KeywordSnapshot[];
  readonly cards: readonly CardSnapshot[];
  readonly zones: readonly ZoneSnapshot[];
  readonly phases: readonly PhaseSnapshot[];
  readonly actionRules: readonly ActionRuleSnapshot[];
  readonly stateBasedRules: readonly StateBasedRuleSnapshot[];
  readonly triggerResolutionOrder: string;
  readonly initManifest: readonly PlayerInitSnapshot[];
  readonly sourceLanguage: string;
  readonly locales: readonly LocaleTranslations[];
}

// ---------------------------------------------------------------------------
// Fetch helper
// ---------------------------------------------------------------------------

/** Serialise → parse project from sidecar. Returns null if no project loaded. */
export async function fetchSnapshot(): Promise<ProjectSnapshot | null> {
  try {
    const result = await window.archetype.invoke<SaveProjectResult>(
      IPC_CHANNELS.SaveProject
    );
    if (!result?.json) return null;
    return JSON.parse(result.json) as ProjectSnapshot;
  } catch {
    return null;
  }
}

// ---------------------------------------------------------------------------
// Marker conversion helpers
// ---------------------------------------------------------------------------

/** Monaco MarkerSeverity numeric values. */
export const MarkerSeverity = { Error: 8, Warning: 4, Info: 2, Hint: 1 } as const;

export interface MarkerData {
  readonly severity: number;
  readonly startLineNumber: number;
  readonly startColumn: number;
  readonly endLineNumber: number;
  readonly endColumn: number;
  readonly message: string;
}

/** Convert a 0-based DSL offset to 1-based line + column. */
function offsetToLineCol(text: string, offset: number): { line: number; col: number } {
  const slice = text.slice(0, offset);
  const lines = slice.split("\n");
  return { line: lines.length, col: (lines[lines.length - 1]?.length ?? 0) + 1 };
}

/** Convert a ProjectDiagnostic (offset-based) to a Monaco MarkerData. */
export function diagnosticToMarker(text: string, diag: ProjectDiagnostic): MarkerData {
  const severity = diag.severity === "error" ? MarkerSeverity.Error : MarkerSeverity.Warning;
  if (!diag.dslRange) {
    return { severity, startLineNumber: 1, startColumn: 1, endLineNumber: 1, endColumn: 1, message: diag.message };
  }
  const start = offsetToLineCol(text, diag.dslRange.start.offset);
  const end   = offsetToLineCol(text, diag.dslRange.end.offset);
  return {
    severity,
    startLineNumber: start.line, startColumn: start.col,
    endLineNumber:   end.line,   endColumn:   end.col,
    message: diag.message,
  };
}
