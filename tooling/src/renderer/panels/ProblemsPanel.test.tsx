import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { useDiagnosticsStore } from "../store/diagnosticsStore";
import { ProblemsPanel } from "./ProblemsPanel";

describe("ProblemsPanel", () => {
  const onNavigate = vi.fn();

  beforeEach(() => {
    onNavigate.mockClear();
    useDiagnosticsStore.setState({ diagnostics: null, globalErrorCount: 0, globalWarningCount: 0 });
    vi.mocked(window.archetype.invoke).mockResolvedValue({ diagnostics: [] });
  });

  it("renders without crashing", async () => {
    expect(() => render(<ProblemsPanel onNavigate={onNavigate} />)).not.toThrow();
    await waitFor(() => screen.getByText(/no problems/i));
  });

  it("shows Problems heading", () => {
    render(<ProblemsPanel onNavigate={onNavigate} />);
    expect(screen.getByRole("heading", { name: "Problems" })).toBeInTheDocument();
  });

  it("shows empty state when no diagnostics", async () => {
    render(<ProblemsPanel onNavigate={onNavigate} />);
    await waitFor(() => expect(screen.getByText(/no problems/i)).toBeInTheDocument());
  });

  it("calls GetAllDiagnostics on mount", async () => {
    render(<ProblemsPanel onNavigate={onNavigate} />);
    await waitFor(() =>
      expect(window.archetype.invoke).toHaveBeenCalledWith("GetAllDiagnostics")
    );
  });

  it("renders error and warning rows sorted errors-first", async () => {
    vi.mocked(window.archetype.invoke).mockResolvedValue({
      diagnostics: [
        { entryKind: "Keyword", entryName: "foo", severity: "warning", message: "unused param" },
        { entryKind: "Card", entryName: "Fireball", severity: "error", message: "undefined keyword" },
      ],
    });
    render(<ProblemsPanel onNavigate={onNavigate} />);
    await waitFor(() => expect(screen.getAllByRole("listitem")).toHaveLength(2));

    const items = screen.getAllByRole("listitem");
    // Error should be first
    expect(items[0]).toHaveTextContent("error");
    expect(items[0]).toHaveTextContent("Fireball");
    expect(items[1]).toHaveTextContent("warning");
    expect(items[1]).toHaveTextContent("foo");
  });

  it("navigates to keywords panel when a Keyword row is clicked", async () => {
    vi.mocked(window.archetype.invoke).mockResolvedValue({
      diagnostics: [
        { entryKind: "Keyword", entryName: "attack", severity: "error", message: "bad" },
      ],
    });
    const user = userEvent.setup();
    render(<ProblemsPanel onNavigate={onNavigate} />);
    await waitFor(() => screen.getAllByRole("listitem"));
    await user.click(screen.getAllByRole("listitem")[0]!);
    expect(onNavigate).toHaveBeenCalledWith("keywords");
  });

  it("navigates to cards panel when a Card row is clicked", async () => {
    vi.mocked(window.archetype.invoke).mockResolvedValue({
      diagnostics: [
        { entryKind: "Card", entryName: "Bolt", severity: "warning", message: "unused" },
      ],
    });
    const user = userEvent.setup();
    render(<ProblemsPanel onNavigate={onNavigate} />);
    await waitFor(() => screen.getAllByRole("listitem"));
    await user.click(screen.getAllByRole("listitem")[0]!);
    expect(onNavigate).toHaveBeenCalledWith("cards");
  });

  it("clears diagnostics store on unmount", async () => {
    const { unmount } = render(<ProblemsPanel onNavigate={onNavigate} />);
    await waitFor(() => screen.getByText(/no problems/i));
    unmount();
    expect(useDiagnosticsStore.getState().diagnostics).toBeNull();
  });
});
