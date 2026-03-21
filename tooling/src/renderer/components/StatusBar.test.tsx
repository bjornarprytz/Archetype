import React from "react";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { useDiagnosticsStore } from "../store/diagnosticsStore";
import { useProjectStore } from "../store/projectStore";
import { StatusBar } from "./StatusBar";

describe("StatusBar", () => {
  const navigate = vi.fn();

  beforeEach(() => {
    navigate.mockClear();
    useDiagnosticsStore.setState({ globalErrorCount: 0, globalWarningCount: 0, diagnostics: null });
    useProjectStore.setState({ projectName: null, isDirty: false, sidecarStatus: "disconnected" });
  });

  it("renders without crashing", () => {
    expect(() => render(<StatusBar onNavigate={navigate} />)).not.toThrow();
  });

  it('shows "no project open" when no project is loaded', () => {
    render(<StatusBar onNavigate={navigate} />);
    expect(screen.getByText(/no project open/i)).toBeInTheDocument();
  });

  it("shows project name when a project is open", () => {
    useProjectStore.setState({ projectName: "my-game" });
    render(<StatusBar onNavigate={navigate} />);
    expect(screen.getByText(/my-game/)).toBeInTheDocument();
  });

  it("shows dirty indicator when project is dirty", () => {
    useProjectStore.setState({ projectName: "my-game", isDirty: true });
    render(<StatusBar onNavigate={navigate} />);
    expect(screen.getByText(/my-game.*●/)).toBeInTheDocument();
  });

  it("displays error count from store", () => {
    useDiagnosticsStore.setState({ globalErrorCount: 3, globalWarningCount: 1 });
    render(<StatusBar onNavigate={navigate} />);
    expect(screen.getByLabelText("3 errors")).toBeInTheDocument();
    expect(screen.getByLabelText("1 warnings")).toBeInTheDocument();
  });

  it("calls onNavigate('problems') when error pill is clicked", async () => {
    useDiagnosticsStore.setState({ globalErrorCount: 2, globalWarningCount: 0 });
    const user = userEvent.setup();
    render(<StatusBar onNavigate={navigate} />);
    await user.click(screen.getByLabelText("2 errors"));
    expect(navigate).toHaveBeenCalledWith("problems");
  });

  it("calls onNavigate('problems') when warning pill is clicked", async () => {
    useDiagnosticsStore.setState({ globalErrorCount: 0, globalWarningCount: 4 });
    const user = userEvent.setup();
    render(<StatusBar onNavigate={navigate} />);
    await user.click(screen.getByLabelText("4 warnings"));
    expect(navigate).toHaveBeenCalledWith("problems");
  });
});
