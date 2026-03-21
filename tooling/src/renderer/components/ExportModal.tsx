/**
 * ExportModal — confirmation dialog for missing-translation exports (Task 6.8, D31).
 *
 * Shown when ExportGameDefinition returns { kind: "missingTranslations" }.
 * Displays per-locale missing counts and offers "Export anyway" / "Cancel".
 * On "Export anyway" re-sends the request with { force: true }, then triggers
 * a file-save dialog.
 */
import React from "react";
import { IPC_CHANNELS } from "@shared/ipc";
import type { ExportGameDefinitionResult, MissingTranslationSummary } from "@shared/ipc";
import { C, Font, S } from "../design/tokens";

interface ExportModalProps {
  summary: readonly MissingTranslationSummary[];
  onClose: () => void;
  /** Called with the exported JSON after a forced export succeeds. */
  onExported: (json: string) => void;
}

export function ExportModal({
  summary,
  onClose,
  onExported,
}: ExportModalProps): React.ReactElement {
  const [loading, setLoading] = React.useState(false);
  const [error,   setError]   = React.useState<string | null>(null);

  async function handleExportAnyway(): Promise<void> {
    setLoading(true);
    setError(null);
    try {
      const result = await window.archetype.invoke<ExportGameDefinitionResult>(
        IPC_CHANNELS.ExportGameDefinition,
        { force: true }
      );
      if (result.kind === "success") {
        onExported(result.json);
        onClose();
      } else if (result.kind === "errors") {
        setError(`Export blocked: ${result.globalErrorCount} error(s) in project.`);
      } else {
        setError("Unexpected response from sidecar.");
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : "Export failed.");
    } finally {
      setLoading(false);
    }
  }

  const totalMissing = summary.reduce((acc, s) => acc + s.missingCount, 0);

  return (
    /* Backdrop */
    <div style={styles.backdrop} role="dialog" aria-modal="true" aria-label="Export with missing translations">
      <div style={styles.modal}>
        {/* Header */}
        <div style={styles.header}>
          <span style={{ color: C.yellow, fontSize: "1rem" }}>⚠</span>
          <h2 style={styles.title}>Missing Translations</h2>
        </div>

        <p style={styles.body}>
          {totalMissing} string{totalMissing !== 1 ? "s" : ""} are untranslated across{" "}
          {summary.length} locale{summary.length !== 1 ? "s" : ""}. Export will include
          untranslated placeholders.
        </p>

        {/* Per-locale table */}
        <div style={styles.table}>
          <div style={styles.tableHeader}>
            <span>Locale</span>
            <span>Missing</span>
          </div>
          {summary.map((row) => (
            <div key={row.locale} style={styles.tableRow}>
              <span style={{ fontFamily: Font.mono, fontSize: "0.8rem" }}>{row.locale}</span>
              <span style={{ color: C.yellow, fontWeight: 600 }}>{row.missingCount}</span>
            </div>
          ))}
        </div>

        {error && (
          <p style={styles.errorMsg} role="alert">{error}</p>
        )}

        {/* Actions */}
        <div style={styles.actions}>
          <button style={S.btnGhost} onClick={onClose} disabled={loading}>
            Cancel
          </button>
          <button
            style={{ ...S.btnPrimary, background: C.yellow, color: C.bg, opacity: loading ? 0.6 : 1 }}
            onClick={() => { void handleExportAnyway(); }}
            disabled={loading}
            aria-label="Export anyway"
          >
            {loading ? "Exporting…" : "Export anyway"}
          </button>
        </div>
      </div>
    </div>
  );
}

const styles = {
  backdrop: {
    position:        "fixed" as const,
    inset:           0,
    background:      "rgba(0,0,0,0.6)",
    display:         "flex",
    alignItems:      "center",
    justifyContent:  "center",
    zIndex:          1000,
  } satisfies React.CSSProperties,

  modal: {
    background:   C.base,
    border:       `1px solid ${C.surface0}`,
    borderRadius: "8px",
    padding:      "24px",
    width:        "380px",
    maxWidth:     "90vw",
    fontFamily:   Font.ui,
    color:        C.text,
  } satisfies React.CSSProperties,

  header: {
    display:      "flex",
    alignItems:   "center",
    gap:          "10px",
    marginBottom: "12px",
  } satisfies React.CSSProperties,

  title: {
    margin:     0,
    fontSize:   "1rem",
    fontWeight: 600,
    color:      C.text,
  } satisfies React.CSSProperties,

  body: {
    fontSize:     "0.8125rem",
    color:        C.subtext,
    marginBottom: "16px",
    lineHeight:   1.5,
    margin:       "0 0 16px",
  } satisfies React.CSSProperties,

  table: {
    border:       `1px solid ${C.surface0}`,
    borderRadius: "4px",
    overflow:     "hidden",
    marginBottom: "16px",
    fontSize:     "0.8125rem",
  } satisfies React.CSSProperties,

  tableHeader: {
    display:        "flex",
    justifyContent: "space-between",
    padding:        "6px 12px",
    background:     C.bg,
    color:          C.muted,
    fontSize:       "0.7rem",
    fontWeight:     700,
    letterSpacing:  "0.08em",
    textTransform:  "uppercase" as const,
  } satisfies React.CSSProperties,

  tableRow: {
    display:        "flex",
    justifyContent: "space-between",
    padding:        "6px 12px",
    borderTop:      `1px solid ${C.surface0}`,
  } satisfies React.CSSProperties,

  errorMsg: {
    fontSize:     "0.8rem",
    color:        C.red,
    marginBottom: "12px",
    margin:       "0 0 12px",
  } satisfies React.CSSProperties,

  actions: {
    display:        "flex",
    justifyContent: "flex-end",
    gap:            "10px",
  } satisfies React.CSSProperties,
} as const;
