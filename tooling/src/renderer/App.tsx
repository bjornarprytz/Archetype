import React, { useState } from "react";
import { KeywordEditorPanel }  from "./panels/KeywordEditorPanel";
import { CardEditorPanel }     from "./panels/CardEditorPanel";
import { GameRulesPanel }      from "./panels/GameRulesPanel";
import { InitManifestPanel }   from "./panels/InitManifestPanel";
import { LocalizationPanel }   from "./panels/LocalizationPanel";
import { GraphPanel }          from "./panels/GraphPanel";
import { SetOverviewPanel }    from "./panels/SetOverviewPanel";
import { ProblemsPanel }       from "./panels/ProblemsPanel";
import { StatusBar }           from "./components/StatusBar";
import { useProjectStore }     from "./store/projectStore";
import { IPC_CHANNELS }        from "@shared/ipc";
import type { LoadProjectResult } from "@shared/ipc";
import { C, Font }             from "./design/tokens";

// Minimal empty project accepted by LoadProject.
const EMPTY_PROJECT_JSON = JSON.stringify({
  keywords: [],
  cards: [],
  zones: [],
  phases: [],
  actionRules: [],
  stateBasedRules: [],
  triggerResolutionOrder: "Chronological",
  initManifest: [],
  sourceLanguage: "en",
  locales: [],
});

// ---------------------------------------------------------------------------
// Navigation model
// ---------------------------------------------------------------------------

type PanelId =
  | "keywords"
  | "cards"
  | "gameRules"
  | "initManifest"
  | "localization"
  | "graph"
  | "setOverview"
  | "problems";

interface NavItem {
  readonly id:    PanelId;
  readonly label: string;
  readonly glyph: string;
}

interface NavGroup {
  readonly heading: string;
  readonly items:   readonly NavItem[];
}

const NAV_GROUPS: readonly NavGroup[] = [
  {
    heading: "Design",
    items: [
      { id: "keywords",     label: "Keywords",    glyph: "ƒ" },
      { id: "cards",        label: "Cards",        glyph: "◇" },
    ],
  },
  {
    heading: "Rules",
    items: [
      { id: "gameRules",    label: "Game Rules",   glyph: "≡" },
      { id: "initManifest", label: "Init Manifest", glyph: "◎" },
    ],
  },
  {
    heading: "Tools",
    items: [
      { id: "localization", label: "Localization", glyph: "⊞" },
      { id: "graph",        label: "Graph",         glyph: "⊹" },
      { id: "setOverview",  label: "Set Overview",  glyph: "◈" },
      { id: "problems",     label: "Problems",      glyph: "⊘" },
    ],
  },
] as const;

// ---------------------------------------------------------------------------
// App shell
// ---------------------------------------------------------------------------

/**
 * Root application shell.
 *
 * Layout: fixed-width sidebar (nav) + flex-grow main panel + status bar footer.
 * Sidebar drives `activePanel`; every panel receives `onNavigate` so panels
 * can programmatically switch (e.g. ProblemsPanel links to relevant panels).
 */
export function App(): React.ReactElement {
  const [activePanel, setActivePanel] = useState<PanelId>("keywords");

  const projectName = useProjectStore((s) => s.projectName);
  const setProject  = useProjectStore((s) => s.setProject);

  const hasProject = projectName !== null;

  function navigate(panel: string): void {
    setActivePanel(panel as PanelId);
  }

  function handleProjectLoaded(path: string | null, name: string | null): void {
    setProject(path, name);
    setActivePanel("keywords");
  }

  return (
    <div style={styles.shell}>
      {/* ------------------------------------------------------------------ */}
      {/* Sidebar                                                              */}
      {/* ------------------------------------------------------------------ */}
      <nav style={styles.sidebar} aria-label="Main navigation">
        {/* Wordmark */}
        <div style={styles.wordmark}>
          <span style={styles.wordmarkText}>Archetype</span>
          <span style={styles.wordmarkDot} aria-hidden="true" />
        </div>

        {/* Navigation groups — only when a project is open */}
        {hasProject && (
          <div style={styles.navScroll}>
            {NAV_GROUPS.map((group) => (
              <div key={group.heading} style={styles.navGroup}>
                <span style={styles.groupHeading} aria-hidden="true">
                  {group.heading}
                </span>
                <ul style={styles.navList} role="listbox" aria-label={group.heading}>
                  {group.items.map((item) => (
                    <NavItemRow
                      key={item.id}
                      item={item}
                      active={activePanel === item.id}
                      onClick={() => setActivePanel(item.id)}
                    />
                  ))}
                </ul>
              </div>
            ))}
          </div>
        )}

        {/* Project buttons always visible at sidebar bottom */}
        <div style={styles.sidebarFooter}>
          <ProjectButtons onProjectLoaded={handleProjectLoaded} />
        </div>
      </nav>

      {/* ------------------------------------------------------------------ */}
      {/* Main content                                                         */}
      {/* ------------------------------------------------------------------ */}
      <main style={styles.main}>
        {/* panelArea: flex:1 + min-height:0 so children's height:100% resolves
            against the content area (after padding), not the padded container.
            Without min-height:0 flex items won't shrink below their content. */}
        <div style={styles.panelArea}>
          {hasProject
            ? <ActivePanel panelId={activePanel} onNavigate={navigate} />
            : <WelcomeView onProjectLoaded={handleProjectLoaded} />
          }
        </div>
      </main>

      {/* ------------------------------------------------------------------ */}
      {/* Status bar                                                           */}
      {/* ------------------------------------------------------------------ */}
      <StatusBar onNavigate={navigate} />
    </div>
  );
}

