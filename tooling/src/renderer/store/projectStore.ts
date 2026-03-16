import { create } from "zustand";
import { invalidateSnapshotCache } from "../snapshot";

// ---------------------------------------------------------------------------
// projectStore — sidecar connection status and current project state (D26)
//
// The renderer never holds a canonical copy of the game definition; it only
// tracks metadata about the current project (name, path, dirty state) and the
// connection status of the sidecar.  Full definition data lives in sidecar
// memory and is fetched on demand via IPC.
// ---------------------------------------------------------------------------

/** Possible states of the sidecar child process. */
export type SidecarStatus =
  | "disconnected"   // not yet started or terminated
  | "connecting"     // spawn in progress
  | "ready"          // accepting requests
  | "error";         // crashed / unrecoverable

export interface ProjectState {
  // -- Sidecar connection --
  /** Current state of the sidecar child process. */
  sidecarStatus: SidecarStatus;

  // -- Project metadata --
  /**
   * Absolute path to the currently open .archetype file.
   * Null when no project is open (new project not yet saved).
   */
  projectPath: string | null;

  /** Human-readable project name derived from the loaded project ID. */
  projectName: string | null;

  /**
   * True when the renderer has sent at least one mutation since the last save.
   * Used to show a "modified" indicator in the title bar.
   */
  isDirty: boolean;

  /**
   * True while a mutation request is in-flight to the sidecar.
   * The UI disables further edits during this window (debounce timer handles
   * the common case; this flag is a safety net for sequential requests).
   */
  isMutating: boolean;
}

interface ProjectActions {
  /** Called by the sidecar manager when the sidecar process is ready. */
  setSidecarStatus: (status: SidecarStatus) => void;

  /** Called after a successful LoadProject response. Also invalidates the
   *  snapshot cache so panels pick up the new project's data. */
  setProject: (path: string | null, name: string | null) => void;

  /** Called when the user closes the project (not yet implemented in Group 2). */
  clearProject: () => void;

  /** Mark the project as having unsaved mutations. */
  markDirty: () => void;

  /** Mark the project as clean (called after a successful save). */
  markClean: () => void;

  /** Set the in-flight mutation flag. */
  setMutating: (mutating: boolean) => void;
}

const initialState: ProjectState = {
  sidecarStatus: "disconnected",
  projectPath: null,
  projectName: null,
  isDirty: false,
  isMutating: false,
};

export const useProjectStore = create<ProjectState & ProjectActions>((set) => ({
  ...initialState,

  setSidecarStatus(status) {
    set({ sidecarStatus: status });
  },

  setProject(path, name) {
    // A new project was loaded — discard any cached snapshot from the
    // previous project so panels re-fetch on next mount.
    invalidateSnapshotCache();
    set({ projectPath: path, projectName: name, isDirty: false });
  },

  clearProject() {
    invalidateSnapshotCache();
    set({ projectPath: null, projectName: null, isDirty: false, isMutating: false });
  },

  markDirty() {
    // Invalidate the snapshot cache so the next fetchSnapshot() call
    // re-fetches from the sidecar rather than returning stale data.
    invalidateSnapshotCache();
    set({ isDirty: true });
  },

  markClean() {
    set({ isDirty: false });
  },

  setMutating(mutating) {
    set({ isMutating: mutating });
  },
}));
