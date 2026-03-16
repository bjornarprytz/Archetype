import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { LocalizationPanel } from "./LocalizationPanel";
import { invalidateSnapshotCache } from "../snapshot";

const snap = {
  id: "test",
  keywords: [], cards: [], zones: [], phases: [], actionRules: [],
  stateBasedRules: [], triggerResolutionOrder: "Chronological", initManifest: [],
  sourceLanguage: "en",
  locales: [
    {
      locale: "en",
      translations: { "card.fireball.name": "Fireball", "card.fireball.flavour": "Hot." },
    },
    {
      locale: "de",
      translations: { "card.fireball.name": "Feuerball" },
      // "card.fireball.flavour" is missing
    },
  ],
};

describe("LocalizationPanel", () => {
  beforeEach(() => {
    invalidateSnapshotCache();
    vi.mocked(window.archetype.invoke).mockResolvedValue({ json: JSON.stringify(snap) });
  });

  it("renders without crashing", async () => {
    expect(() => render(<LocalizationPanel />)).not.toThrow();
    await waitFor(() => screen.getByRole("heading", { name: "Localization" }));
  });

  it("shows the Localization heading", async () => {
    render(<LocalizationPanel />);
    await waitFor(() =>
      expect(screen.getByRole("heading", { name: "Localization" })).toBeInTheDocument()
    );
  });

  it("shows source language selector", async () => {
    render(<LocalizationPanel />);
    await waitFor(() => screen.getByLabelText("Source Language"));
    expect(screen.getByLabelText("Source Language")).toHaveValue("en");
  });

  it("shows locale tabs", async () => {
    render(<LocalizationPanel />);
    await waitFor(() => screen.getByRole("tab", { name: /en/i }));
    expect(screen.getByRole("tab", { name: /en/i })).toBeInTheDocument();
    expect(screen.getByRole("tab", { name: /de/i })).toBeInTheDocument();
  });

  it("shows missing badge on locale with missing strings", async () => {
    render(<LocalizationPanel />);
    await waitFor(() => screen.getByRole("tab", { name: /de/i }));
    // de tab should have the badge showing 1 missing
    const deTab = screen.getByRole("tab", { name: /de/i });
    expect(deTab).toBeInTheDocument();
  });

  it("switches locale and shows translations", async () => {
    const user = userEvent.setup();
    render(<LocalizationPanel />);
    await waitFor(() => screen.getByRole("tab", { name: /de/i }));
    await user.click(screen.getByRole("tab", { name: /de/i }));
    await waitFor(() => screen.getByLabelText(/Translation for card.fireball.name/i));
    const input = screen.getByLabelText(/Translation for card.fireball.name/i);
    expect(input).toHaveValue("Feuerball");
  });

  it("shows missing warning hint for untranslated strings", async () => {
    const user = userEvent.setup();
    render(<LocalizationPanel />);
    await waitFor(() => screen.getByRole("tab", { name: /de/i }));
    await user.click(screen.getByRole("tab", { name: /de/i }));
    await waitFor(() => screen.getByLabelText("Missing translation warning"));
    expect(screen.getByLabelText("Missing translation warning")).toBeInTheDocument();
  });

  it("calls UpdateField when translation changes", async () => {
    vi.mocked(window.archetype.invoke)
      .mockResolvedValueOnce({ json: JSON.stringify(snap) })
      .mockResolvedValue({ affectedEntries: [], diagnostics: [], globalErrorCount: 0, globalWarningCount: 0 });

    const user = userEvent.setup();
    render(<LocalizationPanel />);
    await waitFor(() => screen.getByRole("tab", { name: /de/i }));
    await user.click(screen.getByRole("tab", { name: /de/i }));
    await waitFor(() => screen.getByLabelText(/Translation for card.fireball.name/i));
    await user.clear(screen.getByLabelText(/Translation for card.fireball.name/i));
    await user.type(screen.getByLabelText(/Translation for card.fireball.name/i), "Brand");
    await waitFor(() =>
      expect(window.archetype.invoke).toHaveBeenCalledWith("UpdateField", expect.objectContaining({
        entryKind: "Localization",
      }))
    );
  });
});
