import React from "react";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, beforeEach } from "vitest";
import { App } from "../App";

// ---------------------------------------------------------------------------
// App smoke tests (Task 2.6)
//
// Verifies that:
//   1. The app renders without throwing.
//   2. The sidebar navigation is present.
//   3. Clicking a nav item updates the main panel heading.
//   4. The status bar is present.
//
// No IPC calls are made by the App shell itself — the mock from setup.ts is
// sufficient.  Stores are in their initial (empty) state for all tests here.
// ---------------------------------------------------------------------------

describe("App", () => {
  beforeEach(() => {
    // Reset Zustand stores between tests by re-importing (stores hold module
    // state; vitest isolates modules per test file but not per test case).
    // For this smoke test the default initial state is sufficient — no reset
    // needed since no actions are dispatched.
  });

  it("renders without crashing", () => {
    // If this throws, the Vite + React + Vitest pipeline is broken.
    expect(() => render(<App />)).not.toThrow();
  });

  it("renders the sidebar navigation", () => {
    render(<App />);
    expect(screen.getByRole("navigation", { name: "Main navigation" })).toBeInTheDocument();
  });

  it("shows Keywords as the default active panel", () => {
    render(<App />);
    // The placeholder panel heading should read "Keywords".
    expect(screen.getByRole("heading", { name: "Keywords" })).toBeInTheDocument();
  });

  it("navigates to Cards panel when Cards nav item is clicked", async () => {
    const user = userEvent.setup();
    render(<App />);

    // Click the "Cards" nav item.
    const cardsNavItem = screen.getByRole("option", { name: "Cards" });
    await user.click(cardsNavItem);

    // The main content area should now show the Cards placeholder.
    expect(screen.getByRole("heading", { name: "Cards" })).toBeInTheDocument();
  });

  it("renders the status bar", () => {
    render(<App />);
    expect(screen.getByRole("status")).toBeInTheDocument();
  });
});
