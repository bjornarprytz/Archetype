import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { ExportModal } from "./ExportModal";

const summary = [
  { locale: "de", missingCount: 4 },
  { locale: "fr", missingCount: 2 },
];

describe("ExportModal", () => {
  const onClose    = vi.fn();
  const onExported = vi.fn();

  beforeEach(() => {
    onClose.mockClear();
    onExported.mockClear();
    vi.mocked(window.archetype.invoke).mockResolvedValue({
      kind: "success",
      json: '{"id":"test"}',
    });
  });

  it("renders without crashing", () => {
    expect(() =>
      render(<ExportModal summary={summary} onClose={onClose} onExported={onExported} />)
    ).not.toThrow();
  });

  it("displays missing translation summary", () => {
    render(<ExportModal summary={summary} onClose={onClose} onExported={onExported} />);
    expect(screen.getByText("de")).toBeInTheDocument();
    expect(screen.getByText("4")).toBeInTheDocument();
    expect(screen.getByText("fr")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
  });

  it("shows total missing count in description", () => {
    render(<ExportModal summary={summary} onClose={onClose} onExported={onExported} />);
    expect(screen.getByText(/6 strings/i)).toBeInTheDocument();
  });

  it("calls onClose when Cancel is clicked", async () => {
    const user = userEvent.setup();
    render(<ExportModal summary={summary} onClose={onClose} onExported={onExported} />);
    await user.click(screen.getByText("Cancel"));
    expect(onClose).toHaveBeenCalledOnce();
  });

  it("calls invoke with force:true and then onExported on export anyway", async () => {
    const user = userEvent.setup();
    render(<ExportModal summary={summary} onClose={onClose} onExported={onExported} />);
    await user.click(screen.getByRole("button", { name: /export anyway/i }));
    await waitFor(() => {
      expect(window.archetype.invoke).toHaveBeenCalledWith(
        "ExportGameDefinition",
        { force: true }
      );
    });
    await waitFor(() => expect(onExported).toHaveBeenCalledWith('{"id":"test"}'));
    expect(onClose).toHaveBeenCalled();
  });

  it("shows error message when export fails with errors kind", async () => {
    vi.mocked(window.archetype.invoke).mockResolvedValue({
      kind: "errors",
      globalErrorCount: 5,
    });
    const user = userEvent.setup();
    render(<ExportModal summary={summary} onClose={onClose} onExported={onExported} />);
    await user.click(screen.getByRole("button", { name: /export anyway/i }));
    await waitFor(() =>
      expect(screen.getByRole("alert")).toHaveTextContent(/5 error/i)
    );
    expect(onExported).not.toHaveBeenCalled();
  });

  it("has accessible dialog role and label", () => {
    render(<ExportModal summary={summary} onClose={onClose} onExported={onExported} />);
    expect(screen.getByRole("dialog")).toBeInTheDocument();
  });
});