// ---------------------------------------------------------------------------
// WelcomeView — shown when no project is open
// ---------------------------------------------------------------------------

function WelcomeView({
  onProjectLoaded,
}: {
  onProjectLoaded: (path: string | null, name: string | null) => void;
}): React.ReactElement {
  const [busy,  setBusy]  = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleNew(): Promise<void> {
    setBusy(true);
    setError(null);
    try {
      const savePath = await window.archetype.invoke<string | null>(
        IPC_CHANNELS.ShowSaveDialog,
        { title: "Create New Project", defaultPath: "untitled.archetype",
          filters: [{ name: "Archetype Project", extensions: ["archetype"] }] }
      );
      if (!savePath) return;
      const result = await window.archetype.invoke<LoadProjectResult>(
        IPC_CHANNELS.LoadProject, { json: EMPTY_PROJECT_JSON }
      );
      await window.archetype.invoke(IPC_CHANNELS.WriteFile,
        { path: savePath, content: EMPTY_PROJECT_JSON });
      onProjectLoaded(savePath, result?.id ?? null);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to create project.");
    } finally { setBusy(false); }
  }

  async function handleOpen(): Promise<void> {
    setBusy(true);
    setError(null);
    try {
      const filePath = await window.archetype.invoke<string | null>(
        IPC_CHANNELS.ShowOpenDialog,
        { title: "Open Project",
          filters: [{ name: "Archetype Project", extensions: ["archetype"] }],
          properties: ["openFile"] }
      );
      if (!filePath) return;
      const json = await window.archetype.invoke<string>(
        IPC_CHANNELS.ReadFile, { path: filePath }
      );
      const result = await window.archetype.invoke<LoadProjectResult>(
        IPC_CHANNELS.LoadProject, { json }
      );
      onProjectLoaded(filePath, result?.id ?? null);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to open project.");
    } finally { setBusy(false); }
  }

  return (
    <div style={welcome.root} role="region" aria-label="Welcome screen">
      <div style={welcome.card}>
        <div style={welcome.logo}>A</div>
        <h1 style={welcome.title}>Archetype</h1>
        <p style={welcome.subtitle}>Card game definition editor</p>
        <div style={welcome.actions}>
          <button style={welcome.btnPrimary} onClick={() => { void handleNew(); }} disabled={busy}>
            <span style={welcome.glyph}>+</span> New Project
          </button>
          <button style={welcome.btnSecondary} onClick={() => { void handleOpen(); }} disabled={busy}>
            <span style={welcome.glyph}>⊡</span> Open Project
          </button>
        </div>
        {busy  && <p style={welcome.hint}>Loading…</p>}
        {error && <p style={{ ...welcome.hint, color: C.red }} role="alert">{error}</p>}
      </div>
    </div>
  );
}

