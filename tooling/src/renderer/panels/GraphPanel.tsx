/**
 * GraphPanel — keyword composition graph view (Task 6.9, D26).
 *
 * Calls GetReferenceGraph on open.
 * Renders nodes (keywords) and directed edges (composition relationships)
 * using React Flow, with layout computed via Dagre.
 * Clicking a keyword node calls onNavigate("keywords").
 */
import React, { useEffect, useCallback } from "react";
import {
  ReactFlow,
  Background,
  Controls,
  useNodesState,
  useEdgesState,
  type Node,
  type Edge,
  MarkerType,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import dagre from "@dagrejs/dagre";
import { IPC_CHANNELS } from "@shared/ipc";
import type { GetReferenceGraphResult, GraphNode, GraphEdge } from "@shared/ipc";
import { C, Font, S } from "../design/tokens";

const NODE_WIDTH  = 160;
const NODE_HEIGHT = 36;

interface GraphPanelProps {
  onNavigate: (panel: string) => void;
}

export function GraphPanel({ onNavigate }: GraphPanelProps): React.ReactElement {
  const [nodes, setNodes, onNodesChange] = useNodesState<Node>([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState<Edge>([]);
  const [loading, setLoading] = React.useState(false);
  const [error,   setError]   = React.useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    window.archetype.invoke<GetReferenceGraphResult>(IPC_CHANNELS.GetReferenceGraph)
      .then((result) => {
        if (cancelled) return;
        const { layoutedNodes, layoutedEdges } = applyDagreLayout(
          result?.nodes ?? [],
          result?.edges ?? []
        );
        setNodes(layoutedNodes);
        setEdges(layoutedEdges);
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(err instanceof Error ? err.message : "Failed to load graph.");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, [setNodes, setEdges]);

  const onNodeClick = useCallback(
    (_: React.MouseEvent, node: Node) => {
      if (node.data?.["kind"] === "Keyword") {
        onNavigate("keywords");
      } else if (node.data?.["kind"] === "Card") {
        onNavigate("cards");
      }
    },
    [onNavigate]
  );

  return (
    <div style={styles.root}>
      <h2 style={S.panelTitle}>Composition Graph</h2>

      {loading && <div style={styles.overlay}>Loading graph…</div>}
      {error   && <div style={{ ...styles.overlay, color: C.red }}>Error: {error}</div>}

      {!loading && !error && nodes.length === 0 && (
        <div style={styles.overlay}>No keywords in project.</div>
      )}

      <div style={styles.flowContainer}>
        <ReactFlow
          nodes={nodes}
          edges={edges}
          onNodesChange={onNodesChange}
          onEdgesChange={onEdgesChange}
          onNodeClick={onNodeClick}
          fitView
          style={{ background: C.bg }}
          nodesDraggable
          nodesConnectable={false}
          elementsSelectable
          aria-label="Keyword composition graph"
        >
          <Background color={C.surface0} gap={24} />
          <Controls style={{ background: C.base, border: `1px solid ${C.surface0}` }} />
        </ReactFlow>
      </div>

      {/* Legend */}
      <div style={styles.legend}>
        <LegendItem color={C.mauve} label="Keyword" />
        <LegendItem color={C.teal}  label="Card reference" />
      </div>
    </div>
  );
}

function LegendItem({ color, label }: { color: string; label: string }): React.ReactElement {
  return (
    <div style={{ display: "flex", alignItems: "center", gap: "6px", fontSize: "0.75rem", color: C.muted }}>
      <span style={{ width: "10px", height: "10px", borderRadius: "2px", background: color }} />
      {label}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Dagre layout
// ---------------------------------------------------------------------------

function applyDagreLayout(
  rawNodes: readonly GraphNode[],
  rawEdges: readonly GraphEdge[]
): { layoutedNodes: Node[]; layoutedEdges: Edge[] } {
  const g = new dagre.graphlib.Graph();
  g.setDefaultEdgeLabel(() => ({}));
  g.setGraph({ rankdir: "TB", nodesep: 40, ranksep: 60 });

  rawNodes.forEach((n) => {
    g.setNode(n.id, { width: NODE_WIDTH, height: NODE_HEIGHT });
  });
  rawEdges.forEach((e) => {
    g.setEdge(e.source, e.target);
  });

  dagre.layout(g);

  const layoutedNodes: Node[] = rawNodes.map((n) => {
    const pos = g.node(n.id);
    const isKeyword = n.kind === "Keyword";
    return {
      id:       n.id,
      type:     "default",
      position: { x: (pos?.x ?? 0) - NODE_WIDTH / 2, y: (pos?.y ?? 0) - NODE_HEIGHT / 2 },
      data:     { label: n.id, kind: n.kind },
      style: {
        background:   isKeyword ? `${C.mauve}20` : `${C.teal}20`,
        border:       `1px solid ${isKeyword ? C.mauve : C.teal}`,
        borderRadius: "4px",
        color:        C.text,
        fontFamily:   Font.mono,
        fontSize:     "0.75rem",
        padding:      "4px 10px",
        cursor:       "pointer",
        width:        NODE_WIDTH,
      },
    };
  });

  const layoutedEdges: Edge[] = rawEdges.map((e, i) => ({
    id:           `e-${i}-${e.source}-${e.target}`,
    source:       e.source,
    target:       e.target,
    type:         "smoothstep",
    animated:     false,
    markerEnd:    { type: MarkerType.ArrowClosed, color: C.surface1 },
    style:        { stroke: C.surface1, strokeWidth: 1.5 },
  }));

  return { layoutedNodes, layoutedEdges };
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
    overflow:      "hidden",
  } satisfies React.CSSProperties,

  overlay: {
    position:       "absolute" as const,
    top:            "50%",
    left:           "50%",
    transform:      "translate(-50%, -50%)",
    color:          C.muted,
    fontSize:       "0.875rem",
    pointerEvents:  "none" as const,
    zIndex:         10,
  } satisfies React.CSSProperties,

  flowContainer: {
    flex:     1,
    position: "relative" as const,
  } satisfies React.CSSProperties,

  legend: {
    display:    "flex",
    gap:        "16px",
    padding:    "8px 0 0",
    borderTop:  `1px solid ${C.surface0}`,
    marginTop:  "4px",
  } satisfies React.CSSProperties,
} as const;
