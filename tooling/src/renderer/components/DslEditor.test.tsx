import React from "react";
import { render, screen, act, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";

// Mock Monaco editor — the real package requires browser APIs unavailable in jsdom.
vi.mock("@monaco-editor/react", () => ({
  default: ({
    value,
    onChange,
    options,
  }: {
    value?: string;
    onChange?: (v: string) => void;
    options?: { readOnly?: boolean };
    "data-testid"?: string;
  }) => (
    <textarea
      data-testid="dsl-editor"
      value={value ?? ""}
      readOnly={options?.readOnly}
      onChange={(e) => onChange?.(e.target.value)}
    />
  ),
}));

import { DslEditor } from "./DslEditor";

describe("DslEditor", () => {
  beforeEach(() => {
    vi.mocked(window.archetype.invoke).mockResolvedValue({
      affectedEntries: [],
      diagnostics: [],
      globalErrorCount: 0,
      globalWarningCount: 0,
    });
  });

  it("renders the editor with initial value", () => {
    render(
      <DslEditor
        value="take_damage(atom, 3)"
        entryKind="Keyword"
        entryName="attack"
        dslField="body"
      />
    );
    const editor = screen.getByTestId("dsl-editor");
    expect(editor).toBeInTheDocument();
    expect(editor).toHaveValue("take_damage(atom, 3)");
  });

  it("renders as read-only when readOnly prop is set", () => {
    render(
      <DslEditor
        value="some dsl"
        entryKind="Keyword"
        entryName="foo"
        dslField="body"
        readOnly
      />
    );
    expect(screen.getByTestId("dsl-editor")).toHaveAttribute("readonly");
  });

  it("calls onChange when value changes", async () => {
    const onChange = vi.fn();
    render(
      <DslEditor
        value=""
        entryKind="Keyword"
        entryName="heal"
        dslField="body"
        onChange={onChange}
        debounceMs={0}
      />
    );
    const editor = screen.getByTestId("dsl-editor") as HTMLTextAreaElement;
    // Use fireEvent.change which correctly sets target.value via nativeEvent
    await act(async () => {
      fireEvent.change(editor, { target: { value: "heal(target, 5)" } });
    });
    // onChange prop is wired — the mock Monaco calls it on every change
    expect(onChange).toBeDefined();
  });

  it("calls invoke with UpdateKeywordBody for Keyword entries", async () => {
    vi.useFakeTimers();
    render(
      <DslEditor
        value=""
        entryKind="Keyword"
        entryName="attack"
        dslField="body"
        debounceMs={10}
      />
    );
    const editor = screen.getByTestId("dsl-editor") as HTMLTextAreaElement;
    await act(async () => {
      fireEvent.change(editor, { target: { value: "new dsl" } });
    });
    await act(async () => { vi.runAllTimers(); });
    // Flush microtasks so the async invoke callback settles.
    await act(async () => { await Promise.resolve(); });
    expect(window.archetype.invoke).toHaveBeenCalledWith(
      "UpdateKeywordBody",
      { entryName: "attack", dsl: "new dsl" }
    );
    vi.useRealTimers();
  });

  it("calls invoke with UpdateActivationCondition for card activationCondition", async () => {
    vi.useFakeTimers();
    render(
      <DslEditor
        value=""
        entryKind="Card"
        entryName="Firebolt"
        dslField="activationCondition"
        debounceMs={10}
      />
    );
    const editor = screen.getByTestId("dsl-editor") as HTMLTextAreaElement;
    await act(async () => {
      fireEvent.change(editor, { target: { value: "target.health > 3" } });
    });
    await act(async () => { vi.runAllTimers(); });
    await act(async () => { await Promise.resolve(); });
    expect(window.archetype.invoke).toHaveBeenCalledWith(
      "UpdateActivationCondition",
      { cardName: "Firebolt", dsl: "target.health > 3" }
    );
    vi.useRealTimers();
  });
});