const welcome = {
  root: {
    height: "100%", display: "flex", alignItems: "center",
    justifyContent: "center", fontFamily: Font.ui,
  } satisfies React.CSSProperties,
  card: {
    display: "flex", flexDirection: "column" as const, alignItems: "center",
    gap: "8px", padding: "40px 48px", background: C.bg,
    border: `1px solid ${C.surface0}`, borderRadius: "6px", minWidth: "300px",
  } satisfies React.CSSProperties,
  logo: {
    width: "40px", height: "40px", borderRadius: "8px", background: C.accent,
    color: C.bg, fontFamily: Font.mono, fontSize: "1.25rem", fontWeight: 700,
    display: "flex", alignItems: "center", justifyContent: "center",
    marginBottom: "8px",
  } satisfies React.CSSProperties,
  title: {
    fontFamily: Font.ui, fontSize: "1.25rem", fontWeight: 700,
    color: C.text, margin: 0, letterSpacing: "0.02em",
  } satisfies React.CSSProperties,
  subtitle: {
    fontSize: "0.8rem", color: C.muted, margin: "0 0 24px",
  } satisfies React.CSSProperties,
  actions: {
    display: "flex", flexDirection: "column" as const, gap: "8px", width: "100%",
  } satisfies React.CSSProperties,
  btnPrimary: {
    fontFamily: Font.ui, fontSize: "0.8125rem", fontWeight: 600,
    padding: "10px 16px", borderRadius: "4px", border: "none", cursor: "pointer",
    background: C.accent, color: C.bg, display: "flex", alignItems: "center",
    gap: "8px", justifyContent: "center",
  } satisfies React.CSSProperties,
  btnSecondary: {
    fontFamily: Font.ui, fontSize: "0.8125rem", fontWeight: 500,
    padding: "9px 16px", borderRadius: "4px", border: `1px solid ${C.surface0}`,
    cursor: "pointer", background: "transparent", color: C.subtext,
    display: "flex", alignItems: "center", gap: "8px", justifyContent: "center",
  } satisfies React.CSSProperties,
  glyph: {
    fontFamily: Font.mono, fontSize: "0.875rem",
  } satisfies React.CSSProperties,
  hint: {
    fontSize: "0.75rem", color: C.muted, margin: "8px 0 0", textAlign: "center" as const,
  } satisfies React.CSSProperties,
} as const;

// ---------------------------------------------------------------------------
// ProjectButtons — compact New / Open in the sidebar footer
// ---------------------------------------------------------------------------

function ProjectButtons({
  onProjectLoaded,
}: {
  onProjectLoaded: (path: string | null, name: string | null) => void;
}): React.ReactElement {
  const [busy, setBusy] = useState(false);

  async function handleNew(): Promise<void> {
    if (busy) return;
    setBusy(true);
    try {
      const savePath = await window.archetype.invoke<string | null>(
        IPC_CHANNELS.ShowSaveDialog,
        { title: "Create New Project", defaultPath: "untitled.archetype",
          filters: [{ name: "Archetype Project", extensions: ["archetype"] }] }
      );
      if (!savePath) return;
      const result = await window.archetype.invoke<LoadProjectResult>(
        IPC_CHANNELS.LoadProject, { json: EMPTY_PROJECT_JSON }
      );
      await window.archetype.invoke(IPC_CHANNELS.WriteFile,
        { path: savePath, content: EMPTY_PROJECT_JSON });
      onProjectLoaded(savePath, result?.id ?? null);
    } catch { /* ignore */ }
    finally { setBusy(false); }
  }

  async function handleOpen(): Promise<void> {
    if (busy) return;
    setBusy(true);
    try {
      const filePath = await window.archetype.invoke<string | null>(
        IPC_CHANNELS.ShowOpenDialog,
        { title: "Open Project",
          filters: [{ name: "Archetype Project", extensions: ["archetype"] }],
          properties: ["openFile"] }
      );
      if (!filePath) return;
      const json = await window.archetype.invoke<string>(
        IPC_CHANNELS.ReadFile, { path: filePath }
      );
      const result = await window.archetype.invoke<LoadProjectResult>(
        IPC_CHANNELS.LoadProject, { json }
      );
      onProjectLoaded(filePath, result?.id ?? null);
    } catch { /* ignore */ }
    finally { setBusy(false); }
  }

  return (
    <div style={footerBtns.root}>
      <button style={footerBtns.btn} onClick={() => { void handleNew(); }} disabled={busy} title="New project" aria-label="New project">New</button>
      <button style={footerBtns.btn} onClick={() => { void handleOpen(); }} disabled={busy} title="Open project" aria-label="Open project">Open</button>
    </div>
  );
}

const footerBtns = {
  root: {
    display: "flex", gap: "6px", padding: "8px 10px 10px",
    borderTop: `1px solid ${C.surface0}`,
  } satisfies React.CSSProperties,
  btn: {
    flex: 1, fontFamily: Font.ui, fontSize: "0.7rem", fontWeight: 500,
    padding: "5px 0", borderRadius: "3px", border: `1px solid ${C.surface0}`,
    cursor: "pointer", background: "transparent", color: C.subtext, userSelect: "none" as const,
  } satisfies React.CSSProperties,
} as const;

// ---------------------------------------------------------------------------
// NavItemRow — handles its own hover state
// ---------------------------------------------------------------------------

function NavItemRow({
  item, active, onClick,
}: {
  item: NavItem;
  active: boolean;
  onClick: () => void;
}): React.ReactElement {
  const [hovered, setHovered] = useState(false);

  const itemStyle: React.CSSProperties = {
    ...styles.navItem,
    ...(active
      ? styles.navItemActive
      : hovered
      ? styles.navItemHover
      : {}),
  };

  return (
    <li
      role="option"
      aria-selected={active}
      style={itemStyle}
      onClick={onClick}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
    >
      <span style={{ ...styles.navGlyph, color: active ? C.accent : C.overlay }} aria-hidden="true">
        {item.glyph}
      </span>
      {item.label}
    </li>
  );
}

