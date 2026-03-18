// @vitest-environment node
//
// Tests for sidecarManager.ts (task 5.1).
//
// All tests use Vitest's module mocking to avoid spawning a real .NET process.
// The mock replaces child_process.spawn with a controllable EventEmitter-based
// stub so we can simulate sidecar responses, crashes, and restarts.

import {
  describe,
  it,
  expect,
  vi,
  beforeEach,
  afterEach,
} from "vitest";
import { EventEmitter } from "events";
import { Readable, PassThrough } from "stream";

// ---------------------------------------------------------------------------
// Mock child_process — hoisted before any imports by Vitest.
// We use a module-level factory function that creates a new MockChildProcess
// each time spawn is called, and updates the exported `mockProc` reference.
// ---------------------------------------------------------------------------

/** The most recently created MockChildProcess (updated by the spawn mock). */
class MockChildProcess extends EventEmitter {
  stdin: PassThrough;
  stdout: Readable;
  stderr: Readable;

  /** Lines written by sidecarManager to the sidecar's stdin. */
  writtenLines: string[] = [];

  /** Whether stdin.end() has been called. */
  stdinEnded = false;

  constructor() {
    super();

    // Use a PassThrough so we can capture writes without real piping.
    this.stdin = new PassThrough();
    this.stdin.on("data", (chunk: Buffer) => {
      // Each chunk may contain one or more newline-terminated lines.
      const lines = chunk.toString().split("\n").filter((l) => l.trim());
      this.writtenLines.push(...lines);
    });

    const origEnd = this.stdin.end.bind(this.stdin);
    this.stdin.end = (...args: Parameters<typeof origEnd>) => {
      this.stdinEnded = true;
      return origEnd(...args);
    };

    this.stdout = new Readable({ read() { /* noop */ } });
    this.stderr = new Readable({ read() { /* noop */ } });
  }

  /** Push a response JSON line as if the sidecar wrote it to stdout. */
  pushResponse(obj: unknown): void {
    this.stdout.push(JSON.stringify(obj) + "\n");
  }

  /** Simulate an abnormal exit (e.g. crash). */
  crash(code = 1): void {
    this.emit("exit", code, null);
  }
}

// Active mock process, updated on every spawn call.
let mockProc: MockChildProcess;
const spawnMockFn = vi.fn(() => {
  mockProc = new MockChildProcess();
  return mockProc;
});

vi.mock("child_process", () => ({
  spawn: spawnMockFn,
}));

vi.mock("electron", () => ({
  app: {
    isPackaged: false,
  },
}));

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/** Waits for the event loop to flush pending microtasks / IO callbacks. */
async function flushAsync(): Promise<void> {
  await new Promise<void>((r) => setImmediate(r));
  await new Promise<void>((r) => setImmediate(r));
}

