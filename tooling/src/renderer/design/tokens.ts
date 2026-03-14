/**
 * Design tokens — Catppuccin Mocha palette + shared style primitives.
 * All panels import from here for visual consistency.
 */
import type React from "react";

/** Catppuccin Mocha colour palette */
export const C = {
  bg:       "#181825", // mantle   — shell background, sidebar
  base:     "#1e1e2e", // base     — main content background
  surface0: "#313244", // surface1 — borders, dividers
  surface1: "#45475a", // surface2 — hovered items
  overlay:  "#585b70", // overlay0 — subtle separators
  muted:    "#6c7086", // overlay0 — placeholder, muted labels
  subtext:  "#a6adc8", // subtext0 — secondary text
  text:     "#cdd6f4", // text     — primary text
  accent:   "#89b4fa", // blue     — active selection, focus
  green:    "#a6e3a1", // green    — success, no errors
  yellow:   "#f9e2af", // yellow   — warnings
  red:      "#f38ba8", // red      — errors
  peach:    "#fab387", // peach    — costs, quantities
  teal:     "#94e2d5", // teal     — types, parameters
  mauve:    "#cba6f7", // mauve    — special keywords
} as const;

export const Font = {
  ui:   '"IBM Plex Sans", system-ui, sans-serif',
  mono: '"JetBrains Mono", "Fira Code", monospace',
} as const;

/** Reusable style fragments */
export const S = {
  // Text styles
  sectionLabel: {
    fontFamily: Font.ui,
    fontSize: "0.625rem",
    fontWeight: 700,
    letterSpacing: "0.1em",
    textTransform: "uppercase" as const,
    color: C.muted,
    marginBottom: "6px",
    display: "block",
  } satisfies React.CSSProperties,

  panelTitle: {
    fontFamily: Font.ui,
    fontSize: "0.6875rem",
    fontWeight: 700,
    letterSpacing: "0.08em",
    textTransform: "uppercase" as const,
    color: C.accent,
    margin: "0 0 16px",
  } satisfies React.CSSProperties,

  // Form elements
  input: {
    fontFamily: Font.ui,
    fontSize: "0.8125rem",
    color: C.text,
    background: C.bg,
    border: `1px solid ${C.surface0}`,
    borderRadius: "3px",
    padding: "4px 8px",
    outline: "none",
    width: "100%",
    boxSizing: "border-box" as const,
  } satisfies React.CSSProperties,

  select: {
    fontFamily: Font.ui,
    fontSize: "0.8125rem",
    color: C.text,
    background: C.bg,
    border: `1px solid ${C.surface0}`,
    borderRadius: "3px",
    padding: "4px 8px",
    outline: "none",
    cursor: "pointer",
  } satisfies React.CSSProperties,

  textarea: {
    fontFamily: Font.mono,
    fontSize: "0.8125rem",
    color: C.text,
    background: C.bg,
    border: `1px solid ${C.surface0}`,
    borderRadius: "3px",
    padding: "6px 8px",
    outline: "none",
    width: "100%",
    resize: "vertical" as const,
    boxSizing: "border-box" as const,
    lineHeight: 1.5,
  } satisfies React.CSSProperties,

  // Buttons
  btnPrimary: {
    fontFamily: Font.ui,
    fontSize: "0.75rem",
    fontWeight: 600,
    padding: "5px 14px",
    borderRadius: "3px",
    border: "none",
    cursor: "pointer",
    background: C.accent,
    color: C.bg,
    lineHeight: 1,
  } satisfies React.CSSProperties,

  btnGhost: {
    fontFamily: Font.ui,
    fontSize: "0.75rem",
    fontWeight: 500,
    padding: "4px 10px",
    borderRadius: "3px",
    border: `1px solid ${C.surface0}`,
    cursor: "pointer",
    background: "transparent",
    color: C.subtext,
    lineHeight: 1,
  } satisfies React.CSSProperties,

  btnDanger: {
    fontFamily: Font.ui,
    fontSize: "0.75rem",
    fontWeight: 500,
    padding: "4px 10px",
    borderRadius: "3px",
    border: `1px solid ${C.red}`,
    cursor: "pointer",
    background: "transparent",
    color: C.red,
    lineHeight: 1,
  } satisfies React.CSSProperties,

  btnIcon: {
    fontFamily: Font.mono,
    fontSize: "0.75rem",
    padding: "3px 6px",
    borderRadius: "3px",
    border: "none",
    cursor: "pointer",
    background: "transparent",
    color: C.muted,
    lineHeight: 1,
  } satisfies React.CSSProperties,

  // Layout
  panelRow: {
    display: "flex",
    height: "100%",
    overflow: "hidden",
    fontFamily: Font.ui,
    color: C.text,
    fontSize: "0.8125rem",
  } satisfies React.CSSProperties,

  scrollPane: {
    overflowY: "auto" as const,
    height: "100%",
  } satisfies React.CSSProperties,

  divider: {
    borderTop: `1px solid ${C.surface0}`,
    margin: "12px 0",
  } satisfies React.CSSProperties,

  // Cards / rows
  listItem: {
    padding: "6px 12px",
    cursor: "pointer",
    borderBottom: `1px solid ${C.surface0}`,
    fontSize: "0.8125rem",
    userSelect: "none" as const,
  } satisfies React.CSSProperties,

  listItemActive: {
    background: C.base,
    borderLeft: `2px solid ${C.accent}`,
    paddingLeft: "10px",
    color: C.accent,
  } satisfies React.CSSProperties,

  // Accordion
  accordionHeader: {
    display: "flex",
    alignItems: "center",
    gap: "8px",
    padding: "8px 12px",
    cursor: "pointer",
    userSelect: "none" as const,
    background: C.bg,
    borderBottom: `1px solid ${C.surface0}`,
    fontSize: "0.8125rem",
    fontWeight: 600,
  } satisfies React.CSSProperties,

  // Drag handle
  dragHandle: {
    cursor: "grab",
    color: C.muted,
    fontSize: "0.875rem",
    padding: "0 6px",
    lineHeight: 1,
    userSelect: "none" as const,
  } satisfies React.CSSProperties,
} as const;
