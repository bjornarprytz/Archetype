import { ipcMain } from "electron";
import type { BrowserWindow } from "electron";
import { IPC_CHANNELS } from "../shared/ipc";
import type {
  ReadFileParams,
  WriteFileParams,
  ShowOpenDialogOptions,
  ShowSaveDialogOptions,
} from "../shared/ipc";
import { invoke as sidecarInvoke } from "./sidecarManager";
import {
  readFile,
  writeFile,
  showOpenDialog,
  showSaveDialog,
  getUserDataPath,
} from "./fileHandlers";
import { notifyMutation } from "./autosave";

// ---------------------------------------------------------------------------
// ipcHandlers — main-process IPC handler registration (D26, task 5.2)
//
// This module registers ipcMain.handle entries for every channel defined in
// src/shared/ipc.ts.  The channels fall into two groups:
//
//   1. Sidecar-forwarded channels — the request is forwarded verbatim to the
//      sidecar via sidecarManager.invoke and the result is returned.
//
//   2. File I/O channels ("File:*") — handled entirely in the main process
//      using Node's `fs` module and Electron's `dialog` API (D26).
//
// The autosave timer is notified on every mutation channel call so the
// inactivity countdown resets.
//
// Call registerIpcHandlers(win) once, after the BrowserWindow is created.
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// Helper: sidecar forwarding
// ---------------------------------------------------------------------------

/**
 * Registers a sidecar-forwarded handler.
 *
 * The payload from the renderer is forwarded to the sidecar as `params`.
 * The sidecar's `result` is returned to the renderer.  Any error from the
 * sidecar is re-thrown so the renderer's `invoke` promise rejects.
 */
function registerSidecarChannel(channel: string): void {
  ipcMain.handle(channel, async (_event, payload: unknown) => {
    return sidecarInvoke(channel, payload);
  });
}

/**
 * Registers a sidecar-forwarded handler that also resets the autosave timer
 * (used for all mutation channels).
 */
function registerMutationChannel(channel: string): void {
  ipcMain.handle(channel, async (_event, payload: unknown) => {
    notifyMutation(); // reset the inactivity timer
    return sidecarInvoke(channel, payload);
  });
}

// ---------------------------------------------------------------------------
// Public registration entry point
// ---------------------------------------------------------------------------

/**
 * Registers all IPC handlers for the given window.
 *
 * The `win` parameter is retained so that error notifications from the sidecar
 * manager can be forwarded to the renderer via `win.webContents.send`.
 *
 * Must be called once after app is ready and the BrowserWindow exists.
 */
export function registerIpcHandlers(win: BrowserWindow): void {
  // -------------------------------------------------------------------------
  // Sidecar-forwarded channels — query / non-mutating
  // -------------------------------------------------------------------------

  // LoadProject is a mutation (replaces in-memory state) so it resets autosave.
  registerMutationChannel(IPC_CHANNELS.LoadProject);

  // SaveProject is the save operation itself — does not reset autosave timer
  // (it IS the autosave write path) but also doesn't count as a user mutation.
  registerSidecarChannel(IPC_CHANNELS.SaveProject);

  // -------------------------------------------------------------------------
  // Mutation channels — reset the autosave inactivity timer on each call
  // -------------------------------------------------------------------------

  registerMutationChannel(IPC_CHANNELS.UpdateKeywordBody);
  registerMutationChannel(IPC_CHANNELS.UpdateCardEffect);
  registerMutationChannel(IPC_CHANNELS.UpdateLifetimeSpec);
  registerMutationChannel(IPC_CHANNELS.UpdateActivationCondition);
  registerMutationChannel(IPC_CHANNELS.UpdateCostBody);
  registerMutationChannel(IPC_CHANNELS.UpdateField);
  registerMutationChannel(IPC_CHANNELS.AddEntry);
  registerMutationChannel(IPC_CHANNELS.RemoveEntry);
  registerMutationChannel(IPC_CHANNELS.RenameEntry);

  // -------------------------------------------------------------------------
  // Query channels — forwarded to sidecar, do not reset autosave
  // -------------------------------------------------------------------------

  registerSidecarChannel(IPC_CHANNELS.GetAllDiagnostics);
  registerSidecarChannel(IPC_CHANNELS.GetSymbolInfo);
  registerSidecarChannel(IPC_CHANNELS.GetReferenceGraph);
  registerSidecarChannel(IPC_CHANNELS.GetCompletions);
  registerSidecarChannel(IPC_CHANNELS.RenderCardText);

  // -------------------------------------------------------------------------
  // Export channels — forwarded to sidecar, do not reset autosave
  // -------------------------------------------------------------------------

  registerSidecarChannel(IPC_CHANNELS.ExportGameDefinition);
  registerSidecarChannel(IPC_CHANNELS.ExportGodotClasses);

  // -------------------------------------------------------------------------
  // File I/O channels — handled entirely in the main process (D26)
  // -------------------------------------------------------------------------

  ipcMain.handle(
    IPC_CHANNELS.ReadFile,
    (_event, payload: unknown): string => {
      const { path: filePath } = payload as ReadFileParams;
      return readFile(filePath);
    },
  );

  ipcMain.handle(
    IPC_CHANNELS.WriteFile,
    (_event, payload: unknown): void => {
      const { path: filePath, content } = payload as WriteFileParams;
      writeFile(filePath, content);
    },
  );

  ipcMain.handle(
    IPC_CHANNELS.ShowOpenDialog,
    async (_event, payload: unknown): Promise<string | null> => {
      const options = payload as ShowOpenDialogOptions;
      return showOpenDialog(options);
    },
  );

  ipcMain.handle(
    IPC_CHANNELS.ShowSaveDialog,
    async (_event, payload: unknown): Promise<string | null> => {
      const options = payload as ShowSaveDialogOptions;
      return showSaveDialog(options);
    },
  );

  ipcMain.handle(
    IPC_CHANNELS.GetUserDataPath,
    (_event): string => getUserDataPath(),
  );

  // -------------------------------------------------------------------------
  // Sidecar error notification forwarding
  // -------------------------------------------------------------------------

  // The sidecar manager calls back with an error message when the sidecar
  // crashes and cannot be restarted.  main.ts wires this by calling:
  //
  //   startSidecar((msg) => win.webContents.send("SidecarError", msg))
  //
  // That push direction (main → renderer) does not need an ipcMain.handle.
  // The renderer subscribes via window.archetype.onNotification("SidecarError", ...).
  //
  // `win` is received here so this function remains the single registration
  // point; Group 6 panels may need to send other notifications via `win`.
  // Reference it to satisfy the strict-unused-parameter lint rule.
  void win.id; // win.id is a harmless read; ensures the parameter is "used"
}
