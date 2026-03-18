/**
 * DslEditor — Monaco editor wrapper for archetype-dsl (Task 6.1, D26, D28).
 *
 * • Registers the `archetype-dsl` language with basic token colouring.
 * • Debounces content changes (default 200 ms) then calls the appropriate
 *   sidecar mutation channel based on `entryKind` + `dslField`.
 * • Accepts `markers` prop (offset-already-converted to line/col) and forwards
 *   them to `monaco.editor.setModelMarkers`.
 * • Registers a CompletionItemProvider that calls `GetCompletions`.
 */
import React, { useRef, useCallback, useEffect } from "react";
import MonacoEditor from "@monaco-editor/react";
import type { OnMount, BeforeMount, Monaco } from "@monaco-editor/react";
import { IPC_CHANNELS } from "@shared/ipc";
import type { GetCompletionsResult, MutationResponse } from "@shared/ipc";
import { useDiagnosticsStore } from "../store/diagnosticsStore";
import { useProjectStore } from "../store/projectStore";
import { diagnosticToMarker } from "../snapshot";
import type { MarkerData } from "../snapshot";
import { C, Font } from "../design/tokens";

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export interface DslEditorProps {
  /** Current DSL content (controlled by parent). */
  value: string;
  /** Entry kind string ("Keyword", "Card", …). */
  entryKind: string;
  /** Entry name (keyword name, card name, …). */
  entryName: string;
  /**
   * Which DSL field this editor represents.
   * For keywords: "body"
   * For cards:    "primaryEffect" | "activationCondition" | effect-name | "lifetime:<i>"
   */
  dslField: string;
  /** Inline error/warning markers derived from last MutationResponse. */
  markers?: readonly MarkerData[];
  /** Debounce delay in ms (default 200). */
  debounceMs?: number;
  /** Called with the new DSL string on every debounced change. */
  onChange?: (dsl: string) => void;
  /** Called with updated markers after a successful mutation. */
  onMarkersChange?: (markers: MarkerData[]) => void;
  /**
   * Called after every successful debounced mutation to the sidecar.
   * Use this to invalidate parent state that depends on the committed value
   * (e.g. refreshing the keyword list after a body edit so that re-mounting
   * `KeywordDetail` with `key={name}` picks up the correct body).
   */
  onAfterCommit?: () => void;
  readOnly?: boolean;
  height?: string;
}

// ---------------------------------------------------------------------------
// Language registration (done once per Monaco instance)
// ---------------------------------------------------------------------------

let languageRegistered = false;
// Completion provider must be registered once per Monaco instance — registering
// on every mount accumulates duplicate providers (Monaco does not deduplicate).
let completionProviderRegistered = false;

function registerLanguage(monaco: Monaco): void {
  if (languageRegistered) return;
  languageRegistered = true;

  monaco.languages.register({ id: "archetype-dsl" });

  monaco.languages.setMonarchTokensProvider("archetype-dsl", {
    keywords: [
      "take_damage", "attack", "heal", "draw", "discard", "destroy",
      "create", "move", "max", "min", "if", "else", "return", "for", "while",
    ],
    typeKeywords: ["Atom", "Int", "Bool", "String", "Player", "Zone"],
    tokenizer: {
      root: [
        [/\/\/.*$/, "comment"],
        [/"[^"]*"/, "string"],
        [/\d+(\.\d+)?/, "number"],
        [/[a-zA-Z_]\w*/, {
          cases: {
            "@keywords":     "keyword",
            "@typeKeywords": "type",
            "@default":      "identifier",
          },
        }],
        [/[()[\]{},.]/, "delimiter"],
        [/[-+*/=<>!&|]/, "operator"],
      ],
    },
  });

  monaco.editor.defineTheme("archetype-dark", {
    base: "vs-dark",
    inherit: true,
    rules: [
      { token: "keyword",    foreground: "cba6f7", fontStyle: "bold" },
      { token: "type",       foreground: "94e2d5" },
      { token: "comment",    foreground: "6c7086", fontStyle: "italic" },
      { token: "string",     foreground: "a6e3a1" },
      { token: "number",     foreground: "fab387" },
      { token: "delimiter",  foreground: "a6adc8" },
      { token: "operator",   foreground: "89b4fa" },
      { token: "identifier", foreground: "cdd6f4" },
    ],
    colors: {
      "editor.background":           "#181825",
      "editor.foreground":           "#cdd6f4",
      "editor.lineHighlightBackground": "#1e1e2e",
      "editorLineNumber.foreground": "#585b70",
      "editorCursor.foreground":     "#89b4fa",
      "editor.selectionBackground":  "#313244",
      "editorIndentGuide.background1": "#313244",
    },
  });
}

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

