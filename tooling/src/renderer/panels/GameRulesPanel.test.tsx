import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";

vi.mock("@monaco-editor/react", () => ({
  default: ({ value }: { value?: string }) => (
    <textarea data-testid="dsl-editor" defaultValue={value ?? ""} />
  ),
}));

import { GameRulesPanel } from "./GameRulesPanel";
import { invalidateSnapshotCache } from "../snapshot";

const snap = {
  id: "test",
  keywords: [], cards: [], zones: [],
  phases: [
    { name: "Draw", initDsl: "draw(p, 1)", cleanupDsl: "" },
    { name: "Main", initDsl: "",           cleanupDsl: "" },
  ],
  actionRules: [
    { actionType: "PlayCard", beforeDsl: "// before", afterDsl: "// after" },
  ],
  stateBasedRules: [
    { conditionDsl: "player.health <= 0", bodyDsl: "player.lose()" },
  ],
  triggerResolutionOrder: "Chronological",
  initManifest: [], sourceLanguage: "en", locales: [],
};

describe("GameRulesPanel", () => {
  beforeEach(() => {
    invalidateSnapshotCache();
    vi.mocked(window.archetype.invoke).mockResolvedValue({ json: JSON.stringify(snap) });
  });

  it("renders without crashing", async () => {
    expect(() => render(<GameRulesPanel />)).not.toThrow();
    await waitFor(() => screen.getByRole("heading", { name: "Game Rules" }));
  });

  it("shows the Game Rules heading", async () => {
    render(<GameRulesPanel />);
    await waitFor(() =>
      expect(screen.getByRole("heading", { name: "Game Rules" })).toBeInTheDocument()
    );
  });

  it("renders tab navigation", async () => {
    render(<GameRulesPanel />);
    await waitFor(() => screen.getByRole("tab", { name: "Phases" }));
    expect(screen.getByRole("tab", { name: "Phases" })).toBeInTheDocument();
    expect(screen.getByRole("tab", { name: "Action Rules" })).toBeInTheDocument();
    expect(screen.getByRole("tab", { name: "State Rules" })).toBeInTheDocument();
    expect(screen.getByRole("tab", { name: "Trigger Order" })).toBeInTheDocument();
  });

  it("switches to Action Rules tab", async () => {
    const user = userEvent.setup();
    render(<GameRulesPanel />);
    await waitFor(() => screen.getByRole("tab", { name: "Action Rules" }));
    await user.click(screen.getByRole("tab", { name: "Action Rules" }));
    await waitFor(() => screen.getByText("PlayCard"));
    expect(screen.getByText("PlayCard")).toBeInTheDocument();
  });

  it("shows trigger order radio buttons", async () => {
    const user = userEvent.setup();
    render(<GameRulesPanel />);
    await waitFor(() => screen.getByRole("tab", { name: "Trigger Order" }));
    await user.click(screen.getByRole("tab", { name: "Trigger Order" }));
    await waitFor(() => screen.getByText("Chronological"));
    expect(screen.getByRole("radio", { name: "Chronological" })).toBeChecked();
    expect(screen.getByRole("radio", { name: "ReverseChronological" })).not.toBeChecked();
  });

  it("switches trigger order and calls sidecar", async () => {
    vi.mocked(window.archetype.invoke)
      .mockResolvedValueOnce({ json: JSON.stringify(snap) })
      .mockResolvedValue({ affectedEntries: [], diagnostics: [], globalErrorCount: 0, globalWarningCount: 0 });
    const user = userEvent.setup();
    render(<GameRulesPanel />);
    await waitFor(() => screen.getByRole("tab", { name: "Trigger Order" }));
    await user.click(screen.getByRole("tab", { name: "Trigger Order" }));
    await waitFor(() => screen.getByRole("radio", { name: "ReverseChronological" }));
    await user.click(screen.getByRole("radio", { name: "ReverseChronological" }));
    await waitFor(() =>
      expect(window.archetype.invoke).toHaveBeenCalledWith("UpdateField", expect.objectContaining({
        entryKind: "GameRules",
      }))
    );
  });
});
