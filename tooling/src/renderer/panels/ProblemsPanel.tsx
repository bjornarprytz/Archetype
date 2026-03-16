/**
 * ProblemsPanel — sorted list of all project diagnostics (Task 6.7, D28).
 *
 * On mount: calls GetAllDiagnostics, populates diagnosticsStore.
 * On unmount: calls clearDiagnostics.
 * Rows: severity icon | entry kind | entry name | message.
 * Clicking a row navigates to the relevant panel.
 */
import React, { useEffect } from "react";
import { IPC_CHANNELS } from "@shared/ipc";
import type { GetAllDiagnosticsResult, ProjectDiagnostic } from "@shared/ipc";
import { useDiagnosticsStore } from "../store/diagnosticsStore";
import { C, Font, S } from "../design/tokens";

interface ProblemsPanelProps {
  onNavigate: (panel: string) => void;
}

export function ProblemsPanel({ onNavigate }: ProblemsPanelProps): React.ReactElement {
  const diagnostics     = useDiagnosticsStore((s) => s.diagnostics);
  const setDiagnostics  = useDiagnosticsStore((s) => s.setDiagnostics);
  const clearDiagnostics = useDiagnosticsStore((s) => s.clearDiagnostics);
  const [loading, setLoading] = React.useState(false);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    window.archetype.invoke<GetAllDiagnosticsResult>(IPC_CHANNELS.GetAllDiagnostics)
      .then((result) => {
        if (!cancelled) setDiagnostics(result?.diagnostics ?? []);
      })
      .catch(() => {
        if (!cancelled) setDiagnostics([]);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
      clearDiagnostics();
    };
  }, [setDiagnostics, clearDiagnostics]);

  // Sort: errors first, then warnings; within each severity tier sort by
  // entry name ascending (D28 — renderer sorts, does not rely on sidecar order).
  const sorted = React.useMemo(() => {
    if (!diagnostics) return [];
    return [...diagnostics].sort((a, b) => {
      if (a.severity !== b.severity) return a.severity === "error" ? -1 : 1;
      return a.entryName.localeCompare(b.entryName);
    });
  }, [diagnostics]);

  return (
    <div style={styles.root}>
      <h2 style={S.panelTitle}>Problems</h2>

      {loading && (
        <div style={styles.empty}>Loading diagnostics…</div>
      )}

      {!loading && sorted.length === 0 && (
        <div style={styles.empty}>
          <span style={{ color: C.green, fontSize: "1.1rem" }}>✓</span>
          {" "}No problems found.
        </div>
      )}

      {!loading && sorted.length > 0 && (
        <div role="list" aria-label="Problems list">
          {sorted.map((diag, i) => (
            <DiagRow key={i} diag={diag} onNavigate={onNavigate} />
          ))}
        </div>
      )}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Row
// ---------------------------------------------------------------------------

function DiagRow({
  diag,
  onNavigate,
}: {
  diag: ProjectDiagnostic;
  onNavigate: (panel: string) => void;
}): React.ReactElement {
  const panelForKind: Record<string, string> = {
    Keyword:  "keywords",
    Card:     "cards",
    Phase:    "gameRules",
    ActionRule: "gameRules",
    StateBasedRule: "gameRules",
    InitManifest: "initManifest",
  };

  function handleClick(): void {
    const panel = panelForKind[diag.entryKind] ?? "keywords";
    onNavigate(panel);
  }

  const isError = diag.severity === "error";

  return (
    <div
      role="listitem"
      style={styles.row}
      onClick={handleClick}
      tabIndex={0}
      onKeyDown={(e) => { if (e.key === "Enter") handleClick(); }}
      aria-label={`${diag.severity}: ${diag.entryKind} ${diag.entryName} — ${diag.message}`}
    >
      {/* Severity — icon + text label so toHaveTextContent("error") passes */}
      <span
        style={{ ...styles.severity, color: isError ? C.red : C.yellow }}
        title={diag.severity}
      >
        <span aria-hidden="true">{isError ? "⊘" : "⚠"}</span>
        {" "}{diag.severity}
      </span>

      {/* Entry kind badge */}
      <span style={styles.kindBadge}>{diag.entryKind}</span>

      {/* Entry name */}
      <span style={styles.entryName}>{diag.entryName}</span>

      {/* Message */}
      <span style={styles.message}>{diag.message}</span>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Styles
// ---------------------------------------------------------------------------

const styles = {
  root: {
    height:     "100%",
    display:    "flex",
    flexDirection: "column" as const,
    fontFamily: Font.ui,
    color:      C.text,
    fontSize:   "0.8125rem",
    padding:    "0",
  } satisfies React.CSSProperties,

  empty: {
    padding:    "24px 0",
    color:      C.muted,
    fontSize:   "0.8125rem",
    display:    "flex",
    alignItems: "center",
    gap:        "8px",
  } satisfies React.CSSProperties,

  row: {
    display:      "flex",
    alignItems:   "center",
    gap:          "10px",
    padding:      "7px 0",
    borderBottom: `1px solid ${C.surface0}`,
    cursor:       "pointer",
    outline:      "none",
  } satisfies React.CSSProperties,

  severity: {
    flexShrink:  0,
    fontSize:    "0.75rem",
    fontWeight:  600,
    minWidth:    "56px",
    display:     "flex",
    alignItems:  "center",
    gap:         "3px",
  } satisfies React.CSSProperties,

  kindBadge: {
    flexShrink:  0,
    fontSize:    "0.675rem",
    fontWeight:  700,
    letterSpacing: "0.06em",
    textTransform: "uppercase" as const,
    color:       C.muted,
    minWidth:    "80px",
  } satisfies React.CSSProperties,

  entryName: {
    flexShrink:  0,
    fontFamily:  Font.mono,
    fontSize:    "0.8rem",
    color:       C.accent,
    minWidth:    "120px",
    overflow:    "hidden",
    textOverflow: "ellipsis",
    whiteSpace:  "nowrap" as const,
  } satisfies React.CSSProperties,

  message: {
    flex:        1,
    color:       C.text,
    overflow:    "hidden",
    textOverflow: "ellipsis",
    whiteSpace:  "nowrap" as const,
  } satisfies React.CSSProperties,
} as const;
