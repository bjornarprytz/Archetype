/**
 * SetOverviewPanel — project metrics dashboard (Task 6.10, D26).
 *
 * Shows:
 *  - Stat tiles: keyword / card / zone / phase counts
 *  - Keyword usage frequency (refs-in chart from GetReferenceGraph)
 *  - Composition depth buckets: primitives / composites / complex
 */
import React, { useEffect, useState } from "react";
import { IPC_CHANNELS } from "@shared/ipc";
import type { GetReferenceGraphResult } from "@shared/ipc";
import { fetchSnapshot } from "../snapshot";
import type { ProjectSnapshot } from "../snapshot";
import { C, Font, S } from "../design/tokens";

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

interface KeywordStat {
  name: string;
  inRefs: number;  // how many other nodes point to this keyword
  outRefs: number; // how many other keywords this one calls
}

type DepthBucket = "primitive" | "composite" | "complex";

// ---------------------------------------------------------------------------
// Panel
// ---------------------------------------------------------------------------

export function SetOverviewPanel(): React.ReactElement {
  const [snap,     setSnap]     = useState<ProjectSnapshot | null>(null);
  const [kwStats,  setKwStats]  = useState<KeywordStat[]>([]);
  const [loading,  setLoading]  = useState(false);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);

    Promise.all([
      fetchSnapshot(),
      window.archetype.invoke<GetReferenceGraphResult>(IPC_CHANNELS.GetReferenceGraph)
        .catch(() => null as GetReferenceGraphResult | null),
    ]).then(([snapshot, graph]) => {
      if (cancelled) return;
      setSnap(snapshot);

      if (graph?.nodes && graph?.edges) {
        // Compute in/out reference counts per keyword node
        const inCount:  Record<string, number> = {};
        const outCount: Record<string, number> = {};
        for (const node of graph.nodes) {
          if (node.kind === "Keyword") {
            inCount[node.id]  = inCount[node.id]  ?? 0;
            outCount[node.id] = outCount[node.id] ?? 0;
          }
        }
        for (const edge of graph.edges) {
          if (edge.target in inCount)  inCount[edge.target]  = (inCount[edge.target]  ?? 0) + 1;
          if (edge.source in outCount) outCount[edge.source] = (outCount[edge.source] ?? 0) + 1;
        }
        const stats: KeywordStat[] = Object.keys(inCount)
          .map((name) => ({ name, inRefs: inCount[name] ?? 0, outRefs: outCount[name] ?? 0 }))
          .sort((a, b) => b.inRefs - a.inRefs);
        setKwStats(stats);
      }
    }).finally(() => {
      if (!cancelled) setLoading(false);
    });

    return () => { cancelled = true; };
  }, []);

  const keywords  = snap?.keywords  ?? [];
  const cards     = snap?.cards     ?? [];
  const zones     = snap?.zones     ?? [];
  const phases    = snap?.phases    ?? [];

  // Depth buckets
  const primitives = kwStats.filter((k) => k.outRefs === 0);
  const composites = kwStats.filter((k) => k.outRefs === 1);
  const complex    = kwStats.filter((k) => k.outRefs >= 2);

  const maxInRefs = kwStats.length > 0 ? Math.max(...kwStats.map((k) => k.inRefs), 1) : 1;

  return (
    <div style={styles.root}>
      <h2 style={S.panelTitle}>Set Overview</h2>

      {loading && <div style={styles.loading}>Loading…</div>}

      {!loading && !snap && (
        <div style={styles.loading}>No project open.</div>
      )}

      {!loading && snap && (
        <>
          {/* ---------------------------------------------------------------- */}
          {/* Stat tiles                                                        */}
          {/* ---------------------------------------------------------------- */}
          <div style={styles.statRow}>
            <StatTile value={keywords.length} label="Keywords" color={C.mauve} />
            <StatTile value={cards.length}    label="Cards"    color={C.teal}  />
            <StatTile value={zones.length}    label="Zones"    color={C.peach} />
            <StatTile value={phases.length}   label="Phases"   color={C.green} />
          </div>

          <div style={S.divider} />

          {/* ---------------------------------------------------------------- */}
          {/* Keyword usage chart                                               */}
          {/* ---------------------------------------------------------------- */}
          <section style={styles.section}>
            <span style={S.sectionLabel}>Keyword Usage (incoming references)</span>
            {kwStats.length === 0 && (
              <p style={styles.empty}>No keywords in project.</p>
            )}
            <div style={styles.barList} role="list" aria-label="Keyword usage chart">
              {kwStats.map((kw) => (
                <div key={kw.name} style={styles.barRow} role="listitem">
                  <span style={styles.barLabel}>{kw.name}</span>
                  <div style={styles.barTrack} aria-label={`${kw.inRefs} refs`}>
                    <div
                      style={{
                        ...styles.barFill,
                        width: `${Math.max(2, (kw.inRefs / maxInRefs) * 100)}%`,
                        background: kw.inRefs === 0 ? C.surface1
                          : kw.outRefs === 0 ? C.mauve  // primitive
                          : C.accent,
                      }}
                    />
                  </div>
                  <span style={styles.barCount}>{kw.inRefs}</span>
                </div>
              ))}
            </div>
          </section>

          <div style={S.divider} />

          {/* ---------------------------------------------------------------- */}
          {/* Composition depth                                                 */}
          {/* ---------------------------------------------------------------- */}
          <section style={styles.section}>
            <span style={S.sectionLabel}>Composition Depth</span>
            <div style={styles.depthGrid}>
              <DepthBucketCard
                label="Primitive"
                subtitle="no dependencies"
                color={C.mauve}
                items={primitives.map((k) => k.name)}
              />
              <DepthBucketCard
                label="Composite"
                subtitle="calls 1 keyword"
                color={C.accent}
                items={composites.map((k) => k.name)}
              />
              <DepthBucketCard
                label="Complex"
                subtitle="calls 2+ keywords"
                color={C.yellow}
                items={complex.map((k) => k.name)}
              />
            </div>
          </section>
        </>
      )}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Sub-components
// ---------------------------------------------------------------------------

function StatTile({
  value, label, color,
}: {
  value: number;
  label: string;
  color: string;
}): React.ReactElement {
  return (
    <div style={{ ...styles.tile, borderTopColor: color }} aria-label={`${label}: ${value}`}>
      <span style={{ ...styles.tileValue, color }}>{value}</span>
      <span style={styles.tileLabel}>{label}</span>
    </div>
  );
}

function DepthBucketCard({
  label, subtitle, color, items,
}: {
  label: string;
  subtitle: string;
  color: string;
  items: string[];
}): React.ReactElement {
  return (
    <div style={styles.depthCard} aria-label={`${label}: ${items.length} keywords`}>
      <div style={styles.depthHeader}>
        <span style={{ ...styles.depthLabel, color }}>{label}</span>
        <span style={styles.depthCount}>{items.length}</span>
      </div>
      <span style={styles.depthSub}>{subtitle}</span>
      <div style={styles.depthItems}>
        {items.length === 0
          ? <span style={{ color: C.muted, fontSize: "0.75rem" }}>—</span>
          : items.map((name) => (
              <span key={name} style={styles.depthChip}>{name}</span>
            ))
        }
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Styles
// ---------------------------------------------------------------------------

const styles = {
  root: {
    height:     "100%",
    overflowY:  "auto" as const,
    fontFamily: Font.ui,
    color:      C.text,
    fontSize:   "0.8125rem",
    maxWidth:   "800px",
  } satisfies React.CSSProperties,

  loading: {
    color:    C.muted,
    fontSize: "0.8125rem",
    padding:  "16px 0",
  } satisfies React.CSSProperties,

  statRow: {
    display:  "grid",
    gridTemplateColumns: "repeat(4, 1fr)",
    gap:      "12px",
    marginBottom: "20px",
  } satisfies React.CSSProperties,

  tile: {
    background:    C.bg,
    border:        `1px solid ${C.surface0}`,
    borderTop:     `3px solid transparent`,
    borderRadius:  "4px",
    padding:       "14px 16px 12px",
    display:       "flex",
    flexDirection: "column" as const,
    gap:           "4px",
  } satisfies React.CSSProperties,

  tileValue: {
    fontFamily:  Font.mono,
    fontSize:    "1.75rem",
    fontWeight:  700,
    lineHeight:  1,
    letterSpacing: "-0.02em",
  } satisfies React.CSSProperties,

  tileLabel: {
    fontSize:      "0.6875rem",
    fontWeight:    700,
    letterSpacing: "0.08em",
    textTransform: "uppercase" as const,
    color:         C.muted,
  } satisfies React.CSSProperties,

  section: {
    marginBottom: "20px",
  } satisfies React.CSSProperties,

  empty: {
    color:   C.muted,
    margin:  "8px 0",
    fontSize: "0.8rem",
  } satisfies React.CSSProperties,

  barList: {
    display:       "flex",
    flexDirection: "column" as const,
    gap:           "5px",
    marginTop:     "8px",
  } satisfies React.CSSProperties,

  barRow: {
    display:    "flex",
    alignItems: "center",
    gap:        "10px",
  } satisfies React.CSSProperties,

  barLabel: {
    fontFamily:   Font.mono,
    fontSize:     "0.75rem",
    color:        C.subtext,
    width:        "160px",
    flexShrink:   0,
    overflow:     "hidden",
    textOverflow: "ellipsis",
    whiteSpace:   "nowrap" as const,
  } satisfies React.CSSProperties,

  barTrack: {
    flex:         1,
    height:       "8px",
    background:   C.surface0,
    borderRadius: "2px",
    overflow:     "hidden",
  } satisfies React.CSSProperties,

  barFill: {
    height:       "100%",
    borderRadius: "2px",
    transition:   "width 0.3s ease",
  } satisfies React.CSSProperties,

  barCount: {
    fontFamily:  Font.mono,
    fontSize:    "0.7rem",
    color:       C.muted,
    width:       "28px",
    textAlign:   "right" as const,
    flexShrink:  0,
  } satisfies React.CSSProperties,

  depthGrid: {
    display:             "grid",
    gridTemplateColumns: "repeat(3, 1fr)",
    gap:                 "12px",
    marginTop:           "8px",
  } satisfies React.CSSProperties,

  depthCard: {
    background:    C.bg,
    border:        `1px solid ${C.surface0}`,
    borderRadius:  "4px",
    padding:       "12px",
  } satisfies React.CSSProperties,

  depthHeader: {
    display:        "flex",
    alignItems:     "baseline",
    justifyContent: "space-between",
    marginBottom:   "2px",
  } satisfies React.CSSProperties,

  depthLabel: {
    fontSize:      "0.75rem",
    fontWeight:    700,
    letterSpacing: "0.06em",
    textTransform: "uppercase" as const,
  } satisfies React.CSSProperties,

  depthCount: {
    fontFamily: Font.mono,
    fontSize:   "1rem",
    fontWeight: 700,
    color:      C.text,
  } satisfies React.CSSProperties,

  depthSub: {
    display:    "block",
    fontSize:   "0.7rem",
    color:      C.muted,
    marginBottom: "8px",
  } satisfies React.CSSProperties,

  depthItems: {
    display:  "flex",
    flexWrap: "wrap" as const,
    gap:      "4px",
  } satisfies React.CSSProperties,

  depthChip: {
    fontFamily:   Font.mono,
    fontSize:     "0.7rem",
    background:   C.surface0,
    color:        C.subtext,
    padding:      "2px 6px",
    borderRadius: "2px",
    whiteSpace:   "nowrap" as const,
  } satisfies React.CSSProperties,
} as const;