// ---------------------------------------------------------------------------
// ActivePanel — renders the selected panel
// ---------------------------------------------------------------------------

function ActivePanel({
  panelId, onNavigate,
}: {
  panelId: PanelId;
  onNavigate: (panel: string) => void;
}): React.ReactElement {
  switch (panelId) {
    case "keywords":     return <KeywordEditorPanel />;
    case "cards":        return <CardEditorPanel />;
    case "gameRules":    return <GameRulesPanel />;
    case "initManifest": return <InitManifestPanel />;
    case "localization": return <LocalizationPanel />;
    case "graph":        return <GraphPanel onNavigate={onNavigate} />;
    case "setOverview":  return <SetOverviewPanel />;
    case "problems":     return <ProblemsPanel onNavigate={onNavigate} />;
  }
}

// ---------------------------------------------------------------------------
// Styles
// ---------------------------------------------------------------------------

const styles = {
  shell: {
    display: "grid",
    gridTemplateColumns: "188px 1fr",
    gridTemplateRows:    "1fr 26px",
    gridTemplateAreas:   '"sidebar main" "sidebar statusbar"',
    height:              "100vh",
    overflow:            "hidden",
    background:          C.base,
    fontFamily:          Font.ui,
  } satisfies React.CSSProperties,

  sidebar: {
    gridArea:      "sidebar",
    background:    C.bg,
    borderRight:   `1px solid ${C.surface0}`,
    display:       "flex",
    flexDirection: "column" as const,
    overflow:      "hidden",
  } satisfies React.CSSProperties,

  wordmark: {
    display:      "flex",
    alignItems:   "center",
    gap:          "6px",
    padding:      "14px 14px 12px",
    borderBottom: `1px solid ${C.surface0}`,
    flexShrink:   0,
  } satisfies React.CSSProperties,

  wordmarkText: {
    fontFamily:    Font.ui,
    fontSize:      "0.875rem",
    fontWeight:    700,
    letterSpacing: "0.04em",
    color:         C.text,
    userSelect:    "none" as const,
  } satisfies React.CSSProperties,

  wordmarkDot: {
    width:        "5px",
    height:       "5px",
    borderRadius: "50%",
    background:   C.accent,
    marginLeft:   "auto",
    flexShrink:   0,
  } satisfies React.CSSProperties,

  navScroll: {
    flex:      1,
    overflowY: "auto" as const,
    paddingBottom: "8px",
  } satisfies React.CSSProperties,

  navGroup: {
    marginTop: "16px",
  } satisfies React.CSSProperties,

  groupHeading: {
    display:       "block",
    fontFamily:    Font.ui,
    fontSize:      "0.6rem",
    fontWeight:    700,
    letterSpacing: "0.12em",
    textTransform: "uppercase" as const,
    color:         C.overlay,
    padding:       "0 14px 4px",
    userSelect:    "none" as const,
  } satisfies React.CSSProperties,

  navList: {
    listStyle: "none",
    margin:    0,
    padding:   0,
  } satisfies React.CSSProperties,

  navItem: {
    display:      "flex",
    alignItems:   "center",
    gap:          "8px",
    padding:      "6px 14px",
    cursor:       "pointer",
    fontSize:     "0.8125rem",
    color:        C.subtext,
    userSelect:   "none" as const,
    borderLeft:   "2px solid transparent",
    transition:   "background 0.1s, color 0.1s",
  } satisfies React.CSSProperties,

  navItemHover: {
    background: C.surface0,
    color:      C.text,
  } satisfies React.CSSProperties,

  navItemActive: {
    background:  `${C.accent}14`,
    borderLeft:  `2px solid ${C.accent}`,
    color:       C.accent,
  } satisfies React.CSSProperties,

  navGlyph: {
    fontFamily:  Font.mono,
    fontSize:    "0.8rem",
    width:       "14px",
    flexShrink:  0,
    lineHeight:  1,
  } satisfies React.CSSProperties,

  sidebarFooter: {
    flexShrink: 0,
  } satisfies React.CSSProperties,

  main: {
    gridArea:      "main",
    overflow:      "hidden",
    padding:       "20px 24px",
    display:       "flex",
    flexDirection: "column" as const,
  } satisfies React.CSSProperties,

  // flex:1 + min-height:0 ensures the panel's height:100% resolves against
  // the content area (main minus padding), not the full padded container.
  panelArea: {
    flex:      1,
    minHeight: 0,
    overflow:  "hidden",
  } satisfies React.CSSProperties,
} as const;
