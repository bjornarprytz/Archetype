/**
 * KeywordEditorPanel — keyword list + detail editor (Task 6.2, D26).
 *
 * Left sidebar: scrollable list of keywords, "Create new" action at top.
 * Right panel: detail for selected keyword — name, parameters, DSL body,
 *              text template, signal behaviour checkbox.
 */
import React, { useEffect, useState, useCallback } from "react";
import { IPC_CHANNELS } from "@shared/ipc";
import type { MutationResponse } from "@shared/ipc";
import { useDiagnosticsStore } from "../store/diagnosticsStore";
import { useProjectStore } from "../store/projectStore";
import { fetchSnapshot } from "../snapshot";
import type { KeywordSnapshot, ParameterSpec, MarkerData } from "../snapshot";
import { diagnosticToMarker } from "../snapshot";
import { DslEditor } from "../components/DslEditor";
import { C, Font, S } from "../design/tokens";

const PARAM_TYPES = ["Atom", "Int", "Bool", "String", "Player", "Zone"] as const;

// TypeName enum values from the domain model (D27).
const RETURN_TYPES = [
  "Atom", "Card", "Zone", "Player", "Session", "Number", "Boolean",
  "ConditionName", "PropertyName", "ContributionId", "Lifetime",
  "EffectBlock", "CardDefinitionName",
] as const;

