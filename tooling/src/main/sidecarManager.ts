import { spawn, type ChildProcess } from "child_process";
import * as readline from "readline";
import path from "path";
import { app } from "electron";

// ---------------------------------------------------------------------------
// sidecarManager — sidecar process lifecycle and JSON-RPC correlation (D26)
//
// Responsibilities:
//   - Resolve the sidecar binary path (prod: resourcesPath, dev: build output)
//   - Spawn the sidecar as a child process
//   - Route newline-delimited JSON responses back to waiting callers via an id
//     correlation map
//   - On crash: reject all in-flight requests; attempt one restart; if the
//     restart also fails, emit an error notification to the renderer
//
// The sidecar is strictly request/response — it never sends unsolicited frames.
// ---------------------------------------------------------------------------

/** Notification emitted to the renderer when the sidecar enters an unrecoverable state. */
export type SidecarErrorCallback = (message: string) => void;

/** Pending request entry: resolve/reject for the awaiting caller. */
interface PendingEntry {
  resolve: (value: unknown) => void;
  reject: (reason: Error) => void;
}

/** Counter for generating monotonically increasing request ids. */
let _nextId = 1;

/** The live sidecar child process, or null when not running. */
let _proc: ChildProcess | null = null;

/** Correlates request id strings to their pending resolve/reject callbacks. */
const _pending = new Map<string, PendingEntry>();

/** Whether a restart attempt has already been made in this session. */
let _restartAttempted = false;

/** Callback to surface unrecoverable sidecar errors to the renderer. */
let _onError: SidecarErrorCallback = () => undefined;

// ---------------------------------------------------------------------------
// Binary path resolution
// ---------------------------------------------------------------------------

/**
 * Resolves the path to the sidecar executable.
 *
 * In production, Electron Builder places platform binaries under
 * `resources/<platform>/Archetype.Tooling.Server[.exe]` relative to
 * `process.resourcesPath` (D26).  In development, the sidecar is expected in
 * `../src/Archetype.Tooling.Server/bin/Debug/net10.0/`.
 */
function resolveSidecarPath(): string {
  const isDev =
    process.env["NODE_ENV"] === "development" ||
    !app.isPackaged;

  if (isDev) {
    // Development: use the debug build output next to the tooling directory.
    const devPath = path.join(
      __dirname,
      "..",
      "..",
      "..",
      "..",
      "src",
      "Archetype.Tooling.Server",
      "bin",
      "Debug",
      "net10.0",
      process.platform === "win32"
        ? "Archetype.Tooling.Server.exe"
        : "Archetype.Tooling.Server",
    );
    return devPath;
  }

  // Production: platform-specific binary under resources/.
  const platformDir =
    process.platform === "win32"
      ? "win"
      : process.platform === "darwin"
        ? "mac"
        : "linux";

  const binaryName =
    process.platform === "win32"
      ? "Archetype.Tooling.Server.exe"
      : "Archetype.Tooling.Server";

  return path.join(process.resourcesPath, platformDir, binaryName);
}

// ---------------------------------------------------------------------------
// Spawn and wire
// ---------------------------------------------------------------------------

/**
 * Spawns the sidecar process and wires up the readline/response loop.
 * Returns `true` on success, `false` if spawn fails immediately.
 */
function spawnSidecar(): boolean {
  const binaryPath = resolveSidecarPath();

  let proc: ChildProcess;
  try {
    proc = spawn(binaryPath, [], {
      stdio: ["pipe", "pipe", "pipe"],
      // Sidecar must not inherit the Electron process environment variables
      // that reference Chromium internals — but it does need a minimal env.
      env: { ...process.env },
    });
  } catch (err) {
    // Spawn itself threw synchronously (e.g. ENOENT).
    return false;
  }

  // `proc.stdout` is non-null because we requested "pipe" for all stdio.
  const rl = readline.createInterface({
    input: proc.stdout!,
    crlfDelay: Infinity,
  });

  rl.on("line", (line: string) => {
    const trimmed = line.trim();
    if (trimmed.length === 0) return;
    handleResponseLine(trimmed);
  });

  proc.stderr?.on("data", (chunk: Buffer) => {
    // Sidecar writes diagnostics/logs to stderr; ignore in production.
    if (process.env["NODE_ENV"] === "development") {
      // eslint-disable-next-line no-console
      console.error("[sidecar stderr]", chunk.toString());
    }
  });

  proc.on("error", (err: Error) => {
    // Process-level error (e.g. ENOENT after spawn appeared to succeed).
    handleCrash(`Sidecar process error: ${err.message}`);
  });

  proc.on("exit", (code: number | null, signal: string | null) => {
    if (code !== 0 || signal !== null) {
      handleCrash(
        `Sidecar exited unexpectedly (code=${String(code)}, signal=${String(signal)})`,
      );
    }
  });

  _proc = proc;
  return true;
}

