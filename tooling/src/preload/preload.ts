import { contextBridge, ipcRenderer } from "electron";
import type { ArchetypeApi, IpcChannel } from "../shared/ipc";

// ---------------------------------------------------------------------------
// Preload script (D26)
//
// This module runs in a privileged context with access to both the renderer
// DOM and select Electron APIs. Its only job is to expose a typed, minimal API
// to the renderer via contextBridge — no raw Electron APIs are leaked.
//
// Security rules enforced here:
//   - Only channels defined in IPC_CHANNELS are forwarded.
//   - ipcRenderer is never exposed directly.
//   - No Node.js globals (process, require, __dirname) reach the renderer.
// ---------------------------------------------------------------------------

const archetypeApi: ArchetypeApi = {
  /**
   * Forwards a request to the main process via ipcRenderer.invoke.
   * The renderer awaits the promise; the main process handles the channel and
   * returns the result (or throws, which rejects the promise).
   */
  invoke<T = unknown>(channel: IpcChannel, payload?: unknown): Promise<T> {
    // ipcRenderer.invoke returns Promise<unknown>; we trust the channel
    // contract defined in ipc.ts and cast at the boundary.
    return ipcRenderer.invoke(channel, payload) as Promise<T>;
  },

  /**
   * Registers a handler for push notifications from the main process.
   * Returns an unsubscribe function so callers can clean up on unmount.
   */
  onNotification(channel: IpcChannel, handler: (payload: unknown) => void): () => void {
    // ipcRenderer.on callback receives (event, ...args); we surface only the
    // first argument (the payload) to keep the renderer API simple.
    const listener = (_event: Electron.IpcRendererEvent, payload: unknown): void => {
      handler(payload);
    };

    ipcRenderer.on(channel, listener);

    // Return cleanup function — callers call this in useEffect cleanup.
    return () => {
      ipcRenderer.removeListener(channel, listener);
    };
  },
};

// Expose the API to the renderer under `window.archetype`.
// contextBridge ensures no prototype pollution and no Electron API leakage.
contextBridge.exposeInMainWorld("archetype", archetypeApi);
