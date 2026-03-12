import { create } from "zustand";
import type { ProjectDiagnostic } from "@shared/ipc";

// ---------------------------------------------------------------------------
// diagnosticsStore — global error/warning counts (D26, D28)
//
// Every sidecar mutation response returns globalErrorCount and
// globalWarningCount.  This store receives those counts so the status bar and
// any badge UI can update without polling GetAllDiagnostics.
//
// When the Problems panel opens it calls GetAllDiagnostics and populates the
// full diagnostics list directly (Group 6).  This store does NOT hold the
// full list in the background — that would mean maintaining a second copy of
// data the sidecar owns.
// ---------------------------------------------------------------------------

export interface DiagnosticsState {
  /** Project-wide error count. Sourced from every mutation response. */
  globalErrorCount: number;

  /** Project-wide warning count. Sourced from every mutation response. */
  globalWarningCount: number;

  /**
   * Full diagnostic list, populated only when the Problems panel is open.
   * Null when not loaded; stale after subsequent mutations until refreshed.
   */
  diagnostics: readonly ProjectDiagnostic[] | null;
}

interface DiagnosticsActions {
  /**
   * Update global counts from a mutation response.
   * Called after every sidecar mutation completes.
   */
  updateCounts: (errorCount: number, warningCount: number) => void;

  /**
   * Replace the full diagnostics list (called by ProblemsPanel on open).
   */
  setDiagnostics: (diagnostics: readonly ProjectDiagnostic[]) => void;

  /**
   * Clear the full list (called when the Problems panel closes or a project
   * is unloaded; counts are preserved — they are always current).
   */
  clearDiagnostics: () => void;

  /** Reset all state (called when a project is closed). */
  reset: () => void;
}

const initialState: DiagnosticsState = {
  globalErrorCount: 0,
  globalWarningCount: 0,
  diagnostics: null,
};

export const useDiagnosticsStore = create<DiagnosticsState & DiagnosticsActions>((set) => ({
  ...initialState,

  updateCounts(errorCount, warningCount) {
    set({ globalErrorCount: errorCount, globalWarningCount: warningCount });
  },

  setDiagnostics(diagnostics) {
    set({ diagnostics });
  },

  clearDiagnostics() {
    set({ diagnostics: null });
  },

  reset() {
    set({ ...initialState });
  },
}));
