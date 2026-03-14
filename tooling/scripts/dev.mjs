/**
 * Dev startup script — coordinates Vite + Electron for development.
 *
 * Sequence:
 *   1. Build .NET sidecar (Archetype.Tooling.Server, Debug config).
 *   2. Build main process and preload (TypeScript → JS in dist/).
 *   3. Spawn the Vite dev server in the background.
 *   4. Wait for Vite to accept connections on port 5173.
 *   5. Spawn Electron with VITE_DEV_SERVER_URL set.
 *   6. When Electron exits, kill Vite and exit this process.
 */

import { spawn, execSync } from "child_process";
import { createConnection } from "net";

const VITE_PORT = 5173;
const VITE_URL  = `http://localhost:${VITE_PORT}`;

const ROOT = new URL("..", import.meta.url).pathname;

// ---------------------------------------------------------------------------
// Step 1: Build sidecar (.NET), main process, and preload
// ---------------------------------------------------------------------------

console.log("→ Building .NET sidecar…");
try {
  execSync("dotnet build ../src/Archetype.Tooling.Server/Archetype.Tooling.Server.csproj -c Debug", {
    stdio: "inherit",
    cwd: ROOT,
  });
} catch {
  console.error("Sidecar build failed — run `dotnet build` manually for details.");
  process.exit(1);
}

console.log("→ Building main process and preload…");
try {
  execSync("npm run build:main && npm run build:preload", {
    stdio: "inherit",
    cwd: ROOT,
  });
} catch {
  console.error("Build failed — fix TypeScript errors and retry.");
  process.exit(1);
}

// ---------------------------------------------------------------------------
// Step 2: Spawn Vite dev server
// ---------------------------------------------------------------------------

console.log("→ Starting Vite dev server…");
const vite = spawn("npx", ["vite"], {
  stdio: "inherit",
  shell: true,
  cwd: ROOT,
});

// ---------------------------------------------------------------------------
// Step 3: Wait for Vite to accept TCP connections on port 5173
// ---------------------------------------------------------------------------

async function waitForPort(port, timeoutMs = 30_000) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    const ready = await new Promise((resolve) => {
      const s = createConnection(port, "localhost");
      s.once("connect", () => { s.destroy(); resolve(true);  });
      s.once("error",   () => { s.destroy(); resolve(false); });
    });
    if (ready) return;
    await new Promise((r) => setTimeout(r, 300));
  }
  throw new Error(`Vite did not start within ${timeoutMs / 1000}s.`);
}

try {
  await waitForPort(VITE_PORT);
  console.log(`→ Vite ready at ${VITE_URL}`);
} catch (err) {
  console.error(err.message);
  vite.kill();
  process.exit(1);
}

// ---------------------------------------------------------------------------
// Step 4: Spawn Electron
// ---------------------------------------------------------------------------

console.log("→ Starting Electron…");
const electron = spawn("npx", ["electron", "."], {
  stdio: "inherit",
  cwd: ROOT,
  env: { ...process.env, VITE_DEV_SERVER_URL: VITE_URL },
});

// ---------------------------------------------------------------------------
// Step 5: Clean up when Electron exits
// ---------------------------------------------------------------------------

electron.on("exit", (code) => {
  vite.kill();
  process.exit(code ?? 0);
});

// Also clean up if this script is interrupted (Ctrl+C).
process.on("SIGINT",  () => { electron.kill(); vite.kill(); process.exit(0); });
process.on("SIGTERM", () => { electron.kill(); vite.kill(); process.exit(0); });
