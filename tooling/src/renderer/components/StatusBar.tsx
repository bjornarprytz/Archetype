/**
 * StatusBar — always-visible footer showing global error/warning counts (Task 6.8, D31).
 *
 * Reads counts from diagnosticsStore (updated after every mutation).
 * Clicking the error or warning count opens the Problems panel via onNavigate.
 */
import React from "react";
import { useDiagnosticsStore } from "../store/diagnosticsStore";
import { useProjectStore } from "../store/projectStore";
import { C, Font } from "../design/tokens";

interface StatusBarProps {
  onNavigate: (panel: string) => void;
}

export function StatusBar({ onNavigate }: StatusBarProps): React.ReactElement {
  const errorCount   = useDiagnosticsStore((s) => s.globalErrorCount);
  const warningCount = useDiagnosticsStore((s) => s.globalWarningCount);
  const projectName  = useProjectStore((s) => s.projectName);
  const isDirty      = useProjectStore((s) => s.isDirty);
  const sidecarStatus = useProjectStore((s) => s.sidecarStatus);

  const sidecarDot = sidecarStatus === "ready"
    ? C.green
    : sidecarStatus === "error"
    ? C.red
    : C.yellow;

  return (
    <div style={styles.bar} role="status" aria-label="Status bar">
      {/* Sidecar status dot */}
      <span
        title={`Sidecar: ${sidecarStatus}`}
        style={{ ...styles.dot, background: sidecarDot }}
      />

      {/* Project name */}
      <span style={styles.project}>
        {projectName
          ? `${projectName}${isDirty ? " ●" : ""}`
          : "no project open"}
      </span>

      <span style={styles.spacer} />

      {/* Error count — clickable */}
      <button
        style={{
          ...styles.pill,
          color: errorCount > 0 ? C.red : C.muted,
          background: errorCount > 0 ? `${C.red}18` : "transparent",
        }}
        onClick={() => onNavigate("problems")}
        aria-label={`${errorCount} errors`}
        title="Open Problems panel"
      >
        <span aria-hidden="true">⊘</span>
        {" "}{errorCount}
      </button>

      {/* Warning count — clickable */}
      <button
        style={{
          ...styles.pill,
          color: warningCount > 0 ? C.yellow : C.muted,
          background: warningCount > 0 ? `${C.yellow}18` : "transparent",
        }}
        onClick={() => onNavigate("problems")}
        aria-label={`${warningCount} warnings`}
        title="Open Problems panel"
      >
        <span aria-hidden="true">⚠</span>
        {" "}{warningCount}
      </button>
    </div>
  );
}

const styles = {
  bar: {
    gridArea: "statusbar",
    display:         "flex",
    alignItems:      "center",
    gap:             "8px",
    padding:         "0 10px",
    background:      C.bg,
    borderTop:       `1px solid ${C.surface0}`,
    fontFamily:      Font.ui,
    fontSize:        "0.725rem",
    color:           C.muted,
    userSelect:      "none" as const,
    overflow:        "hidden",
  } satisfies React.CSSProperties,

  dot: {
    width:        "7px",
    height:       "7px",
    borderRadius: "50%",
    flexShrink:   0,
  } satisfies React.CSSProperties,

  project: {
    color:        C.subtext,
    overflow:     "hidden",
    textOverflow: "ellipsis",
    whiteSpace:   "nowrap" as const,
    maxWidth:     "200px",
  } satisfies React.CSSProperties,

  spacer: {
    flex: 1,
  } satisfies React.CSSProperties,

  pill: {
    fontFamily:   Font.ui,
    fontSize:     "0.725rem",
    padding:      "2px 8px",
    borderRadius: "3px",
    border:       "none",
    cursor:       "pointer",
    display:      "flex",
    alignItems:   "center",
    gap:          "4px",
    lineHeight:   1,
  } satisfies React.CSSProperties,
} as const;
