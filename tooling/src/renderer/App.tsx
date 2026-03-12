import React from "react";

// ---------------------------------------------------------------------------
// Navigation panel identifiers.
// Each entry corresponds to a top-level panel that will be implemented in
// Group 6.
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
  readonly id: PanelId;
  readonly label: string;
}

const NAV_ITEMS: readonly NavItem[] = [
  { id: "keywords",     label: "Keywords" },
  { id: "cards",        label: "Cards" },
  { id: "gameRules",    label: "Game Rules" },
  { id: "initManifest", label: "Init Manifest" },
  { id: "localization", label: "Localization" },
  { id: "graph",        label: "Composition Graph" },
  { id: "setOverview",  label: "Set Overview" },
  { id: "problems",     label: "Problems" },
];

/**
 * Root application shell.
 *
 * Layout: fixed left sidebar for navigation + flex-grow main content area.
 * The sidebar drives `activePanel` state; the main area renders a placeholder
 * for the selected panel (real panels added in Group 6).
 *
 * The status bar at the bottom shows global error/warning counts (populated
 * by diagnosticsStore in Group 6).
 */
export function App(): React.ReactElement {
  const [activePanel, setActivePanel] = React.useState<PanelId>("keywords");

  return (
    <div style={styles.shell}>
      {/* Sidebar navigation */}
      <nav style={styles.sidebar} aria-label="Main navigation">
        <div style={styles.sidebarTitle}>Archetype</div>
        <ul style={styles.navList} role="listbox">
          {NAV_ITEMS.map((item) => (
            <li
              key={item.id}
              role="option"
              aria-selected={activePanel === item.id}
              style={{
                ...styles.navItem,
                ...(activePanel === item.id ? styles.navItemActive : {}),
              }}
              onClick={() => setActivePanel(item.id)}
            >
              {item.label}
            </li>
          ))}
        </ul>
      </nav>

      {/* Main content area */}
      <main style={styles.main}>
        <PanelPlaceholder panelId={activePanel} />
      </main>

      {/* Status bar — error/warning counts (wired to diagnosticsStore in Group 6) */}
      <footer style={styles.statusBar} role="status">
        <StatusBarPlaceholder />
      </footer>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Placeholder components — replaced by real panels in Group 6
// ---------------------------------------------------------------------------

function PanelPlaceholder({ panelId }: { panelId: PanelId }): React.ReactElement {
  const item = NAV_ITEMS.find((n) => n.id === panelId);
  return (
    <div style={styles.placeholder}>
      <h2 style={styles.placeholderTitle}>{item?.label ?? panelId}</h2>
      <p style={styles.placeholderBody}>Panel not yet implemented — Group 6.</p>
    </div>
  );
}

function StatusBarPlaceholder(): React.ReactElement {
  return (
    <span style={styles.statusBarText}>
      0 errors &nbsp; 0 warnings &nbsp;—&nbsp; no project open
    </span>
  );
}

// ---------------------------------------------------------------------------
// Inline styles (avoids a CSS module dependency in this skeleton)
// ---------------------------------------------------------------------------

const styles = {
  shell: {
    display: "grid",
    gridTemplateColumns: "200px 1fr",
    gridTemplateRows: "1fr 28px",
    gridTemplateAreas: '"sidebar main" "sidebar statusbar"',
    height: "100vh",
    overflow: "hidden",
  } satisfies React.CSSProperties,

  sidebar: {
    gridArea: "sidebar",
    background: "#181825",
    borderRight: "1px solid #313244",
    display: "flex",
    flexDirection: "column" as const,
    overflow: "hidden",
  } satisfies React.CSSProperties,

  sidebarTitle: {
    padding: "16px 12px 8px",
    fontWeight: 700,
    fontSize: "0.85rem",
    letterSpacing: "0.08em",
    textTransform: "uppercase" as const,
    color: "#89b4fa",
  } satisfies React.CSSProperties,

  navList: {
    listStyle: "none",
    flex: 1,
    overflowY: "auto" as const,
  } satisfies React.CSSProperties,

  navItem: {
    padding: "8px 16px",
    cursor: "pointer",
    fontSize: "0.875rem",
    color: "#cdd6f4",
    borderLeftWidth: "3px",
    borderLeftStyle: "solid",
    borderLeftColor: "transparent",
    userSelect: "none" as const,
  } satisfies React.CSSProperties,

  navItemActive: {
    background: "#1e1e2e",
    borderLeftColor: "#89b4fa",
    color: "#89b4fa",
  } satisfies React.CSSProperties,

  main: {
    gridArea: "main",
    overflow: "auto",
    padding: "24px",
  } satisfies React.CSSProperties,

  statusBar: {
    gridArea: "statusbar",
    background: "#181825",
    borderTop: "1px solid #313244",
    display: "flex",
    alignItems: "center",
    padding: "0 12px",
    fontSize: "0.75rem",
    color: "#6c7086",
  } satisfies React.CSSProperties,

  statusBarText: {} satisfies React.CSSProperties,

  placeholder: {
    maxWidth: 480,
  } satisfies React.CSSProperties,

  placeholderTitle: {
    fontSize: "1.25rem",
    fontWeight: 600,
    marginBottom: "8px",
    color: "#cdd6f4",
  } satisfies React.CSSProperties,

  placeholderBody: {
    fontSize: "0.875rem",
    color: "#6c7086",
  } satisfies React.CSSProperties,
} as const;
