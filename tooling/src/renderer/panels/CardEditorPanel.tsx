/**
 * CardEditorPanel — card list + full card detail editor (Task 6.3, D26, D30).
 *
 * Sidebar: card list + create action.
 * Detail: name, static properties, primary effect (DslEditor), additional
 *         effects accordion, static effects, activation condition, costs,
 *         flavour text, art picker + crop region, card text preview.
 */
import React, { useEffect, useState } from "react";
import { IPC_CHANNELS } from "@shared/ipc";
import type { MutationResponse, RenderCardTextResult } from "@shared/ipc";
import { useDiagnosticsStore } from "../store/diagnosticsStore";
import { useProjectStore } from "../store/projectStore";
import { fetchSnapshot } from "../snapshot";
import type { CardSnapshot, MarkerData } from "../snapshot";
import { diagnosticToMarker } from "../snapshot";
import { DslEditor } from "../components/DslEditor";
import { C, Font, S } from "../design/tokens";

export function CardEditorPanel(): React.ReactElement {
  const [cards,    setCards]    = useState<CardSnapshot[]>([]);
  const [selected, setSelected] = useState<string | null>(null);
  const [loading,  setLoading]  = useState(false);
  const [newName,  setNewName]  = useState("");
  const [creating, setCreating] = useState(false);

  const updateCounts = useDiagnosticsStore((s) => s.updateCounts);
  const markDirty    = useProjectStore((s) => s.markDirty);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    fetchSnapshot().then((snap) => {
      if (cancelled) return;
      setCards((snap?.cards ?? []) as CardSnapshot[]);
      setLoading(false);
    });
    return () => { cancelled = true; };
  }, []);

  const selectedCard = cards.find((c) => c.name === selected) ?? null;

  async function handleCreate(): Promise<void> {
    const name = newName.trim();
    if (!name) return;
    setCreating(true);
    try {
      const resp = await window.archetype.invoke<MutationResponse>(IPC_CHANNELS.AddEntry, {
        entryKind: "Card", entryName: name,
      });
      markDirty();
      updateCounts(resp.globalErrorCount, resp.globalWarningCount);
      const snap = await fetchSnapshot();
      setCards((snap?.cards ?? []) as CardSnapshot[]);
      setSelected(name);
      setNewName("");
    } finally {
      setCreating(false);
    }
  }

  async function handleDelete(name: string): Promise<void> {
    const resp = await window.archetype.invoke<MutationResponse>(IPC_CHANNELS.RemoveEntry, {
      entryKind: "Card", entryName: name,
    });
    markDirty();
    updateCounts(resp.globalErrorCount, resp.globalWarningCount);
    setCards((prev) => prev.filter((c) => c.name !== name));
    if (selected === name) setSelected(null);
  }

  return (
    <div style={styles.root}>
      {/* Sidebar */}
      <aside style={styles.sidebar}>
        <h2 style={S.panelTitle}>Cards</h2>

        <div style={styles.createRow}>
          <input
            style={{ ...S.input, flex: 1 }}
            placeholder="new card…"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            onKeyDown={(e) => { if (e.key === "Enter") { void handleCreate(); } }}
            aria-label="New card name"
          />
          <button
            style={S.btnPrimary}
            onClick={() => { void handleCreate(); }}
            disabled={creating || !newName.trim()}
            aria-label="Create card"
          >
            +
          </button>
        </div>

        {loading && <div style={styles.empty}>Loading…</div>}

        <ul style={styles.list} role="listbox" aria-label="Card list">
          {cards.map((card) => (
            <li
              key={card.name}
              role="option"
              aria-selected={card.name === selected}
              style={{
                ...styles.listItem,
                ...(card.name === selected ? styles.listItemActive : {}),
              }}
              onClick={() => setSelected(card.name)}
            >
              <span style={styles.cardName}>{card.name}</span>
              <button
                style={{ ...S.btnIcon, marginLeft: "auto" }}
                onClick={(e) => { e.stopPropagation(); void handleDelete(card.name); }}
                aria-label={`Delete ${card.name}`}
              >
                ×
              </button>
            </li>
          ))}
        </ul>
      </aside>

      {/* Detail */}
      <main style={styles.detail}>
        {selectedCard
          ? <CardDetail
              key={selectedCard.name}
              card={selectedCard}
              onRefresh={async () => {
                const snap = await fetchSnapshot();
                setCards((snap?.cards ?? []) as CardSnapshot[]);
              }}
            />
          : <div style={styles.emptyDetail}>Select or create a card.</div>
        }
      </main>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Card detail
// ---------------------------------------------------------------------------

function CardDetail({
  card,
  onRefresh,
}: {
  card: CardSnapshot;
  onRefresh: () => Promise<void>;
}): React.ReactElement {
  const [name,        setName]        = useState(card.name);
  const [flavour,     setFlavour]     = useState(card.flavourText);
  const [artPath,     setArtPath]     = useState(card.artPath ?? "");
  const [cropX,       setCropX]       = useState(0);
  const [cropY,       setCropY]       = useState(0);
  const [cropW,       setCropW]       = useState(100);
  const [cropH,       setCropH]       = useState(100);
  const [primaryDsl,  setPrimaryDsl]  = useState(card.primaryEffect?.dsl ?? "");
  const [condDsl,     setCondDsl]     = useState(card.activationCondition ?? "");
  const [openSections, setOpenSections] = useState<Set<string>>(new Set(["primary"]));
  const [renderTree,  setRenderTree]  = useState<unknown>(null);
  const [primaryMarkers, setPrimaryMarkers] = useState<MarkerData[]>([]);
  const [condMarkers,    setCondMarkers]    = useState<MarkerData[]>([]);

  const updateCounts = useDiagnosticsStore((s) => s.updateCounts);
  const markDirty    = useProjectStore((s) => s.markDirty);

  function toggleSection(id: string): void {
    setOpenSections((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }

  async function commitField(field: string, value: unknown): Promise<void> {
    const resp = await window.archetype.invoke<MutationResponse>(IPC_CHANNELS.UpdateField, {
      entryKind: "Card", entryName: card.name, field, value,
    });
    markDirty();
    updateCounts(resp.globalErrorCount, resp.globalWarningCount);
  }

  async function handlePickArt(): Promise<void> {
    const result = await window.archetype.invoke<{ filePaths?: string[] }>(
      IPC_CHANNELS.ShowOpenDialog as "File:ShowOpenDialog",
      {
        title: "Select Art",
        filters: [{ name: "Images", extensions: ["png", "jpg", "jpeg", "webp"] }],
        properties: ["openFile"],
      }
    );
    const path = result?.filePaths?.[0];
    if (path) {
      setArtPath(path);
      await commitField("artPath", path);
    }
  }

  async function refreshPreview(): Promise<void> {
    try {
      const result = await window.archetype.invoke<RenderCardTextResult>(
        IPC_CHANNELS.RenderCardText,
        { cardName: card.name, locale: null }
      );
      setRenderTree(result?.renderTree ?? null);
    } catch { /* ignore */ }
  }

  return (
    <div style={styles.detailInner}>
      {/* Name */}
      <div style={styles.row2col}>
        <section style={{ flex: 1 }}>
          <label style={S.sectionLabel} htmlFor="card-name">Card Name</label>
          <input
            id="card-name"
            style={S.input}
            value={name}
            onChange={(e) => setName(e.target.value)}
            onBlur={() => { void commitField("name", name); }}
          />
        </section>
      </div>

      {/* Primary effect */}
      <Accordion
        id="primary"
        label="Primary Effect"
        open={openSections.has("primary")}
        onToggle={() => toggleSection("primary")}
      >
        <DslEditor
          value={primaryDsl}
          entryKind="Card"
          entryName={card.name}
          dslField="primaryEffect"
          markers={primaryMarkers}
          onChange={setPrimaryDsl}
          onMarkersChange={setPrimaryMarkers}
          height="160px"
        />
      </Accordion>

      {/* Additional effects */}
      {card.additionalEffects.map((eff, i) => (
        <Accordion
          key={eff.name ?? i}
          id={`eff-${i}`}
          label={eff.name ?? `Effect ${i + 1}`}
          open={openSections.has(`eff-${i}`)}
          onToggle={() => toggleSection(`eff-${i}`)}
        >
          <DslEditor
            value={eff.dsl}
            entryKind="Card"
            entryName={card.name}
            dslField={eff.name ?? `effect_${i}`}
            height="140px"
          />
        </Accordion>
      ))}

      {/* Static effects */}
      {card.staticEffects.length > 0 && (
        <Accordion
          id="static"
          label="Static Effects"
          open={openSections.has("static")}
          onToggle={() => toggleSection("static")}
        >
          {card.staticEffects.map((se, i) => (
            <div key={i} style={styles.staticEffectBlock}>
              <label style={S.sectionLabel}>Lifetime</label>
              <DslEditor
                value={se.lifetimeDsl}
                entryKind="Card"
                entryName={card.name}
                dslField={`lifetime:${i}`}
                height="80px"
              />
              <label style={{ ...S.sectionLabel, marginTop: "8px" }}>Effect</label>
              <DslEditor
                value={se.effectDsl}
                entryKind="Card"
                entryName={card.name}
                dslField={`staticEffect_${i}`}
                height="80px"
              />
            </div>
          ))}
        </Accordion>
      )}

      {/* Activation condition */}
      <Accordion
        id="cond"
        label="Activation Condition"
        open={openSections.has("cond")}
        onToggle={() => toggleSection("cond")}
      >
        <DslEditor
          value={condDsl}
          entryKind="Card"
          entryName={card.name}
          dslField="activationCondition"
          markers={condMarkers}
          onChange={setCondDsl}
          onMarkersChange={setCondMarkers}
          height="100px"
        />
      </Accordion>

      {/* Costs */}
      <Accordion
        id="costs"
        label="Costs"
        open={openSections.has("costs")}
        onToggle={() => toggleSection("costs")}
      >
        {card.costs.map((cost, i) => (
          <div key={i} style={{ marginBottom: "8px" }}>
            <label style={S.sectionLabel}>Cost {i + 1}</label>
            <DslEditor
              value={cost.dsl}
              entryKind="Card"
              entryName={card.name}
              dslField={`cost:${i}`}
              height="80px"
            />
          </div>
        ))}
        {card.costs.length === 0 && (
          <span style={{ color: C.muted, fontSize: "0.8rem" }}>No costs defined.</span>
        )}
      </Accordion>

      {/* Flavour text */}
      <section style={styles.section}>
        <label style={S.sectionLabel} htmlFor="card-flavour">Flavour Text</label>
        <textarea
          id="card-flavour"
          style={{ ...S.textarea, height: "64px" }}
          value={flavour}
          onChange={(e) => setFlavour(e.target.value)}
          onBlur={() => { void commitField("flavourText", flavour); }}
          placeholder="When the fire speaks, even stone listens."
        />
      </section>

      {/* Art */}
      <section style={styles.section}>
        <label style={S.sectionLabel}>Art</label>
        <div style={styles.artRow}>
          <input style={{ ...S.input, flex: 1 }} value={artPath} readOnly placeholder="No art selected" />
          <button style={S.btnGhost} onClick={() => { void handlePickArt(); }}>Browse…</button>
        </div>
        {artPath && (
          <div style={styles.cropRow}>
            {[
              ["X", cropX, setCropX],
              ["Y", cropY, setCropY],
              ["W", cropW, setCropW],
              ["H", cropH, setCropH],
            ].map(([label, val, setter]) => (
              <label key={String(label)} style={{ fontSize: "0.75rem", color: C.subtext }}>
                {String(label)}
                <input
                  type="number"
                  style={{ ...S.input, width: "64px", marginLeft: "4px" }}
                  value={String(val)}
                  onChange={(e) => (setter as React.Dispatch<React.SetStateAction<number>>)(Number(e.target.value))}
                  onBlur={() => { void commitField("cropRegion", { x: cropX, y: cropY, w: cropW, h: cropH }); }}
                  aria-label={`Crop ${String(label)}`}
                />
              </label>
            ))}
          </div>
        )}
      </section>

      {/* Card text preview */}
      <section style={styles.section}>
        <div style={{ display: "flex", alignItems: "center", marginBottom: "8px" }}>
          <label style={S.sectionLabel}>Card Text Preview</label>
          <button
            style={{ ...S.btnGhost, marginLeft: "auto", fontSize: "0.7rem" }}
            onClick={() => { void refreshPreview(); }}
          >
            Refresh
          </button>
        </div>
        <div style={styles.preview} aria-label="Card text preview">
          {renderTree
            ? <pre style={styles.previewText}>{JSON.stringify(renderTree, null, 2)}</pre>
            : <span style={{ color: C.muted, fontSize: "0.8rem" }}>Click Refresh to render.</span>
          }
        </div>
      </section>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Accordion
// ---------------------------------------------------------------------------

function Accordion({
  id, label, open, onToggle, children,
}: {
  id: string;
  label: string;
  open: boolean;
  onToggle: () => void;
  children: React.ReactNode;
}): React.ReactElement {
  return (
    <section style={{ marginBottom: "4px" }}>
      <button
        style={styles.accordionBtn}
        onClick={onToggle}
        aria-expanded={open}
        aria-controls={`accordion-${id}`}
      >
        <span style={{ color: C.muted, marginRight: "6px", fontSize: "0.7rem" }}>
          {open ? "▼" : "▶"}
        </span>
        <span style={S.sectionLabel}>{label}</span>
      </button>
      {open && (
        <div id={`accordion-${id}`} style={styles.accordionBody}>
          {children}
        </div>
      )}
    </section>
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
    width:         "200px",
    flexShrink:    0,
    borderRight:   `1px solid ${C.surface0}`,
    display:       "flex",
    flexDirection: "column" as const,
    padding:       "16px 0 0",
    overflow:      "hidden",
  } satisfies React.CSSProperties,

  createRow: {
    display:      "flex",
    gap:          "6px",
    padding:      "0 10px 10px",
    borderBottom: `1px solid ${C.surface0}`,
  } satisfies React.CSSProperties,

  list: {
    listStyle: "none",
    flex:      1,
    overflowY: "auto" as const,
    margin:    0,
    padding:   0,
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

  cardName: {
    overflow:     "hidden",
    textOverflow: "ellipsis",
    whiteSpace:   "nowrap" as const,
  } satisfies React.CSSProperties,

  detail: {
    flex:      1,
    overflowY: "auto" as const,
    padding:   "16px 24px",
  } satisfies React.CSSProperties,

  detailInner: {
    maxWidth: "700px",
  } satisfies React.CSSProperties,

  emptyDetail: {
    color:    C.muted,
    fontSize: "0.8125rem",
    padding:  "16px 0",
  } satisfies React.CSSProperties,

  empty: {
    color:   C.muted,
    padding: "12px 10px",
    fontSize: "0.8125rem",
  } satisfies React.CSSProperties,

  section: {
    marginBottom: "20px",
  } satisfies React.CSSProperties,

  row2col: {
    display:      "flex",
    gap:          "16px",
    marginBottom: "20px",
  } satisfies React.CSSProperties,

  staticEffectBlock: {
    padding:      "12px",
    border:       `1px solid ${C.surface0}`,
    borderRadius: "4px",
    marginBottom: "8px",
  } satisfies React.CSSProperties,

  artRow: {
    display:      "flex",
    gap:          "8px",
    marginBottom: "8px",
    alignItems:   "center",
  } satisfies React.CSSProperties,

  cropRow: {
    display:    "flex",
    gap:        "16px",
    flexWrap:   "wrap" as const,
    alignItems: "center",
    fontSize:   "0.75rem",
    color:      C.subtext,
  } satisfies React.CSSProperties,

  preview: {
    background:   C.bg,
    border:       `1px solid ${C.surface0}`,
    borderRadius: "4px",
    padding:      "12px",
    minHeight:    "80px",
    maxHeight:    "200px",
    overflowY:    "auto" as const,
  } satisfies React.CSSProperties,

  previewText: {
    margin:     0,
    fontFamily: Font.mono,
    fontSize:   "0.8rem",
    color:      C.subtext,
  } satisfies React.CSSProperties,

  accordionBtn: {
    display:    "flex",
    alignItems: "center",
    background: "transparent",
    border:     "none",
    cursor:     "pointer",
    padding:    "4px 0",
    width:      "100%",
    textAlign:  "left" as const,
  } satisfies React.CSSProperties,

  accordionBody: {
    paddingBottom: "12px",
  } satisfies React.CSSProperties,
} as const;