export function KeywordEditorPanel(): React.ReactElement {
  const [keywords,  setKeywords]  = useState<KeywordSnapshot[]>([]);
  const [selected,  setSelected]  = useState<string | null>(null);
  const [loading,   setLoading]   = useState(false);
  const [newName,   setNewName]   = useState("");
  const [creating,  setCreating]  = useState(false);

  const updateCounts = useDiagnosticsStore((s) => s.updateCounts);
  const markDirty    = useProjectStore((s) => s.markDirty);

  // Load on mount
  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    fetchSnapshot().then((snap) => {
      if (cancelled) return;
      setKeywords((snap?.keywords ?? []) as KeywordSnapshot[]);
      setLoading(false);
    });
    return () => { cancelled = true; };
  }, []);

  const selectedKw = keywords.find((k) => k.name === selected) ?? null;

  async function handleCreate(): Promise<void> {
    const name = newName.trim();
    if (!name) return;
    setCreating(true);
    try {
      const resp = await window.archetype.invoke<MutationResponse>(IPC_CHANNELS.AddEntry, {
        entryKind: "Keyword",
        entryName: name,
      });
      markDirty();
      updateCounts(resp.globalErrorCount, resp.globalWarningCount);
      const snap = await fetchSnapshot();
      setKeywords((snap?.keywords ?? []) as KeywordSnapshot[]);
      setSelected(name);
      setNewName("");
    } finally {
      setCreating(false);
    }
  }

  async function handleDelete(name: string): Promise<void> {
    const resp = await window.archetype.invoke<MutationResponse>(IPC_CHANNELS.RemoveEntry, {
      entryKind: "Keyword",
      entryName: name,
    });
    markDirty();
    updateCounts(resp.globalErrorCount, resp.globalWarningCount);
    setKeywords((prev) => prev.filter((k) => k.name !== name));
    if (selected === name) setSelected(null);
  }

  async function handleRename(oldName: string, newNameStr: string): Promise<void> {
    const resp = await window.archetype.invoke<MutationResponse>(IPC_CHANNELS.RenameEntry, {
      entryKind: "Keyword",
      oldName,
      newName: newNameStr,
    });
    markDirty();
    updateCounts(resp.globalErrorCount, resp.globalWarningCount);
    const snap = await fetchSnapshot();
    setKeywords((snap?.keywords ?? []) as KeywordSnapshot[]);
    setSelected(newNameStr);
  }

  return (
    <div style={styles.root}>
      {/* Sidebar */}
      <aside style={styles.sidebar}>
        <h2 style={S.panelTitle}>Keywords</h2>

        {/* Create new */}
        <div style={styles.createRow}>
          <input
            style={{ ...S.input, flex: 1 }}
            placeholder="new keyword…"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            onKeyDown={(e) => { if (e.key === "Enter") { void handleCreate(); } }}
            aria-label="New keyword name"
          />
          <button
            style={S.btnPrimary}
            onClick={() => { void handleCreate(); }}
            disabled={creating || !newName.trim()}
            aria-label="Create keyword"
          >
            +
          </button>
        </div>

        {loading && <div style={styles.emptyMsg}>Loading…</div>}

        {/* Keyword list */}
        <ul style={styles.list} role="listbox" aria-label="Keyword list">
          {keywords.map((kw) => (
            <li
              key={kw.name}
              role="option"
              aria-selected={kw.name === selected}
              style={{
                ...styles.listItem,
                ...(kw.name === selected ? styles.listItemActive : {}),
              }}
              onClick={() => setSelected(kw.name)}
              title={kw.name}
            >
              <span style={styles.kwName}>{kw.name}</span>
              <button
                style={{ ...S.btnIcon, marginLeft: "auto" }}
                onClick={(e) => { e.stopPropagation(); void handleDelete(kw.name); }}
                aria-label={`Delete ${kw.name}`}
                title="Delete"
              >
                ×
              </button>
            </li>
          ))}
        </ul>
      </aside>

      {/* Detail */}
      <main style={styles.detail}>
        {selectedKw
          ? <KeywordDetail
              key={selectedKw.name}
              keyword={selectedKw}
              onRename={handleRename}
              onRefresh={async () => {
                const snap = await fetchSnapshot();
                setKeywords((snap?.keywords ?? []) as KeywordSnapshot[]);
              }}
            />
          : <div style={styles.emptyDetail}>Select or create a keyword.</div>
        }
      </main>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Keyword detail view
// ---------------------------------------------------------------------------

interface KeywordDetailProps {
  keyword: KeywordSnapshot;
  onRename: (old: string, newName: string) => Promise<void>;
  onRefresh: () => Promise<void>;
}

function KeywordDetail({ keyword, onRename, onRefresh }: KeywordDetailProps): React.ReactElement {
  const [nameEdit,    setNameEdit]    = useState(keyword.name);
  const [template,   setTemplate]    = useState(keyword.textTemplate);
  const [noSignal,   setNoSignal]    = useState(keyword.noSignal);
  const [params,     setParams]      = useState<ParameterSpec[]>([...keyword.parameters]);
  const [bodyValue,  setBodyValue]   = useState(keyword.body);
  const [returnType, setReturnType]  = useState<string>(keyword.returnType ?? "");
  const [markers,    setMarkers]     = useState<MarkerData[]>([]);

  const updateCounts = useDiagnosticsStore((s) => s.updateCounts);
  const markDirty    = useProjectStore((s) => s.markDirty);

  async function commitField(field: string, value: unknown): Promise<void> {
    const resp = await window.archetype.invoke<MutationResponse>(IPC_CHANNELS.UpdateField, {
      entryKind: "Keyword",
      entryName: keyword.name,
      field,
      value,
    });
    markDirty();
    updateCounts(resp.globalErrorCount, resp.globalWarningCount);
  }

  async function handleNameBlur(): Promise<void> {
    const trimmed = nameEdit.trim();
    if (trimmed && trimmed !== keyword.name) await onRename(keyword.name, trimmed);
  }

  async function addParam(): Promise<void> {
    const updated = [...params, { name: "param", type: "Int" }];
    setParams(updated);
    await commitField("parameters", updated);
  }

  async function removeParam(i: number): Promise<void> {
    const updated = params.filter((_, idx) => idx !== i);
    setParams(updated);
    await commitField("parameters", updated);
  }

  async function updateParam(i: number, field: "name" | "type", val: string): Promise<void> {
    const updated = params.map((p, idx) => idx === i ? { ...p, [field]: val } : p);
    setParams(updated);
    await commitField("parameters", updated);
  }

  return (
    <div style={styles.detailInner}>
      {/* Name */}
      <section style={styles.section}>
        <label style={S.sectionLabel} htmlFor="kw-name">Name</label>
        <input
          id="kw-name"
          style={S.input}
          value={nameEdit}
          onChange={(e) => setNameEdit(e.target.value)}
          onBlur={() => { void handleNameBlur(); }}
          onKeyDown={(e) => { if (e.key === "Enter") { void handleNameBlur(); } }}
        />
      </section>

      {/* Return type */}
      <section style={styles.section}>
        <label style={S.sectionLabel} htmlFor="kw-return-type">Return Type</label>
        <select
          id="kw-return-type"
          style={S.select}
          value={returnType}
          onChange={(e) => {
            setReturnType(e.target.value);
            void commitField("returnType", e.target.value || null);
          }}
          aria-label="Return type"
        >
          {/* Empty option lets the user leave return type unset (validator will flag it). */}
          <option value="">— unset —</option>
          {RETURN_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
        </select>
      </section>

      {/* Parameters */}
      <section style={styles.section}>
        <div style={{ display: "flex", alignItems: "center", marginBottom: "6px" }}>
          <span style={S.sectionLabel}>Parameters</span>
          <button style={{ ...S.btnGhost, marginLeft: "auto", fontSize: "0.7rem" }} onClick={() => { void addParam(); }}>
            + Add
          </button>
        </div>
        {params.map((p, i) => (
          <div key={i} style={styles.paramRow}>
            <input
              style={{ ...S.input, flex: 1 }}
              value={p.name}
              // D28: non-DSL fields commit on blur/Enter, not on every keystroke,
              // to avoid firing UpdateField + full re-validation per keypress.
              onChange={(e) => setParams((prev) =>
                prev.map((pp, idx) => idx === i ? { ...pp, name: e.target.value } : pp)
              )}
              onBlur={(e) => { void updateParam(i, "name", e.target.value); }}
              onKeyDown={(e) => { if (e.key === "Enter") { void updateParam(i, "name", (e.target as HTMLInputElement).value); } }}
              aria-label={`Parameter ${i + 1} name`}
            />
            <select
              style={S.select}
              value={p.type}
              onChange={(e) => { void updateParam(i, "type", e.target.value); }}
              aria-label={`Parameter ${i + 1} type`}
            >
              {PARAM_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
            </select>
            <button
              style={S.btnIcon}
              onClick={() => { void removeParam(i); }}
              aria-label={`Remove parameter ${p.name}`}
            >
              ×
            </button>
          </div>
        ))}
        {params.length === 0 && (
          <span style={{ color: C.muted, fontSize: "0.8rem" }}>No parameters.</span>
        )}
      </section>

      {/* Body */}
      <section style={styles.section}>
        <label style={S.sectionLabel}>Body</label>
        <DslEditor
          value={bodyValue}
          entryKind="Keyword"
          entryName={keyword.name}
          dslField="body"
          markers={markers}
          onChange={setBodyValue}
          onMarkersChange={setMarkers}
          onAfterCommit={onRefresh}
          height="200px"
        />
      </section>

      {/* Text template */}
      <section style={styles.section}>
        <label style={S.sectionLabel} htmlFor="kw-template">Text Template</label>
        <input
          id="kw-template"
          style={S.input}
          value={template}
          onChange={(e) => setTemplate(e.target.value)}
          onBlur={() => { void commitField("textTemplate", template); }}
          placeholder="Deal {amount} damage to {atom}"
        />
      </section>

      {/* Signal */}
      <section style={styles.section}>
        <label style={styles.checkLabel}>
          <input
            type="checkbox"
            checked={noSignal}
            onChange={(e) => {
              setNoSignal(e.target.checked);
              void commitField("noSignal", e.target.checked);
            }}
            aria-label="No signal"
          />
          <span style={{ marginLeft: "8px" }}>
            <code style={{ fontFamily: Font.mono, color: C.mauve }}>[NoSignal]</code>
            {" "}— suppress Godot signal for this keyword
          </span>
        </label>
      </section>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Styles
// ---------------------------------------------------------------------------

const styles = {
  root: {
    display:    "flex",
    height:     "100%",
    overflow:   "hidden",
    fontFamily: Font.ui,
    color:      C.text,
  } satisfies React.CSSProperties,

  sidebar: {
    width:        "200px",
    flexShrink:   0,
    borderRight:  `1px solid ${C.surface0}`,
    display:      "flex",
    flexDirection: "column" as const,
    padding:      "16px 0 0",
    overflow:     "hidden",
  } satisfies React.CSSProperties,

  createRow: {
    display:      "flex",
    gap:          "6px",
    padding:      "0 10px 10px",
    borderBottom: `1px solid ${C.surface0}`,
  } satisfies React.CSSProperties,

  list: {
    listStyle:  "none",
    flex:       1,
    overflowY:  "auto" as const,
    margin:     0,
    padding:    0,
  } satisfies React.CSSProperties,

  listItem: {
    display:      "flex",
    alignItems:   "center",
    padding:      "6px 10px",
    cursor:       "pointer",
    borderBottom: `1px solid ${C.surface0}`,
    fontSize:     "0.8125rem",
    userSelect:   "none" as const,
  } satisfies React.CSSProperties,

  listItemActive: {
    background:  C.base,
    borderLeft:  `2px solid ${C.accent}`,
    paddingLeft: "8px",
    color:       C.accent,
  } satisfies React.CSSProperties,

  kwName: {
    fontFamily:   Font.mono,
    overflow:     "hidden",
    textOverflow: "ellipsis",
    whiteSpace:   "nowrap" as const,
    fontSize:     "0.8rem",
  } satisfies React.CSSProperties,

  detail: {
    flex:      1,
    overflowY: "auto" as const,
    padding:   "16px 24px",
  } satisfies React.CSSProperties,

  detailInner: {
    maxWidth: "640px",
  } satisfies React.CSSProperties,

  emptyDetail: {
    color:    C.muted,
    fontSize: "0.8125rem",
    padding:  "16px 0",
  } satisfies React.CSSProperties,

  emptyMsg: {
    color:   C.muted,
    padding: "12px 10px",
    fontSize: "0.8125rem",
  } satisfies React.CSSProperties,

  section: {
    marginBottom: "20px",
  } satisfies React.CSSProperties,

  paramRow: {
    display:      "flex",
    gap:          "8px",
    alignItems:   "center",
    marginBottom: "6px",
  } satisfies React.CSSProperties,

  checkLabel: {
    display:     "flex",
    alignItems:  "center",
    cursor:      "pointer",
    fontSize:    "0.8125rem",
    color:       C.subtext,
    userSelect:  "none" as const,
  } satisfies React.CSSProperties,
} as const;
