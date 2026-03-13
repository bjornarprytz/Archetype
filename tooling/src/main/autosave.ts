import { invoke as sidecarInvoke } from "./sidecarManager";
import { writeFile } from "./fileHandlers";

// ---------------------------------------------------------------------------
// autosave — 60-second inactivity autosave timer (D27, task 5.4)
//
// The autosave timer fires 60 seconds after the last mutation IPC call.
// When it fires it:
//   1. Calls SaveProject on the sidecar to get the serialised JSON.
//   2. Writes the JSON to the known project file path via writeFile.
//
// Errors are swallowed silently — autosave is best-effort and must never
// interrupt the user's workflow.  The user is responsible for explicit saves.
//
// The timer is only active when a project is open (projectPath is non-null).
// ipcHandlers calls notifyMutation() on every mutation channel; main.ts calls
// setProjectPath(path) when a project is loaded and clearProjectPath() when
// it is closed.
// ---------------------------------------------------------------------------

/** Interval between autosave firing after last mutation, in milliseconds. */
const AUTOSAVE_DELAY_MS = 60_000;

/** The current pending autosave timer handle, or null when inactive. */
let _timer: ReturnType<typeof setTimeout> | null = null;

/** The path of the currently open project file, or null when none is open. */
let _projectPath: string | null = null;

// ---------------------------------------------------------------------------
// Internal
// ---------------------------------------------------------------------------

/** Clears the pending timer if one is set. */
function clearTimer(): void {
  if (_timer !== null) {
    clearTimeout(_timer);
    _timer = null;
  }
}

/**
 * Performs the actual autosave: calls the sidecar's SaveProject method and
 * writes the returned JSON to disk.  All errors are caught and discarded.
 */
async function performAutosave(): Promise<void> {
  _timer = null;

  if (_projectPath === null) {
    // No project open — nothing to save.
    return;
  }

  try {
    // Ask the sidecar to serialise the current ProjectState.
    const result = await sidecarInvoke("SaveProject", null);
    const { json } = result as { json: string };

    // Write to disk via the validated fileHandlers path.
    writeFile(_projectPath, json);
  } catch {
    // Autosave is best-effort: swallow all errors silently.
  }
}

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

/**
 * Sets the path of the currently open project file.
 * Starts the autosave timer if it was previously inactive.
 *
 * Call this after a successful LoadProject or SaveAs operation.
 */
export function setProjectPath(filePath: string): void {
  _projectPath = filePath;
}

/**
 * Clears the project path, disabling the autosave timer.
 * Call this when the project is closed.
 */
export function clearProjectPath(): void {
  _projectPath = null;
  clearTimer();
}

/**
 * Resets the inactivity timer.  Must be called from ipcHandlers on every
 * mutation channel invocation (UpdateKeywordBody, AddEntry, etc.).
 *
 * If no project is open the call is a no-op.
 */
export function notifyMutation(): void {
  if (_projectPath === null) {
    // No open project — autosave is not applicable.
    return;
  }

  // Reset the countdown.
  clearTimer();
  _timer = setTimeout(() => {
    void performAutosave();
  }, AUTOSAVE_DELAY_MS);
}

/**
 * Immediately cancels any pending autosave and disables future autosaves.
 * Call this when the app is about to quit.
 */
export function shutdownAutosave(): void {
  clearTimer();
  _projectPath = null;
}
