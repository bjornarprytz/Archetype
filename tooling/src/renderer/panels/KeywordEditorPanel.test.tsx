import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";

vi.mock("@monaco-editor/react", () => ({
  default: ({ value }: { value?: string }) => (
    <textarea data-testid="dsl-editor" defaultValue={value ?? ""} />
  ),
}));

import { KeywordEditorPanel } from "./KeywordEditorPanel";

const makeSnapshot = (keywords = [{ name: "attack", body: "take_damage(a,1)", parameters: [], textTemplate: "", noSignal: false }]) => ({
  keywords,
  cards: [], zones: [], phases: [], actionRules: [], stateBasedRules: [],
  triggerResolutionOrder: "Chronological",
  initManifest: [], sourceLanguage: "en", locales: [],
  id: "test",
});

describe("KeywordEditorPanel", () => {
  beforeEach(() => {
    vi.mocked(window.archetype.invoke).mockResolvedValue({
      json: JSON.stringify(makeSnapshot()),
    });
  });

  it("renders without crashing", async () => {
    expect(() => render(<KeywordEditorPanel />)).not.toThrow();
    await waitFor(() => screen.getByRole("heading", { name: "Keywords" }));
  });

  it("shows the Keywords heading", async () => {
    render(<KeywordEditorPanel />);
    await waitFor(() =>
      expect(screen.getByRole("heading", { name: "Keywords" })).toBeInTheDocument()
    );
  });

  it("populates keyword list from snapshot", async () => {
    render(<KeywordEditorPanel />);
    await waitFor(() => screen.getByRole("option", { name: /attack/i }));
    expect(screen.getByRole("option", { name: /attack/i })).toBeInTheDocument();
  });

  it("shows detail panel when a keyword is selected", async () => {
    const user = userEvent.setup();
    render(<KeywordEditorPanel />);
    await waitFor(() => screen.getByRole("option", { name: /attack/i }));
    await user.click(screen.getByRole("option", { name: /attack/i }));
    await waitFor(() => screen.getByLabelText("New keyword name"));
    expect(screen.getByTestId("dsl-editor")).toBeInTheDocument();
  });

  it("calls AddEntry when create button is clicked", async () => {
    vi.mocked(window.archetype.invoke)
      .mockResolvedValueOnce({ json: JSON.stringify(makeSnapshot()) })
      .mockResolvedValue({
        affectedEntries: [],
        diagnostics: [],
        globalErrorCount: 0,
        globalWarningCount: 0,
      });

    const user = userEvent.setup();
    render(<KeywordEditorPanel />);
    await waitFor(() => screen.getByLabelText("New keyword name"));
    await user.type(screen.getByLabelText("New keyword name"), "heal");
    await user.click(screen.getByLabelText("Create keyword"));
    await waitFor(() =>
      expect(window.archetype.invoke).toHaveBeenCalledWith("AddEntry", {
        entryKind: "Keyword",
        entryName: "heal",
      })
    );
  });
});
