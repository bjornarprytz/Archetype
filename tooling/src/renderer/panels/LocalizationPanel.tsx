/**
 * LocalizationPanel — source language picker + translation editor (Task 6.6, D26, D31).
 *
 * Source language selection (dropdown).
 * Translation view: source strings on left, editable target strings on right.
 * Missing strings shown with a warning state (yellow highlight), not error.
 */
import React, { useEffect, useState } from "react";
import { IPC_CHANNELS } from "@shared/ipc";
import type { MutationResponse } from "@shared/ipc";
import { useDiagnosticsStore } from "../store/diagnosticsStore";
import { useProjectStore } from "../store/projectStore";
import { fetchSnapshot } from "../snapshot";
import type { LocaleTranslations } from "../snapshot";
import { C, Font, S } from "../design/tokens";

const COMMON_LANGUAGES = [
  { code: "en", name: "English" },
  { code: "de", name: "German" },
  { code: "fr", name: "French" },
  { code: "es", name: "Spanish" },
  { code: "it", name: "Italian" },
  { code: "pt", name: "Portuguese" },
  { code: "ja", name: "Japanese" },
  { code: "ko", name: "Korean" },
  { code: "zh", name: "Chinese (Simplified)" },
];

export function LocalizationPanel(): React.ReactElement {
  const [sourceLanguage, setSourceLanguage] = useState("en");
  const [locales,        setLocales]        = useState<LocaleTranslations[]>([]);
  const [activeLocale,   setActiveLocale]   = useState<string | null>(null);
  const [loading,        setLoading]        = useState(false);

  const updateCounts = useDiagnosticsStore((s) => s.updateCounts);
  const markDirty    = useProjectStore((s) => s.markDirty);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    fetchSnapshot().then((snap) => {
      if (cancelled) return;
      setSourceLanguage(snap?.sourceLanguage ?? "en");
      setLocales((snap?.locales ?? []) as LocaleTranslations[]);
      if (snap?.locales?.[0]) setActiveLocale(snap.locales[0].locale);
      setLoading(false);
    });
    return () => { cancelled = true; };
  }, []);

  async function handleSourceLangChange(lang: string): Promise<void> {
    setSourceLanguage(lang);
    const resp = await window.archetype.invoke<MutationResponse>(IPC_CHANNELS.UpdateField, {
      entryKind: "Localization",
      entryName: "localization",
      field:     "sourceLanguage",
      value:     lang,
    });
    markDirty();
    updateCounts(resp.globalErrorCount, resp.globalWarningCount);
  }

  async function handleTranslationChange(
    locale: string,
    key: string,
    value: string
  ): Promise<void> {
    setLocales((prev) =>
      prev.map((l) =>
        l.locale === locale
          ? { ...l, translations: { ...l.translations, [key]: value } }
          : l
      )
    );
    const resp = await window.archetype.invoke<MutationResponse>(IPC_CHANNELS.UpdateField, {
      entryKind: "Localization",
      entryName: locale,
      field:     key,
      value,
    });
    markDirty();
    updateCounts(resp.globalErrorCount, resp.globalWarningCount);
  }

  // Source locale entry (for displaying source strings on the left)
  const sourceLocale = locales.find((l) => l.locale === sourceLanguage);
  const sourceKeys   = sourceLocale ? Object.keys(sourceLocale.translations) : [];

  const activeLocaleData = locales.find((l) => l.locale === activeLocale);

  return (
    <div style={styles.root}>
      <h2 style={S.panelTitle}>Localization</h2>

      {/* Source language */}
      <div style={styles.sourceRow}>
        <label style={S.sectionLabel} htmlFor="source-lang">Source Language</label>
        <select
          id="source-lang"
          style={{ ...S.select, minWidth: "180px" }}
          value={sourceLanguage}
          onChange={(e) => { void handleSourceLangChange(e.target.value); }}
        >
          {COMMON_LANGUAGES.map((l) => (
            <option key={l.code} value={l.code}>{l.name} ({l.code})</option>
          ))}
        </select>
      </div>

      {loading && <div style={styles.empty}>Loading…</div>}

      {!loading && locales.length === 0 && (
        <div style={styles.empty}>No locales defined in this project.</div>
      )}

      {!loading && locales.length > 0 && (
        <div style={styles.mainArea}>
          {/* Locale selector */}
          <div style={styles.localeTabs} role="tablist" aria-label="Locale tabs">
            {locales.map((l) => {
              const missingCount = sourceKeys.filter((k) => !l.translations[k]).length;
              return (
                <button
                  key={l.locale}
                  role="tab"
                  aria-selected={l.locale === activeLocale}
                  style={{
                    ...styles.localeTab,
                    ...(l.locale === activeLocale ? styles.localeTabActive : {}),
                  }}
                  onClick={() => setActiveLocale(l.locale)}
                >
                  <span>{l.locale}</span>
                  {missingCount > 0 && (
                    <span style={styles.missingBadge} title={`${missingCount} missing`}>
                      {missingCount}
                    </span>
                  )}
                </button>
              );
            })}
          </div>

          {/* Translation editor */}
          {activeLocaleData && (
            <div style={styles.translationGrid}>
              {/* Header */}
              <div style={styles.gridHeader}>
                <span>{sourceLanguage} (source)</span>
                <span>{activeLocale}</span>
              </div>

              {/* Rows */}
              <div style={styles.gridBody}>
                {sourceKeys.length === 0 && (
                  <div style={styles.empty}>No source strings found.</div>
                )}
                {sourceKeys.map((key) => {
                  const sourceVal  = sourceLocale?.translations[key] ?? key;
                  const targetVal  = activeLocaleData.translations[key] ?? "";
                  const isMissing  = !targetVal;
                  return (
                    <div key={key} style={styles.gridRow}>
                      {/* Source */}
                      <div style={styles.sourceCell}>
                        <div style={styles.keyLabel}>{key}</div>
                        <div style={styles.sourceText}>{sourceVal}</div>
                      </div>

                      {/* Target */}
                      <div style={{
                        ...styles.targetCell,
                        ...(isMissing ? styles.targetCellMissing : {}),
                      }}>
                        <textarea
                          style={{
                            ...S.textarea,
                            height:     "54px",
                            background: isMissing ? `${C.yellow}10` : C.bg,
                            border:     isMissing
                              ? `1px solid ${C.yellow}60`
                              : `1px solid ${C.surface0}`,
                          }}
                          value={targetVal}
                          onChange={(e) => {
                            void handleTranslationChange(activeLocale!, key, e.target.value);
                          }}
                          placeholder={isMissing ? "⚠ Missing translation" : ""}
                          aria-label={`Translation for ${key}`}
                        />
                        {isMissing && (
                          <span style={styles.missingHint} aria-label="Missing translation warning">
                            ⚠ missing
                          </span>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Styles
// ---------------------------------------------------------------------------

const styles = {
  root: {
    height:        "100%",
    display:       "flex",
    flexDirection: "column" as const,
    fontFamily:    Font.ui,
    color:         C.text,
    fontSize:      "0.8125rem",
    overflow:      "hidden",
  } satisfies React.CSSProperties,

  sourceRow: {
    display:      "flex",
    alignItems:   "center",
    gap:          "12px",
    marginBottom: "16px",
  } satisfies React.CSSProperties,

  empty: {
    color:   C.muted,
    padding: "12px 0",
    fontSize: "0.8125rem",
  } satisfies React.CSSProperties,

  mainArea: {
    flex:          1,
    display:       "flex",
    flexDirection: "column" as const,
    overflow:      "hidden",
  } satisfies React.CSSProperties,

  localeTabs: {
    display:      "flex",
    gap:          "4px",
    borderBottom: `1px solid ${C.surface0}`,
    marginBottom: "16px",
    flexWrap:     "wrap" as const,
  } satisfies React.CSSProperties,

  localeTab: {
    fontFamily:   Font.ui,
    fontSize:     "0.8125rem",
    padding:      "6px 14px",
    border:       "none",
    borderBottom: "2px solid transparent",
    background:   "transparent",
    color:        C.subtext,
    cursor:       "pointer",
    display:      "flex",
    alignItems:   "center",
    gap:          "6px",
  } satisfies React.CSSProperties,

  localeTabActive: {
    color:        C.accent,
    borderBottom: `2px solid ${C.accent}`,
  } satisfies React.CSSProperties,

  missingBadge: {
    background:   `${C.yellow}30`,
    color:        C.yellow,
    fontSize:     "0.65rem",
    fontWeight:   700,
    padding:      "1px 5px",
    borderRadius: "9px",
  } satisfies React.CSSProperties,

  translationGrid: {
    flex:          1,
    display:       "flex",
    flexDirection: "column" as const,
    overflow:      "hidden",
  } satisfies React.CSSProperties,

  gridHeader: {
    display:       "grid",
    gridTemplateColumns: "1fr 1fr",
    gap:           "16px",
    fontSize:      "0.65rem",
    fontWeight:    700,
    letterSpacing: "0.08em",
    textTransform: "uppercase" as const,
    color:         C.muted,
    padding:       "0 0 8px",
    borderBottom:  `1px solid ${C.surface0}`,
    marginBottom:  "8px",
  } satisfies React.CSSProperties,

  gridBody: {
    flex:      1,
    overflowY: "auto" as const,
  } satisfies React.CSSProperties,

  gridRow: {
    display:              "grid",
    gridTemplateColumns:  "1fr 1fr",
    gap:                  "16px",
    padding:              "8px 0",
    borderBottom:         `1px solid ${C.surface0}`,
    alignItems:           "start",
  } satisfies React.CSSProperties,

  sourceCell: {
    display:       "flex",
    flexDirection: "column" as const,
    gap:           "2px",
  } satisfies React.CSSProperties,

  keyLabel: {
    fontFamily:   Font.mono,
    fontSize:     "0.7rem",
    color:        C.muted,
    marginBottom: "2px",
  } satisfies React.CSSProperties,

  sourceText: {
    fontSize:  "0.8125rem",
    color:     C.subtext,
    wordBreak: "break-word" as const,
  } satisfies React.CSSProperties,

  targetCell: {
    position: "relative" as const,
  } satisfies React.CSSProperties,

  targetCellMissing: {
    // Handled via inline styles on textarea
  } satisfies React.CSSProperties,

  missingHint: {
    position:  "absolute" as const,
    top:       "4px",
    right:     "6px",
    fontSize:  "0.65rem",
    color:     C.yellow,
    pointerEvents: "none" as const,
  } satisfies React.CSSProperties,
} as const;
