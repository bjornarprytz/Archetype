/**
 * SetOverviewPanel tests (Task 6.10, D26).
 *
 * Tests cover: stat tiles, keyword usage chart, composition depth buckets,
 * empty state when no project is open.
 */
import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";

import { SetOverviewPanel } from "./SetOverviewPanel";

const snap = {
  id: "test",
  keywords: [
    { name: "attack",     body: "take_damage(a,1)", parameters: [], textTemplate: "", noSignal: false },
    { name: "take_damage", body: "",                parameters: [], textTemplate: "", noSignal: false },
    { name: "heal",       body: "add_hp(a,1)",      parameters: [], textTemplate: "", noSignal: false },
  ],
  cards: [
    {
      name: "Fireball",
      primaryEffect: { name: null, dsl: "attack(target, 5)" },
      additionalEffects: [],
      staticEffects: [],
      activationCondition: null,
      costs: [],
      flavourText: "",
      artPath: null,
      staticProperties: {},
    },
  ],
  zones: [{ name: "Hand", ownedBy: null, accumulator: "", condition: null }],
  phases: [{ name: "Draw", initDsl: "", cleanupDsl: "" }],
  actionRules: [],
  stateBasedRules: [],
  triggerResolutionOrder: "Chronological",
  initManifest: [],
  sourceLanguage: "en",
  locales: [],
};

const graphResult = {
  nodes: [
    { id: "attack",      kind: "Keyword" },
    { id: "take_damage", kind: "Keyword" },
    { id: "heal",        kind: "Keyword" },
  ],
  edges: [
    { source: "attack", target: "take_damage" },
  ],
};

describe("SetOverviewPanel", () => {
  beforeEach(() => {
    // First call returns SaveProject (snapshot), second returns GetReferenceGraph
    vi.mocked(window.archetype.invoke)
      .mockResolvedValueOnce({ json: JSON.stringify(snap) })
      .mockResolvedValueOnce(graphResult);
  });

  it("renders without crashing", async () => {
    expect(() => render(<SetOverviewPanel />)).not.toThrow();
    await waitFor(() => screen.getByRole("heading", { name: "Set Overview" }));
  });

  it("shows the Set Overview heading", async () => {
    render(<SetOverviewPanel />);
    await waitFor(() =>
      expect(screen.getByRole("heading", { name: "Set Overview" })).toBeInTheDocument()
    );
  });

  it("shows stat tiles with counts", async () => {
    render(<SetOverviewPanel />);
    await waitFor(() => screen.getByLabelText("Keywords: 3"));
    expect(screen.getByLabelText("Keywords: 3")).toBeInTheDocument();
    expect(screen.getByLabelText("Cards: 1")).toBeInTheDocument();
    expect(screen.getByLabelText("Zones: 1")).toBeInTheDocument();
    expect(screen.getByLabelText("Phases: 1")).toBeInTheDocument();
  });

  it("shows keyword usage chart", async () => {
    render(<SetOverviewPanel />);
    await waitFor(() => screen.getByLabelText("Keyword usage chart"));
    expect(screen.getByLabelText("Keyword usage chart")).toBeInTheDocument();
  });

  it("shows keyword names in usage chart", async () => {
    render(<SetOverviewPanel />);
    await waitFor(() => screen.getByLabelText("Keyword usage chart"));
    // take_damage is referenced by attack — its bar label text should appear
    // (there may be multiple instances — depth buckets also show the name)
    const elements = screen.getAllByText("take_damage");
    expect(elements.length).toBeGreaterThan(0);
  });

  it("shows composition depth buckets", async () => {
    render(<SetOverviewPanel />);
    await waitFor(() => screen.getByLabelText(/Primitive:/i));
    expect(screen.getByLabelText(/Primitive:/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Composite:/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Complex:/i)).toBeInTheDocument();
  });

  it("shows empty state when no project is open", async () => {
    vi.mocked(window.archetype.invoke).mockReset();
    vi.mocked(window.archetype.invoke)
      .mockResolvedValueOnce({ json: null })   // SaveProject returns null
      .mockResolvedValueOnce({ nodes: [], edges: [] }); // GetReferenceGraph
    render(<SetOverviewPanel />);
    await waitFor(() => screen.getByText(/no project open/i));
    expect(screen.getByText(/no project open/i)).toBeInTheDocument();
  });
});