export function DslEditor({
  value,
  entryKind,
  entryName,
  dslField,
  markers = [],
  debounceMs = 200,
  onChange,
  onMarkersChange,
  onAfterCommit,
  readOnly = false,
  height = "160px",
}: DslEditorProps): React.ReactElement {
  const editorRef  = useRef<Parameters<OnMount>[0] | null>(null);
  const monacoRef  = useRef<Monaco | null>(null);
  const timerRef   = useRef<ReturnType<typeof setTimeout> | null>(null);
  const currentDsl = useRef(value);

  const updateCounts = useDiagnosticsStore((s) => s.updateCounts);
  const isMutating   = useProjectStore((s) => s.isMutating);
  const setMutating  = useProjectStore((s) => s.setMutating);
  const markDirty    = useProjectStore((s) => s.markDirty);

  // Apply markers whenever the prop changes
  useEffect(() => {
    const editor = editorRef.current;
    const monaco = monacoRef.current;
    if (!editor || !monaco) return;
    const model = editor.getModel();
    if (!model) return;
    monaco.editor.setModelMarkers(model, "archetype", [...markers]);
  }, [markers]);

  // Keep currentDsl ref in sync
  useEffect(() => { currentDsl.current = value; }, [value]);

  const beforeMount: BeforeMount = useCallback((monaco) => {
    registerLanguage(monaco);
  }, []);

  const onMount: OnMount = useCallback((editor, monaco) => {
    editorRef.current  = editor;
    monacoRef.current  = monaco;

    // Apply any markers that arrived before the editor was ready
    const model = editor.getModel();
    if (model && markers.length > 0) {
      monaco.editor.setModelMarkers(model, "archetype", [...markers]);
    }

    // Register the completion provider once per Monaco instance. Monaco
    // accumulates rather than deduplicates, so a second registration would
    // produce N copies of every suggestion (same pattern as languageRegistered).
    if (!completionProviderRegistered) {
      completionProviderRegistered = true;
      monaco.languages.registerCompletionItemProvider("archetype-dsl", {
        triggerCharacters: [".", "("],
        provideCompletionItems: async (model, position) => {
          const offset   = model.getOffsetAt(position);
          const wordInfo = model.getWordUntilPosition(position);
          try {
            const result = await window.archetype.invoke<GetCompletionsResult>(
              IPC_CHANNELS.GetCompletions,
              { entryKind, entryName, dslField, offset, prefix: wordInfo.word }
            );
            return {
              suggestions: result.items.map((item) => ({
                label:      item.label,
                kind:       completionKind(monaco, item.kind),
                detail:     item.detail ?? undefined,
                // Use sidecar-supplied snippet template when present (D28);
                // fall back to label for plain identifiers.
                insertText: item.insertText ?? item.label,
                range: {
                  startLineNumber: position.lineNumber,
                  startColumn:     wordInfo.startColumn,
                  endLineNumber:   position.lineNumber,
                  endColumn:       wordInfo.endColumn,
                },
              })),
            };
          } catch {
            return { suggestions: [] };
          }
        },
      });
    }
  }, [entryKind, entryName, dslField, markers]);

  const handleChange = useCallback((newValue: string | undefined) => {
    if (newValue === undefined) return;
    currentDsl.current = newValue;
    onChange?.(newValue);

    if (timerRef.current !== null) clearTimeout(timerRef.current);
    timerRef.current = setTimeout(async () => {
      if (isMutating) return;
      setMutating(true);
      try {
        const [channel, payload] = resolveMutation(entryKind, entryName, dslField, newValue);
        const resp = await window.archetype.invoke<MutationResponse>(channel, payload);
        markDirty();
        updateCounts(resp.globalErrorCount, resp.globalWarningCount);

        if (onMarkersChange) {
          const relevantDiags = resp.diagnostics.filter(
            (d) => d.entryName === entryName
          );
          onMarkersChange(relevantDiags.map((d) => diagnosticToMarker(newValue, d)));
        }
        // Notify the parent that a commit succeeded so it can refresh any
        // stale snapshot-derived state (e.g. keyword body in parent's list).
        onAfterCommit?.();
      } catch {
        // Errors surface as diagnostics; swallow network/IPC errors
      } finally {
        setMutating(false);
      }
    }, debounceMs);
  }, [entryKind, entryName, dslField, debounceMs, isMutating, onChange,
      onMarkersChange, onAfterCommit, updateCounts, setMutating, markDirty]);

  return (
    <div style={{ border: `1px solid ${C.surface0}`, borderRadius: "3px", overflow: "hidden" }}>
      <MonacoEditor
        height={height}
        language="archetype-dsl"
        value={value}
        theme="archetype-dark"
        beforeMount={beforeMount}
        onMount={onMount}
        onChange={handleChange}
        options={{
          readOnly,
          minimap:             { enabled: false },
          lineNumbers:         "on",
          scrollBeyondLastLine: false,
          fontSize:            13,
          fontFamily:          Font.mono,
          lineHeight:          20,
          padding:             { top: 8, bottom: 8 },
          overviewRulerLanes:  0,
          renderLineHighlight: "gutter",
          scrollbar:           { vertical: "auto", horizontal: "hidden" },
          wordWrap:            "on",
          glyphMargin:         true,
          folding:             false,
          lineDecorationsWidth: 4,
        }}
      />
    </div>
  );
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function resolveMutation(
  entryKind: string,
  entryName: string,
  dslField: string,
  dsl: string
): [string, unknown] {
  if (entryKind === "Keyword") {
    return [IPC_CHANNELS.UpdateKeywordBody, { entryName, dsl }];
  }
  if (entryKind === "Card") {
    if (dslField === "activationCondition") {
      return [IPC_CHANNELS.UpdateActivationCondition, { cardName: entryName, dsl }];
    }
    if (dslField.startsWith("lifetime:")) {
      const idx = parseInt(dslField.split(":")[1] ?? "0", 10);
      return [IPC_CHANNELS.UpdateLifetimeSpec, { cardName: entryName, staticEffectIndex: idx, dsl }];
    }
    if (dslField.startsWith("cost:")) {
      const idx = parseInt(dslField.split(":")[1] ?? "0", 10);
      return [IPC_CHANNELS.UpdateCostBody, { cardName: entryName, costIndex: idx, dsl }];
    }
    // primaryEffect or named additional effect
    const effectName = dslField === "primaryEffect" ? null : dslField;
    return [IPC_CHANNELS.UpdateCardEffect, { cardName: entryName, effectName, dsl }];
  }
  // Generic fallback
  return [IPC_CHANNELS.UpdateField, { entryKind, entryName, field: dslField, value: dsl }];
}

function completionKind(
  monaco: Monaco,
  kind: string
): Monaco["languages"]["CompletionItemKind"][keyof Monaco["languages"]["CompletionItemKind"]] {
  const k = monaco.languages.CompletionItemKind;
  switch (kind) {
    case "Function": return k.Function;
    case "Variable": return k.Variable;
    case "Keyword":  return k.Keyword;
    case "Type":     return k.Class;
    default:         return k.Text;
  }
}