// ---------------------------------------------------------------------------
// Response handling
// ---------------------------------------------------------------------------

/** Dispatches a single newline-delimited JSON response line to its caller. */
function handleResponseLine(line: string): void {
  let envelope: { id?: unknown; result?: unknown; error?: unknown };
  try {
    envelope = JSON.parse(line) as typeof envelope;
  } catch {
    // Malformed JSON from sidecar — drop; callers are not affected unless
    // their specific id never arrives (which will time out or be rejected on
    // crash).
    return;
  }

  const id = typeof envelope.id === "string" ? envelope.id : null;
  if (id === null) return;

  const entry = _pending.get(id);
  if (entry === undefined) return; // Unexpected or duplicate response.

  _pending.delete(id);

  if (envelope.error !== undefined) {
    entry.reject(
      new Error(
        typeof envelope.error === "string"
          ? envelope.error
          : JSON.stringify(envelope.error),
      ),
    );
  } else {
    entry.resolve(envelope.result);
  }
}

// ---------------------------------------------------------------------------
// Crash recovery
// ---------------------------------------------------------------------------

/** Rejects all in-flight requests and optionally attempts a restart. */
function handleCrash(reason: string): void {
  _proc = null;

  // Reject every pending request — their callers get an error immediately.
  for (const [, entry] of _pending) {
    entry.reject(new Error(`Sidecar crash: ${reason}`));
  }
  _pending.clear();

  if (_restartAttempted) {
    // Second failure — sidecar is unrecoverable; notify the renderer.
    _onError(
      `Sidecar failed to restart: ${reason}. Please restart the application.`,
    );
    return;
  }

  _restartAttempted = true;

  // Attempt one restart.
  const ok = spawnSidecar();
  if (!ok) {
    _onError(
      `Sidecar could not be restarted: ${reason}. Please restart the application.`,
    );
  }
}

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

/**
 * Starts the sidecar process.  Must be called once during app startup.
 *
 * @param onError  Called when the sidecar crashes and cannot be restarted.
 *                 The main process should forward this message to the renderer.
 */
export function startSidecar(onError: SidecarErrorCallback): void {
  _onError = onError;
  const ok = spawnSidecar();
  if (!ok) {
    // First spawn failed immediately — treat as a crash with no prior restart.
    _restartAttempted = true; // suppress the "one restart" attempt
    _onError(
      "Sidecar could not be started. Please check the installation.",
    );
  }
}

/**
 * Sends a JSON-RPC request to the sidecar and returns a promise that resolves
 * with the result or rejects with an error.
 *
 * @param method  The RPC method name (must match a registered sidecar handler).
 * @param params  Optional parameters object (serialised to JSON).
 */
export function invoke(method: string, params?: unknown): Promise<unknown> {
  return new Promise<unknown>((resolve, reject) => {
    if (_proc === null || _proc.stdin === null) {
      reject(new Error("Sidecar is not running."));
      return;
    }

    const id = String(_nextId++);
    const request = JSON.stringify({ id, method, params: params ?? null });

    _pending.set(id, { resolve, reject });

    // Write the newline-delimited JSON request to the sidecar's stdin.
    const ok = _proc.stdin.write(request + "\n");
    if (!ok) {
      // Back-pressure: drain before writing more.  In practice this is rare
      // given the low request rate of the authoring tool.
      _proc.stdin.once("drain", () => {
        // Request was already written — nothing more to do.
      });
    }
  });
}

/**
 * Terminates the sidecar gracefully.  Called when the Electron app is
 * about to quit so the child process does not become an orphan.
 */
export function stopSidecar(): void {
  if (_proc !== null) {
    // Close stdin — the sidecar's read loop exits on EOF.
    _proc.stdin?.end();
    _proc = null;
  }
}
