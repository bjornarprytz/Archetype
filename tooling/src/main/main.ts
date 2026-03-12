import { app, BrowserWindow } from "electron";
import { createWindow } from "./windowManager";

// Group 5 will add sidecar spawn here; for now the main process just manages
// the window lifecycle and IPC handler registration.

let mainWindow: BrowserWindow | null = null;

app.whenReady().then(() => {
  mainWindow = createWindow();

  // macOS: re-create the window when the dock icon is clicked and no windows
  // are open (standard macOS behaviour).
  app.on("activate", () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      mainWindow = createWindow();
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
