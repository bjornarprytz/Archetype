/**
 * GameRulesPanel — phases, action rules, state-based rules, trigger order (Task 6.4, D26).
 *
 * Phases: ordered list with drag handles; per-phase init/cleanup DslEditor.
 * Action rules: accordion by action type; per-rule before/after DslEditor; reorderable.
 * State-based rules: ordered list with drag handles; condition + body DslEditor.
 * Trigger resolution order: radio group.
 */
import React, { useEffect, useState } from "react";
import { IPC_CHANNELS } from "@shared/ipc";
import type { MutationResponse } from "@shared/ipc";
import { useDiagnosticsStore } from "../store/diagnosticsStore";
import { useProjectStore } from "../store/projectStore";
import { fetchSnapshot } from "../snapshot";
import type { PhaseSnapshot, ActionRuleSnapshot, StateBasedRuleSnapshot } from "../snapshot";
import { DslEditor } from "../components/DslEditor";
import { C, Font, S } from "../design/tokens";

const TRIGGER_ORDERS = ["Chronological", "ReverseChronological", "Simultaneous"] as const;

export function GameRulesPanel(): React.ReactElement {
  const [phases,        setPhases]        = useState<PhaseSnapshot[]>([]);
  const [actionRules,   setActionRules]   = useState<ActionRuleSnapshot[]>([]);
  const [stateRules,    setStateRules]    = useState<StateBasedRuleSnapshot[]>([]);
  const [triggerOrder,  setTriggerOrder]  = useState("Chronological");
  const [loading,       setLoading]       = useState(false);
  const [activeSection, setActiveSection] = useState<"phases" | "actionRules" | "stateRules" | "trigger">("phases");
  const [dragIdx,       setDragIdx]       = useState<number | null>(null);

  const updateCounts = useDiagnosticsStore((s) => s.updateCounts);
  const markDirty    = useProjectStore((s) => s.markDirty);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    fetchSnapshot().then((snap) => {
      if (cancelled) return;
      setPhases((snap?.phases ?? []) as PhaseSnapshot[]);
      setActionRules((snap?.actionRules ?? []) as ActionRuleSnapshot[]);
      setStateRules((snap?.stateBasedRules ?? []) as StateBasedRuleSnapshot[]);
      setTriggerOrder(snap?.triggerResolutionOrder ?? "Chronological");
      setLoading(false);
    });
    return () => { cancelled = true; };
  }, []);

  async function commitGameRules(
    updatedPhases: PhaseSnapshot[],
    updatedActionRules: ActionRuleSnapshot[],
    updatedStateRules: StateBasedRuleSnapshot[],
    updatedTrigger: string
  ): Promise<void> {
    const resp = await window.archetype.invoke<MutationResponse>(IPC_CHANNELS.UpdateField, {
      entryKind: "GameRules",
      entryName: "gameRules",
      field: "rules",
      value: {
        phases: updatedPhases,
        actionRules: updatedActionRules,
        stateBasedRules: updatedStateRules,
        triggerResolutionOrder: updatedTrigger,
      },
    });
    markDirty();
    updateCounts(resp.globalErrorCount, resp.globalWarningCount);
  }

  // Drag-and-drop helpers
  function handleDragStart(i: number): void { setDragIdx(i); }
  function handleDragOver(e: React.DragEvent): void { e.preventDefault(); }

  function handlePhaseDrop(targetIdx: number): void {
    if (dragIdx === null || dragIdx === targetIdx) { setDragIdx(null); return; }
    const next = [...phases];
    const [item] = next.splice(dragIdx, 1);
    if (item) next.splice(targetIdx, 0, item);
    setPhases(next);
    void commitGameRules(next, actionRules, stateRules, triggerOrder);
    setDragIdx(null);
  }

  function handleStateRuleDrop(targetIdx: number): void {
    if (dragIdx === null || dragIdx === targetIdx) { setDragIdx(null); return; }
    const next = [...stateRules];
    const [item] = next.splice(dragIdx, 1);
    if (item) next.splice(targetIdx, 0, item);
    setStateRules(next);
    void commitGameRules(phases, actionRules, next, triggerOrder);
    setDragIdx(null);
  }

  const tabs: Array<{ id: typeof activeSection; label: string }> = [
    { id: "phases",      label: "Phases" },
    { id: "actionRules", label: "Action Rules" },
    { id: "stateRules",  label: "State Rules" },
    { id: "trigger",     label: "Trigger Order" },
  ];

  return (
    <div style={styles.root}>
      <h2 style={S.panelTitle}>Game Rules</h2>

      {/* Tab bar */}
      <div style={styles.tabs} role="tablist">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            role="tab"
            aria-selected={activeSection === tab.id}
            style={{
              ...styles.tab,
              ...(activeSection === tab.id ? styles.tabActive : {}),
            }}
            onClick={() => setActiveSection(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {loading && <div style={styles.loading}>Loading…</div>}

      {/* Phases */}
      {!loading && activeSection === "phases" && (
        <div style={styles.scrollPane}>
          {phases.map((phase, i) => (
            <PhaseRow
              key={`${phase.name}-${i}`}
              phase={phase}
              onDragStart={() => handleDragStart(i)}
              onDragOver={handleDragOver}
              onDrop={() => handlePhaseDrop(i)}
              isDragging={dragIdx === i}
            />
          ))}
          {phases.length === 0 && <div style={styles.empty}>No phases defined.</div>}
        </div>
      )}

      {/* Action Rules */}
      {!loading && activeSection === "actionRules" && (
        <div style={styles.scrollPane}>
          <ActionRulesSection
            rules={actionRules}
            setRules={setActionRules}
            onCommit={(updated) => void commitGameRules(phases, updated, stateRules, triggerOrder)}
          />
        </div>
      )}

      {/* State-based Rules */}
      {!loading && activeSection === "stateRules" && (
        <div style={styles.scrollPane}>
          {stateRules.map((rule, i) => (
            <StateRuleRow
              key={i}
              rule={rule}
              onDragStart={() => handleDragStart(i)}
              onDragOver={handleDragOver}
              onDrop={() => handleStateRuleDrop(i)}
              isDragging={dragIdx === i}
            />
          ))}
          {stateRules.length === 0 && <div style={styles.empty}>No state-based rules defined.</div>}
        </div>
      )}

      {/* Trigger Order */}
      {!loading && activeSection === "trigger" && (
        <div style={styles.triggerSection}>
          <span style={S.sectionLabel}>Trigger Resolution Order</span>
          {TRIGGER_ORDERS.map((order) => (
            <label key={order} style={styles.radioLabel}>
              <input
                type="radio"
                name="triggerOrder"
                value={order}
                checked={triggerOrder === order}
                onChange={() => {
                  setTriggerOrder(order);
                  void commitGameRules(phases, actionRules, stateRules, order);
                }}
              />
              <span style={{ marginLeft: "8px" }}>{order}</span>
            </label>
          ))}
        </div>
      )}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Phase row
// ---------------------------------------------------------------------------

function PhaseRow({
  phase, onDragStart, onDragOver, onDrop, isDragging,
}: {
  phase: PhaseSnapshot;
  onDragStart: () => void;
  onDragOver: (e: React.DragEvent) => void;
  onDrop: () => void;
  isDragging: boolean;
}): React.ReactElement {
  const [open, setOpen] = useState(false);
  return (
    <div
      draggable
      onDragStart={onDragStart}
      onDragOver={onDragOver}
      onDrop={onDrop}
      style={{ ...styles.draggableRow, opacity: isDragging ? 0.4 : 1 }}
      aria-label={`Phase: ${phase.name}`}
    >
      <span style={S.dragHandle} title="Drag to reorder">⠿</span>
      <div style={{ flex: 1 }}>
        <button style={styles.phaseHeader} onClick={() => setOpen((o) => !o)} aria-expanded={open}>
          <span style={{ color: C.teal, fontFamily: Font.mono, fontSize: "0.8rem" }}>{phase.name}</span>
          <span style={{ marginLeft: "auto", color: C.muted, fontSize: "0.7rem" }}>{open ? "▲" : "▼"}</span>
        </button>
        {open && (
          <div style={styles.phaseBody}>
            <label style={S.sectionLabel}>Init</label>
            <DslEditor value={phase.initDsl} entryKind="Phase" entryName={phase.name} dslField="initDsl" height="80px" />
            <label style={{ ...S.sectionLabel, marginTop: "8px" }}>Cleanup</label>
            <DslEditor value={phase.cleanupDsl} entryKind="Phase" entryName={phase.name} dslField="cleanupDsl" height="80px" />
          </div>
        )}
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Action rules section
// ---------------------------------------------------------------------------

function ActionRulesSection({
  rules, setRules, onCommit,
}: {
  rules: ActionRuleSnapshot[];
  setRules: React.Dispatch<React.SetStateAction<ActionRuleSnapshot[]>>;
  onCommit: (updated: ActionRuleSnapshot[]) => void;
}): React.ReactElement {
  const byType = rules.reduce<Record<string, ActionRuleSnapshot[]>>((acc, r) => {
    const arr = acc[r.actionType] ?? [];
    arr.push(r);
    acc[r.actionType] = arr;
    return acc;
  }, {});

  return (
    <>
      {Object.entries(byType).map(([type, typeRules]) => (
        <div key={type} style={styles.actionTypeGroup}>
          <div style={styles.actionTypeHeader}>{type}</div>
          {typeRules.map((rule, i) => (
            <div key={i} style={styles.actionRuleRow}>
              <div style={{ marginBottom: "8px" }}>
                <label style={S.sectionLabel}>Before</label>
                <DslEditor value={rule.beforeDsl} entryKind="ActionRule" entryName={type} dslField={`before_${i}`} height="80px" />
              </div>
              <div>
                <label style={S.sectionLabel}>After</label>
                <DslEditor value={rule.afterDsl} entryKind="ActionRule" entryName={type} dslField={`after_${i}`} height="80px" />
              </div>
            </div>
          ))}
        </div>
      ))}
      {rules.length === 0 && <div style={styles.empty}>No action rules defined.</div>}
    </>
  );
}

// ---------------------------------------------------------------------------
// State rule row
// ---------------------------------------------------------------------------

function StateRuleRow({
  rule, onDragStart, onDragOver, onDrop, isDragging,
}: {
  rule: StateBasedRuleSnapshot;
  onDragStart: () => void;
  onDragOver: (e: React.DragEvent) => void;
  onDrop: () => void;
  isDragging: boolean;
}): React.ReactElement {
  const [open, setOpen] = useState(false);
  const preview = rule.conditionDsl.slice(0, 40) + (rule.conditionDsl.length > 40 ? "…" : "");
  return (
    <div
      draggable
      onDragStart={onDragStart}
      onDragOver={onDragOver}
      onDrop={onDrop}
      style={{ ...styles.draggableRow, opacity: isDragging ? 0.4 : 1 }}
    >
      <span style={S.dragHandle}>⠿</span>
      <div style={{ flex: 1 }}>
        <button style={styles.phaseHeader} onClick={() => setOpen((o) => !o)} aria-expanded={open}>
          <span style={{ fontFamily: Font.mono, fontSize: "0.8rem", color: C.peach }}>{preview || "(empty)"}</span>
          <span style={{ marginLeft: "auto", color: C.muted, fontSize: "0.7rem" }}>{open ? "▲" : "▼"}</span>
        </button>
        {open && (
          <div style={styles.phaseBody}>
            <label style={S.sectionLabel}>Condition</label>
            <DslEditor value={rule.conditionDsl} entryKind="StateBasedRule" entryName="rule" dslField="conditionDsl" height="80px" />
            <label style={{ ...S.sectionLabel, marginTop: "8px" }}>Body</label>
            <DslEditor value={rule.bodyDsl} entryKind="StateBasedRule" entryName="rule" dslField="bodyDsl" height="80px" />
          </div>
        )}
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Styles
// ---------------------------------------------------------------------------

const styles = {
  root: {
    display:       "flex",
    flexDirection: "column" as const,
    height:        "100%",
    overflow:      "hidden",
    fontFamily:    Font.ui,
    color:         C.text,
  } satisfies React.CSSProperties,

  tabs: {
    display:      "flex",
    borderBottom: `1px solid ${C.surface0}`,
    marginBottom: "16px",
    gap:          "2px",
  } satisfies React.CSSProperties,

  tab: {
    fontFamily:   Font.ui,
    fontSize:     "0.8125rem",
    padding:      "7px 16px",
    border:       "none",
    background:   "transparent",
    color:        C.subtext,
    cursor:       "pointer",
    borderBottom: "2px solid transparent",
  } satisfies React.CSSProperties,

  tabActive: {
    color:        C.accent,
    borderBottom: `2px solid ${C.accent}`,
  } satisfies React.CSSProperties,

  scrollPane: {
    flex:      1,
    overflowY: "auto" as const,
  } satisfies React.CSSProperties,

  loading: {
    color:   C.muted,
    padding: "12px 0",
    fontSize: "0.8125rem",
  } satisfies React.CSSProperties,

  empty: {
    color:   C.muted,
    padding: "16px 0",
    fontSize: "0.8125rem",
  } satisfies React.CSSProperties,

  draggableRow: {
    display:      "flex",
    alignItems:   "flex-start",
    gap:          "8px",
    borderBottom: `1px solid ${C.surface0}`,
    padding:      "8px 0",
  } satisfies React.CSSProperties,

  phaseHeader: {
    display:    "flex",
    alignItems: "center",
    width:      "100%",
    background: "transparent",
    border:     "none",
    cursor:     "pointer",
    padding:    "4px 0",
    fontFamily: Font.ui,
    color:      C.text,
    fontSize:   "0.8125rem",
  } satisfies React.CSSProperties,

  phaseBody: {
    paddingTop: "12px",
  } satisfies React.CSSProperties,

  actionTypeGroup: {
    marginBottom: "20px",
  } satisfies React.CSSProperties,

  actionTypeHeader: {
    fontSize:      "0.7rem",
    fontWeight:    700,
    letterSpacing: "0.08em",
    textTransform: "uppercase" as const,
    color:         C.peach,
    padding:       "6px 0",
    borderBottom:  `1px solid ${C.surface0}`,
    marginBottom:  "10px",
  } satisfies React.CSSProperties,

  actionRuleRow: {
    padding:      "8px 0 12px",
    borderBottom: `1px solid ${C.surface0}`,
    marginBottom: "8px",
  } satisfies React.CSSProperties,

  triggerSection: {
    padding:    "8px 0",
    maxWidth:   "320px",
  } satisfies React.CSSProperties,

  radioLabel: {
    display:     "flex",
    alignItems:  "center",
    fontSize:    "0.8125rem",
    padding:     "6px 0",
    cursor:      "pointer",
    color:       C.text,
  } satisfies React.CSSProperties,
} as const;
