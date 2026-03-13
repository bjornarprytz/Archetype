import { app, BrowserWindow } from "electron";
import { createWindow } from "./windowManager";
import { startSidecar, stopSidecar } from "./sidecarManager";
import { registerIpcHandlers } from "./ipcHandlers";
import { shutdownAutosave } from "./autosave";

// ---------------------------------------------------------------------------
// main.ts — Electron main process entry point (D26)
//
// Startup sequence:
//   1. Create the BrowserWindow.
//   2. Register all IPC handlers (sidecar-forwarded + file I/O).
//   3. Spawn the sidecar; on unrecoverable error forward to the renderer.
//
// Shutdown sequence:
//   1. Cancel any pending autosave.
//   2. Terminate the sidecar (closes stdin so the sidecar exits cleanly).
// ---------------------------------------------------------------------------

let mainWindow: BrowserWindow | null = null;

app.whenReady().then(() => {
  mainWindow = createWindow();

  // Register all ipcMain.handle channels before the renderer loads so no
  // request arrives before the handlers are wired.
  registerIpcHandlers(mainWindow);

  // Start the sidecar.  On unrecoverable crash, send a push notification to
  // the renderer so it can show an error dialog.
  startSidecar((message: string) => {
    mainWindow?.webContents.send("SidecarError", message);
  });

  // macOS: re-create the window when the dock icon is clicked and no windows
  // are open (standard macOS behaviour).
  app.on("activate", () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      mainWindow = createWindow();
      registerIpcHandlers(mainWindow);
    }
  });
});

// Quit on all windows closed, except on macOS where the app stays active in
// the dock until the user explicitly quits with Cmd+Q.
app.on("window-all-closed", () => {
  if (process.platform !== "darwin") {
    app.quit();
  }
});

// Graceful shutdown: cancel autosave and stop the sidecar before the process
// exits so no orphan child processes are left behind.
app.on("before-quit", () => {
  shutdownAutosave();
  stopSidecar();
});