/** Waits until at least `count` lines have been written to stdin. */
async function waitForWrittenLines(
  count: number,
  maxMs = 200,
): Promise<void> {
  const deadline = Date.now() + maxMs;
  while (mockProc.writtenLines.length < count && Date.now() < deadline) {
    await flushAsync();
  }
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe("sidecarManager", () => {
  beforeEach(() => {
    vi.resetModules();
    spawnMockFn.mockClear();
    spawnMockFn.mockImplementation(() => {
      mockProc = new MockChildProcess();
      return mockProc;
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  // -------------------------------------------------------------------------
  // invoke / request-response correlation
  // -------------------------------------------------------------------------

  it("sends a newline-delimited JSON request with an id", async () => {
    const { startSidecar, invoke } = await import("../sidecarManager");
    startSidecar(() => undefined);

    const resultPromise = invoke("TestMethod", { key: "value" });

    await waitForWrittenLines(1);

    expect(mockProc.writtenLines.length).toBeGreaterThan(0);
    const written = JSON.parse(mockProc.writtenLines[0]!) as {
      id: string;
      method: string;
      params: { key: string };
    };
    expect(written.method).toBe("TestMethod");
    expect(written.params.key).toBe("value");
    expect(typeof written.id).toBe("string");

    // Push the matching response.
    mockProc.pushResponse({ id: written.id, result: { ok: true } });

    const result = await resultPromise;
    expect((result as { ok: boolean }).ok).toBe(true);
  });

  it("correlates multiple concurrent requests independently", async () => {
    const { startSidecar, invoke } = await import("../sidecarManager");
    startSidecar(() => undefined);

    const p1 = invoke("MethodA", { n: 1 });
    const p2 = invoke("MethodB", { n: 2 });

    await waitForWrittenLines(2);

    const req1 = JSON.parse(mockProc.writtenLines[0]!) as { id: string };
    const req2 = JSON.parse(mockProc.writtenLines[1]!) as { id: string };

    // Respond in reverse order — correlation must still work.
    mockProc.pushResponse({ id: req2.id, result: "result-b" });
    mockProc.pushResponse({ id: req1.id, result: "result-a" });

    expect(await p1).toBe("result-a");
    expect(await p2).toBe("result-b");
  });

  it("rejects the promise when the sidecar returns an error envelope", async () => {
    const { startSidecar, invoke } = await import("../sidecarManager");
    startSidecar(() => undefined);

    const resultPromise = invoke("BadMethod");
    await waitForWrittenLines(1);

    const req = JSON.parse(mockProc.writtenLines[0]!) as { id: string };
    mockProc.pushResponse({ id: req.id, error: "Unknown method 'BadMethod'." });

    await expect(resultPromise).rejects.toThrow("Unknown method 'BadMethod'.");
  });

  it("rejects immediately when the sidecar is not running", async () => {
    const { invoke } = await import("../sidecarManager");
    // Do NOT call startSidecar — the sidecar is "not running".
    await expect(invoke("AnyMethod")).rejects.toThrow(/not running/i);
  });

  // -------------------------------------------------------------------------
  // Crash recovery
  // -------------------------------------------------------------------------

  it("rejects all pending requests when the sidecar crashes", async () => {
    const { startSidecar, invoke } = await import("../sidecarManager");
    startSidecar(() => undefined);

    // Attach catch handlers immediately so rejection is never "unhandled".
    const p1 = invoke("Slow1").catch((e: unknown) => e);
    const p2 = invoke("Slow2").catch((e: unknown) => e);

    await waitForWrittenLines(2);

    // Crash without responding.
    mockProc.crash();
    await flushAsync();

    const r1 = await p1;
    const r2 = await p2;
    expect(r1).toBeInstanceOf(Error);
    expect((r1 as Error).message).toMatch(/crash/i);
    expect(r2).toBeInstanceOf(Error);
    expect((r2 as Error).message).toMatch(/crash/i);
  });

  it("attempts one restart on crash", async () => {
    const { startSidecar } = await import("../sidecarManager");
    const onError = vi.fn();
    startSidecar(onError);

    // Crash the first process.
    mockProc.crash();
    await flushAsync();

    // spawn should have been called twice: initial + restart attempt.
    expect(spawnMockFn).toHaveBeenCalledTimes(2);
    // The onError should NOT have been called yet (restart succeeded).
    expect(onError).not.toHaveBeenCalled();
  });

  it("calls onError callback when restart also fails", async () => {
    spawnMockFn.mockImplementationOnce(() => {
      // First call: return a normal mock proc.
      mockProc = new MockChildProcess();
      return mockProc;
    });
    spawnMockFn.mockImplementationOnce(() => {
      // Second call (restart): throw to simulate ENOENT.
      throw new Error("ENOENT: no such file");
    });

    const onError = vi.fn();
    const { startSidecar } = await import("../sidecarManager");
    startSidecar(onError);

    // Crash the first process — triggers the restart attempt.
    mockProc.crash(1);
    await flushAsync();

    expect(onError).toHaveBeenCalledOnce();
    expect(onError.mock.calls[0]![0] as string).toMatch(/restart/i);
  });

  // -------------------------------------------------------------------------
  // stopSidecar
  // -------------------------------------------------------------------------

  it("closes stdin when stopSidecar is called", async () => {
    const { startSidecar, stopSidecar } = await import("../sidecarManager");
    startSidecar(() => undefined);

    stopSidecar();

    expect(mockProc.stdinEnded).toBe(true);
  });

  // -------------------------------------------------------------------------
  // Malformed sidecar responses
  // -------------------------------------------------------------------------

  it("ignores malformed JSON lines from the sidecar without crashing", async () => {
    const { startSidecar, invoke } = await import("../sidecarManager");
    startSidecar(() => undefined);

    const p = invoke("ValidMethod");
    await waitForWrittenLines(1);

    const req = JSON.parse(mockProc.writtenLines[0]!) as { id: string };

    // Push garbage first, then the real response.
    mockProc.stdout.push("not-valid-json\n");
    mockProc.pushResponse({ id: req.id, result: "ok" });

    await expect(p).resolves.toBe("ok");
  });
});
