/**
 * GraphPanel tests (Task 6.9, D26).
 *
 * React Flow requires ResizeObserver and a real DOM — we mock it to avoid
 * jsdom limitations.  The tests focus on panel behaviour (loading, empty
 * state, navigation callbacks) rather than graph rendering internals.
 */
import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";

// Mock React Flow — the real package uses ResizeObserver / canvas APIs
// not available in jsdom.
vi.mock("@xyflow/react", () => ({
  ReactFlow: ({
    nodes,
    onNodeClick,
    children,
  }: {
    nodes: Array<{ id: string; data: Record<string, unknown> }>;
    edges: unknown[];
    onNodeClick?: (e: React.MouseEvent, node: { id: string; data: Record<string, unknown> }) => void;
    children?: React.ReactNode;
  }) => (
    <div data-testid="react-flow" aria-label="Keyword composition graph">
      {nodes.map((node) => (
        <button
          key={node.id}
          data-testid={`node-${node.id}`}
          onClick={(e) => onNodeClick?.(e, node)}
        >
          {node.id}
        </button>
      ))}
      {children}
    </div>
  ),
  Background: () => <div data-testid="rf-background" />,
  Controls:   () => <div data-testid="rf-controls" />,
  useNodesState: (init: unknown[]) => {
    const [nodes, setNodes] = React.useState(init);
    return [nodes, setNodes, vi.fn()];
  },
  useEdgesState: (init: unknown[]) => {
    const [edges, setEdges] = React.useState(init);
    return [edges, setEdges, vi.fn()];
  },
  MarkerType: { ArrowClosed: "arrowclosed" },
}));

// Mock Dagre — avoids layout computation in jsdom
vi.mock("@dagrejs/dagre", () => ({
  default: {
    graphlib: {
      Graph: class {
        setDefaultEdgeLabel() { return this; }
        setGraph()            { return this; }
        setNode()             { return this; }
        setEdge()             { return this; }
        node()                { return { x: 0, y: 0 }; }
      },
    },
    layout: vi.fn(),
  },
}));

import { GraphPanel } from "./GraphPanel";

const graphResult = {
  nodes: [
    { id: "attack",     kind: "Keyword" },
    { id: "take_damage", kind: "Keyword" },
  ],
  edges: [
    { source: "attack", target: "take_damage" },
  ],
};

describe("GraphPanel", () => {
  const onNavigate = vi.fn();

  beforeEach(() => {
    onNavigate.mockClear();
    vi.mocked(window.archetype.invoke).mockResolvedValue(graphResult);
  });

  it("renders without crashing", async () => {
    expect(() => render(<GraphPanel onNavigate={onNavigate} />)).not.toThrow();
    await waitFor(() => screen.getByRole("heading", { name: "Composition Graph" }));
  });

  it("shows Composition Graph heading", async () => {
    render(<GraphPanel onNavigate={onNavigate} />);
    await waitFor(() =>
      expect(screen.getByRole("heading", { name: "Composition Graph" })).toBeInTheDocument()
    );
  });

  it("calls GetReferenceGraph on mount", async () => {
    render(<GraphPanel onNavigate={onNavigate} />);
    await waitFor(() =>
      expect(window.archetype.invoke).toHaveBeenCalledWith("GetReferenceGraph")
    );
  });

  it("renders the flow canvas after data loads", async () => {
    render(<GraphPanel onNavigate={onNavigate} />);
    await waitFor(() => screen.getByTestId("react-flow"));
    expect(screen.getByTestId("react-flow")).toBeInTheDocument();
  });

  it("shows empty state when no keywords in project", async () => {
    vi.mocked(window.archetype.invoke).mockResolvedValue({ nodes: [], edges: [] });
    render(<GraphPanel onNavigate={onNavigate} />);
    await waitFor(() => screen.getByText(/no keywords in project/i));
    expect(screen.getByText(/no keywords in project/i)).toBeInTheDocument();
  });

  it("calls onNavigate('keywords') when a Keyword node is clicked", async () => {
    const user = userEvent.setup();
    render(<GraphPanel onNavigate={onNavigate} />);
    await waitFor(() => screen.getByTestId("node-attack"));
    await user.click(screen.getByTestId("node-attack"));
    expect(onNavigate).toHaveBeenCalledWith("keywords");
  });

  it("shows error state when invoke rejects", async () => {
    vi.mocked(window.archetype.invoke).mockRejectedValue(new Error("network error"));
    render(<GraphPanel onNavigate={onNavigate} />);
    await waitFor(() => screen.getByText(/error:.*network error/i));
    expect(screen.getByText(/error:.*network error/i)).toBeInTheDocument();
  });
});
