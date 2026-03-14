import React from "react";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, afterEach } from "vitest";
import { App } from "../App";
import { useProjectStore } from "../store/projectStore";

// ---------------------------------------------------------------------------
// App smoke tests (Task 2.6)
//
// Verifies that:
//   1. The app renders without throwing.
//   2. The sidebar navigation is present.
//   3. Welcome screen is shown with no project open.
//   4. Clicking a nav item updates the main panel heading (with project loaded).
//   5. The status bar is present.
// ---------------------------------------------------------------------------

function loadProject(): void {
  useProjectStore.getState().setProject("/fake/project.archetype", "test-project");
}

describe("App", () => {
  afterEach(() => {
    // Clear project state between tests.
    useProjectStore.getState().clearProject();
  });

  it("renders without crashing", () => {
    expect(() => render(<App />)).not.toThrow();
  });

  it("renders the sidebar navigation", () => {
    render(<App />);
    expect(screen.getByRole("navigation", { name: "Main navigation" })).toBeInTheDocument();
  });

  it("shows welcome screen when no project is open", () => {
    render(<App />);
    expect(screen.getByRole("region", { name: "Welcome screen" })).toBeInTheDocument();
  });

  it("shows Keywords as the default active panel when a project is open", () => {
    loadProject();
    render(<App />);
    expect(screen.getByRole("heading", { name: "Keywords" })).toBeInTheDocument();
  });

  it("navigates to Cards panel when Cards nav item is clicked", async () => {
    loadProject();
    const user = userEvent.setup();
    render(<App />);

    const cardsNavItem = screen.getByRole("option", { name: "Cards" });
    await user.click(cardsNavItem);

    expect(screen.getByRole("heading", { name: "Cards" })).toBeInTheDocument();
  });

  it("renders the status bar", () => {
    render(<App />);
    expect(screen.getByRole("status")).toBeInTheDocument();
  });
});
