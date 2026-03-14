import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { InitManifestPanel } from "./InitManifestPanel";

const snap = {
  id: "test",
  keywords: [], cards: [], zones: [],
  phases: [], actionRules: [], stateBasedRules: [],
  triggerResolutionOrder: "Chronological",
  sourceLanguage: "en", locales: [],
  initManifest: [
    {
      playerId: null,
      zones: [{ zoneName: "SharedDeck", cards: [{ cardName: "StartCard" }] }],
    },
    {
      playerId: "p1",
      zones: [{ zoneName: "Hand", cards: [] }],
    },
  ],
};

describe("InitManifestPanel", () => {
  beforeEach(() => {
    vi.mocked(window.archetype.invoke).mockResolvedValue({ json: JSON.stringify(snap) });
  });

  it("renders without crashing", async () => {
    expect(() => render(<InitManifestPanel />)).not.toThrow();
    await waitFor(() => screen.getByRole("heading", { name: "Init Manifest" }));
  });

  it("shows the Init Manifest heading", async () => {
    render(<InitManifestPanel />);
    await waitFor(() =>
      expect(screen.getByRole("heading", { name: "Init Manifest" })).toBeInTheDocument()
    );
  });

  it("shows shared zones section", async () => {
    render(<InitManifestPanel />);
    await waitFor(() => screen.getByText(/shared.*neutral/i));
    expect(screen.getByText(/shared.*neutral/i)).toBeInTheDocument();
  });

  it("shows player section", async () => {
    render(<InitManifestPanel />);
    await waitFor(() => screen.getByText(/player: p1/i));
    expect(screen.getByText(/player: p1/i)).toBeInTheDocument();
  });

  it("shows zone names", async () => {
    render(<InitManifestPanel />);
    await waitFor(() => screen.getByText("SharedDeck"));
    expect(screen.getByText("SharedDeck")).toBeInTheDocument();
    await waitFor(() => screen.getByText("Hand"));
    expect(screen.getByText("Hand")).toBeInTheDocument();
  });

  it("shows card names within zones", async () => {
    render(<InitManifestPanel />);
    await waitFor(() => screen.getByLabelText("Card: StartCard"));
    expect(screen.getByLabelText("Card: StartCard")).toBeInTheDocument();
  });

  it("renders empty state when no manifest", async () => {
    vi.mocked(window.archetype.invoke).mockResolvedValue({
      json: JSON.stringify({ ...snap, initManifest: [] }),
    });
    render(<InitManifestPanel />);
    await waitFor(() => screen.getByText(/no init manifest/i));
    expect(screen.getByText(/no init manifest/i)).toBeInTheDocument();
  });
});
