import { app, dialog } from "electron";
import fs from "fs";
import path from "path";
import type {
  ShowOpenDialogOptions,
  ShowSaveDialogOptions,
} from "../shared/ipc";

// ---------------------------------------------------------------------------
// fileHandlers — file I/O channel implementations (D26, task 5.3)
//
// All file I/O is owned by the main process (D26).  The renderer may never
// touch the filesystem directly.  These functions are called from ipcHandlers
// after request-shape validation.
//
// Security: every path accepted from the renderer is normalised and validated
// against the allowed base directories before any I/O is performed.  This
// prevents directory traversal attacks in the event the renderer is
// compromised.
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// Path validation
// ---------------------------------------------------------------------------

/**
 * Allowed base directories for file I/O.
 *
 * The renderer may only read/write files under:
 *   1. The user's home directory (project files, exports)
 *   2. The user data directory (app state)
 *   3. The temp directory (temporary exports)
 *
 * Paths outside these directories are rejected.
 */
function getAllowedRoots(): string[] {
  return [
    app.getPath("home"),
    app.getPath("userData"),
    app.getPath("temp"),
    app.getPath("documents"),
    app.getPath("desktop"),
  ];
}

/**
 * Validates that `filePath` is an absolute path that resolves within one of
 * the allowed base directories.  Throws if validation fails.
 */
function validatePath(filePath: string): void {
  if (!path.isAbsolute(filePath)) {
    throw new Error(`Relative paths are not permitted: ${filePath}`);
  }

  const normalised = path.normalize(filePath);
  const allowed = getAllowedRoots();

  const isAllowed = allowed.some((root) => {
    const normRoot = path.normalize(root);
    // Ensure the path is inside the root (with a trailing separator to prevent
    // prefix collision e.g. /home/user2 starting with /home/user).
    return (
      normalised === normRoot ||
      normalised.startsWith(normRoot + path.sep)
    );
  });

  if (!isAllowed) {
    throw new Error(
      `File path is outside allowed directories: ${normalised}`,
    );
  }
}

// ---------------------------------------------------------------------------
// Channel implementations
// ---------------------------------------------------------------------------

/**
 * Reads a UTF-8 text file and returns its contents as a string.
 * Rejects if the path is invalid or the file cannot be read.
 */
export function readFile(filePath: string): string {
  validatePath(filePath);
  return fs.readFileSync(filePath, "utf-8");
}

/**
 * Writes `content` (a UTF-8 string) to `filePath`, creating intermediate
 * directories as needed.  Rejects if the path is invalid.
 */
export function writeFile(filePath: string, content: string): void {
  validatePath(filePath);
  const dir = path.dirname(filePath);
  fs.mkdirSync(dir, { recursive: true });
  fs.writeFileSync(filePath, content, "utf-8");
}

/**
 * Shows the native open-file dialog and returns the selected path,
 * or `null` if the user cancelled.
 */
export async function showOpenDialog(
  options: ShowOpenDialogOptions,
): Promise<string | null> {
  // Build the Electron dialog options object explicitly, omitting keys whose
  // values are undefined (required by exactOptionalPropertyTypes).
  const dialogOpts: Electron.OpenDialogOptions = {
    properties: options.properties ? [...options.properties] : ["openFile"],
  };
  if (options.title !== undefined) dialogOpts.title = options.title;
  if (options.defaultPath !== undefined) dialogOpts.defaultPath = options.defaultPath;
  if (options.filters !== undefined) {
    dialogOpts.filters = [...options.filters].map((f) => ({
      name: f.name,
      extensions: [...f.extensions],
    }));
  }

  const result = await dialog.showOpenDialog(dialogOpts);

  if (result.canceled || result.filePaths.length === 0) return null;

  // Return the first selected path; multiSelections are not surfaced through
  // this single-return API — callers that need multiple paths open their own
  // dialog via Electron directly.
  return result.filePaths[0] ?? null;
}

/**
 * Shows the native save-file dialog and returns the chosen path,
 * or `null` if the user cancelled.
 */
export async function showSaveDialog(
  options: ShowSaveDialogOptions,
): Promise<string | null> {
  // Build options explicitly to satisfy exactOptionalPropertyTypes.
  const dialogOpts: Electron.SaveDialogOptions = {};
  if (options.title !== undefined) dialogOpts.title = options.title;
  if (options.defaultPath !== undefined) dialogOpts.defaultPath = options.defaultPath;
  if (options.filters !== undefined) {
    dialogOpts.filters = [...options.filters].map((f) => ({
      name: f.name,
      extensions: [...f.extensions],
    }));
  }

  const result = await dialog.showSaveDialog(dialogOpts);

  if (result.canceled || result.filePath === undefined) return null;
  return result.filePath;
}

/**
 * Returns the Electron user-data directory for this application.
 * The renderer uses this to persist editor state between sessions.
 */
export function getUserDataPath(): string {
  return app.getPath("userData");
}
