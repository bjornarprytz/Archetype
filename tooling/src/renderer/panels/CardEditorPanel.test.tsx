import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";

vi.mock("@monaco-editor/react", () => ({
  default: ({ value }: { value?: string }) => (
    <textarea data-testid="dsl-editor" defaultValue={value ?? ""} />
  ),
}));

import { CardEditorPanel } from "./CardEditorPanel";
import { invalidateSnapshotCache } from "../snapshot";

const snap = {
  id: "test",
  keywords: [],
  cards: [
    {
      name: "Fireball",
      primaryEffect: { name: null, dsl: "take_damage(target, 5)" },
      additionalEffects: [],
      staticEffects: [],
      activationCondition: null,
      costs: [{ dsl: "pay(3)" }],
      flavourText: "Hot.",
      artPath: null,
      staticProperties: {},
    },
  ],
  zones: [], phases: [], actionRules: [], stateBasedRules: [],
  triggerResolutionOrder: "Chronological",
  initManifest: [], sourceLanguage: "en", locales: [],
};

describe("CardEditorPanel", () => {
  beforeEach(() => {
    invalidateSnapshotCache();
    vi.mocked(window.archetype.invoke).mockResolvedValue({ json: JSON.stringify(snap) });
  });

  it("renders without crashing", async () => {
    expect(() => render(<CardEditorPanel />)).not.toThrow();
    await waitFor(() => screen.getByRole("heading", { name: "Cards" }));
  });

  it("shows the Cards heading", async () => {
    render(<CardEditorPanel />);
    await waitFor(() =>
      expect(screen.getByRole("heading", { name: "Cards" })).toBeInTheDocument()
    );
  });

  it("renders card list from snapshot", async () => {
    render(<CardEditorPanel />);
    await waitFor(() => screen.getByRole("option", { name: /Fireball/i }));
    expect(screen.getByRole("option", { name: /Fireball/i })).toBeInTheDocument();
  });

  it("shows card detail when card is selected", async () => {
    const user = userEvent.setup();
    render(<CardEditorPanel />);
    await waitFor(() => screen.getByRole("option", { name: /Fireball/i }));
    await user.click(screen.getByRole("option", { name: /Fireball/i }));
    await waitFor(() => screen.getByLabelText("Card Name"));
    expect(screen.getByTestId("dsl-editor")).toBeInTheDocument();
  });

  it("calls AddEntry when create button clicked", async () => {
    vi.mocked(window.archetype.invoke)
      .mockResolvedValueOnce({ json: JSON.stringify(snap) })
      .mockResolvedValue({ affectedEntries: [], diagnostics: [], globalErrorCount: 0, globalWarningCount: 0 });

    const user = userEvent.setup();
    render(<CardEditorPanel />);
    await waitFor(() => screen.getByLabelText("New card name"));
    await user.type(screen.getByLabelText("New card name"), "Lightning Bolt");
    await user.click(screen.getByLabelText("Create card"));
    await waitFor(() =>
      expect(window.archetype.invoke).toHaveBeenCalledWith("AddEntry", {
        entryKind: "Card",
        entryName: "Lightning Bolt",
      })
    );
  });
});
