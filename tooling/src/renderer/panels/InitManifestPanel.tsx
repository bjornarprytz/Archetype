/**
 * InitManifestPanel — initial game state editor (Task 6.5, D26, D29).
 *
 * Shared/neutral zones section at top.
 * Per-player accordion with zone rows and reorderable card entries.
 * All mutations send UpdateField with entryKind "InitManifest".
 */
import React, { useEffect, useState } from "react";
import { IPC_CHANNELS } from "@shared/ipc";
import type { MutationResponse } from "@shared/ipc";
import { useDiagnosticsStore } from "../store/diagnosticsStore";
import { useProjectStore } from "../store/projectStore";
import { fetchSnapshot } from "../snapshot";
import type { PlayerInitSnapshot, InitZoneEntry } from "../snapshot";
import { C, Font, S } from "../design/tokens";

export function InitManifestPanel(): React.ReactElement {
  const [manifest,  setManifest]  = useState<PlayerInitSnapshot[]>([]);
  const [loading,   setLoading]   = useState(false);

  const updateCounts = useDiagnosticsStore((s) => s.updateCounts);
  const markDirty    = useProjectStore((s) => s.markDirty);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    fetchSnapshot().then((snap) => {
      if (cancelled) return;
      setManifest((snap?.initManifest ?? []) as PlayerInitSnapshot[]);
      setLoading(false);
    });
    return () => { cancelled = true; };
  }, []);

  async function commitManifest(updated: PlayerInitSnapshot[]): Promise<void> {
    const resp = await window.archetype.invoke<MutationResponse>(IPC_CHANNELS.UpdateField, {
      entryKind: "InitManifest",
      entryName: "initManifest",
      field:     "manifest",
      value:     updated,
    });
    markDirty();
    updateCounts(resp.globalErrorCount, resp.globalWarningCount);
  }

  const shared  = manifest.find((p) => p.playerId === null);
  const players = manifest.filter((p) => p.playerId !== null);

  return (
    <div style={styles.root}>
      <h2 style={S.panelTitle}>Init Manifest</h2>

      {loading && <div style={styles.empty}>Loading…</div>}

      {!loading && (
        <>
          {/* Shared zones */}
          <Section title="Shared / Neutral Zones">
            {shared
              ? <ZoneList
                  playerId={null}
                  zones={[...shared.zones]}
                  manifest={manifest}
                  onCommit={async (updated) => {
                    setManifest(updated);
                    await commitManifest(updated);
                  }}
                />
              : <div style={styles.empty}>No shared zones defined.</div>
            }
          </Section>

          {/* Per-player sections */}
          {players.map((player) => (
            <Section key={player.playerId!} title={`Player: ${player.playerId ?? "?"}`}>
              <ZoneList
                playerId={player.playerId}
                zones={[...player.zones]}
                manifest={manifest}
                onCommit={async (updated) => {
                  setManifest(updated);
                  await commitManifest(updated);
                }}
              />
            </Section>
          ))}

          {manifest.length === 0 && (
            <div style={styles.empty}>No init manifest entries. Load a project first.</div>
          )}
        </>
      )}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Zone list for a player
// ---------------------------------------------------------------------------

function ZoneList({
  playerId,
  zones,
  manifest,
  onCommit,
}: {
  playerId: string | null;
  zones: InitZoneEntry[];
  manifest: PlayerInitSnapshot[];
  onCommit: (updated: PlayerInitSnapshot[]) => Promise<void>;
}): React.ReactElement {
  const [dragIdx, setDragIdx] = useState<{ zone: number; card: number | null } | null>(null);

  function updateManifest(updatedZones: InitZoneEntry[]): PlayerInitSnapshot[] {
    return manifest.map((p) =>
      p.playerId === playerId ? { ...p, zones: updatedZones } : p
    );
  }

  async function handleCardDrop(zoneIdx: number, targetCardIdx: number): Promise<void> {
    if (!dragIdx || dragIdx.zone !== zoneIdx || dragIdx.card === null) {
      setDragIdx(null);
      return;
    }
    const fromCard = dragIdx.card;
    if (fromCard === targetCardIdx) { setDragIdx(null); return; }

    const zone = zones[zoneIdx];
    if (!zone) { setDragIdx(null); return; }
    const cards = [...zone.cards];
    const [item] = cards.splice(fromCard, 1);
    if (item) cards.splice(targetCardIdx, 0, item);
    const updatedZones = zones.map((z, i) => i === zoneIdx ? { ...z, cards } : z);
    await onCommit(updateManifest(updatedZones));
    setDragIdx(null);
  }

  return (
    <>
      {zones.map((zone, zoneIdx) => (
        <div key={zone.zoneName} style={styles.zoneBlock}>
          {/* Zone header row */}
          <div style={styles.zoneHeader}>
            <span style={{ fontFamily: Font.mono, color: C.teal, fontSize: "0.8rem" }}>
              {zone.zoneName}
            </span>
          </div>

          {/* Cards */}
          <div style={styles.cardList} aria-label={`Cards in zone ${zone.zoneName}`}>
            {zone.cards.map((entry, cardIdx) => (
              <div
                key={`${entry.cardName}-${cardIdx}`}
                draggable
                onDragStart={() => setDragIdx({ zone: zoneIdx, card: cardIdx })}
                onDragOver={(e) => e.preventDefault()}
                onDrop={() => { void handleCardDrop(zoneIdx, cardIdx); }}
                style={{
                  ...styles.cardRow,
                  opacity: dragIdx?.zone === zoneIdx && dragIdx.card === cardIdx ? 0.4 : 1,
                }}
                aria-label={`Card: ${entry.cardName}`}
              >
                <span style={S.dragHandle}>⠿</span>
                <span style={styles.cardLabel}>{entry.cardName}</span>
              </div>
            ))}
            {zone.cards.length === 0 && (
              <div style={styles.emptyCards}>No cards in this zone.</div>
            )}
          </div>
        </div>
      ))}
      {zones.length === 0 && (
        <div style={styles.empty}>No zones for this player.</div>
      )}
    </>
  );
}

// ---------------------------------------------------------------------------
// Collapsible section
// ---------------------------------------------------------------------------

function Section({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}): React.ReactElement {
  const [open, setOpen] = useState(true);
  return (
    <div style={styles.section}>
      <button
        style={styles.sectionHeader}
        onClick={() => setOpen((o) => !o)}
        aria-expanded={open}
      >
        <span style={{ color: C.muted, fontSize: "0.7rem", marginRight: "6px" }}>
          {open ? "▼" : "▶"}
        </span>
        <span style={S.sectionLabel}>{title}</span>
      </button>
      {open && <div style={styles.sectionBody}>{children}</div>}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Styles
// ---------------------------------------------------------------------------

const styles = {
  root: {
    height:        "100%",
    overflowY:     "auto" as const,
    fontFamily:    Font.ui,
    color:         C.text,
    fontSize:      "0.8125rem",
  } satisfies React.CSSProperties,

  empty: {
    color:   C.muted,
    padding: "12px 0",
    fontSize: "0.8125rem",
  } satisfies React.CSSProperties,

  section: {
    marginBottom: "4px",
    border:       `1px solid ${C.surface0}`,
    borderRadius: "4px",
    overflow:     "hidden",
    marginTop:    "12px",
  } satisfies React.CSSProperties,

  sectionHeader: {
    display:    "flex",
    alignItems: "center",
    width:      "100%",
    background: C.bg,
    border:     "none",
    padding:    "8px 12px",
    cursor:     "pointer",
    textAlign:  "left" as const,
  } satisfies React.CSSProperties,

  sectionBody: {
    padding: "12px",
  } satisfies React.CSSProperties,

  zoneBlock: {
    marginBottom: "12px",
  } satisfies React.CSSProperties,

  zoneHeader: {
    display:      "flex",
    alignItems:   "center",
    gap:          "12px",
    paddingBottom: "6px",
    borderBottom: `1px solid ${C.surface0}`,
    marginBottom: "6px",
  } satisfies React.CSSProperties,

  cardList: {
    paddingLeft: "16px",
  } satisfies React.CSSProperties,

  cardRow: {
    display:      "flex",
    alignItems:   "center",
    gap:          "8px",
    padding:      "4px 0",
    borderBottom: `1px solid ${C.surface0}`,
    cursor:       "default",
  } satisfies React.CSSProperties,

  cardLabel: {
    fontFamily: Font.mono,
    fontSize:   "0.8rem",
    color:      C.subtext,
  } satisfies React.CSSProperties,

  emptyCards: {
    color:    C.muted,
    fontSize: "0.75rem",
    padding:  "4px 0",
  } satisfies React.CSSProperties,
} as const;
