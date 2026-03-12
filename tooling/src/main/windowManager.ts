import { BrowserWindow, app } from "electron";
import path from "path";

// Vite injects VITE_DEV_SERVER_URL when running in dev mode.
// The process.env lookup is guarded so it compiles in both dev and prod.
const DEV_SERVER_URL = process.env["VITE_DEV_SERVER_URL"] as string | undefined;

/**
 * Creates and returns the single application BrowserWindow.
 *
 * Security posture (D26):
 * - contextIsolation: true  — renderer cannot access Node.js directly.
 * - nodeIntegration: false  — renderer has no require() access.
 * - sandbox: true           — additional OS-level process isolation.
 *
 * The preload script bridges the two worlds via contextBridge.
 */
export function createWindow(): BrowserWindow {
  const preloadPath = path.join(__dirname, "..", "preload", "preload.js");

  const win = new BrowserWindow({
    width: 1400,
    height: 900,
    minWidth: 900,
    minHeight: 600,
    title: "Archetype",
    webPreferences: {
      preload: preloadPath,
      contextIsolation: true,   // D26: renderer cannot access Node APIs
      nodeIntegration: false,   // D26: no direct Node require in renderer
      sandbox: true,            // additional OS-level isolation
    },
  });

  if (DEV_SERVER_URL !== undefined && DEV_SERVER_URL !== "") {
    // Dev mode: load from Vite dev server for hot module replacement.
    void win.loadURL(DEV_SERVER_URL);
    win.webContents.openDevTools();
  } else {
    // Production: load the built index.html from the renderer dist directory.
    const indexPath = path.join(__dirname, "..", "renderer", "index.html");
    void win.loadFile(indexPath);
  }

  return win;
}
