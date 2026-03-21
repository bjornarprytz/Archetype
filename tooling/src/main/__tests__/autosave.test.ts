// @vitest-environment node
//
// Tests for autosave.ts (task 5.4).
//
// Uses fake timers so the 60-second delay does not slow tests.
// The sidecar and fileHandlers are mocked.

import {
  describe,
  it,
  expect,
  vi,
  beforeEach,
  afterEach,
} from "vitest";

// ---------------------------------------------------------------------------
// Mocks — must be declared before importing autosave
// ---------------------------------------------------------------------------

vi.mock("../sidecarManager", () => ({
  invoke: vi.fn(),
}));

vi.mock("../fileHandlers", () => ({
  writeFile: vi.fn(),
}));

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe("autosave", () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.resetModules();
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  // -------------------------------------------------------------------------
  // Timer fires after 60 seconds of inactivity
  // -------------------------------------------------------------------------

  it("autosaves 60 seconds after the last mutation", async () => {
    const { invoke } = await import("../sidecarManager");
    const { writeFile } = await import("../fileHandlers");

    vi.mocked(invoke).mockResolvedValue({ json: '{"version":1}' });

    const { setProjectPath, notifyMutation } = await import("../autosave");
    setProjectPath("/home/user/game.archetype");
    notifyMutation();

    // Timer has not fired yet.
    expect(invoke).not.toHaveBeenCalled();

    // Advance time to just before the threshold — should not fire.
    await vi.advanceTimersByTimeAsync(59_999);
    expect(invoke).not.toHaveBeenCalled();

    // Advance past the threshold.
    await vi.advanceTimersByTimeAsync(1);

    // Allow async SaveProject to complete.
    await vi.runAllTimersAsync();

    expect(invoke).toHaveBeenCalledWith("SaveProject", null);
    expect(writeFile).toHaveBeenCalledWith(
      "/home/user/game.archetype",
      '{"version":1}',
    );
  });

  // -------------------------------------------------------------------------
  // Timer resets on each mutation
  // -------------------------------------------------------------------------

  it("resets the timer on each mutation call", async () => {
    const { invoke } = await import("../sidecarManager");
    vi.mocked(invoke).mockResolvedValue({ json: "{}" });

    const { setProjectPath, notifyMutation } = await import("../autosave");
    setProjectPath("/home/user/game.archetype");

    notifyMutation();
    await vi.advanceTimersByTimeAsync(50_000);

    // Second mutation resets the 60s countdown.
    notifyMutation();
    await vi.advanceTimersByTimeAsync(50_000);

    // Only 50s have passed since the reset — should not fire yet.
    expect(invoke).not.toHaveBeenCalled();

    // Now let the full 60s elapse after the second mutation.
    await vi.advanceTimersByTimeAsync(10_001);
    await vi.runAllTimersAsync();

    expect(invoke).toHaveBeenCalledOnce();
  });

  // -------------------------------------------------------------------------
  // No-op when no project is open
  // -------------------------------------------------------------------------

  it("does nothing when no project path is set", async () => {
    const { invoke } = await import("../sidecarManager");

    const { notifyMutation } = await import("../autosave");
    // clearProjectPath is the initial state — no call to setProjectPath.
    notifyMutation();

    await vi.advanceTimersByTimeAsync(120_000);
    await vi.runAllTimersAsync();

    expect(invoke).not.toHaveBeenCalled();
  });

  // -------------------------------------------------------------------------
  // clearProjectPath disables the timer
  // -------------------------------------------------------------------------

  it("cancels the timer when the project is closed", async () => {
    const { invoke } = await import("../sidecarManager");
    vi.mocked(invoke).mockResolvedValue({ json: "{}" });

    const { setProjectPath, clearProjectPath, notifyMutation } =
      await import("../autosave");

    setProjectPath("/home/user/game.archetype");
    notifyMutation();

    // Close the project before the timer fires.
    clearProjectPath();

    await vi.advanceTimersByTimeAsync(120_000);
    await vi.runAllTimersAsync();

    expect(invoke).not.toHaveBeenCalled();
  });

  // -------------------------------------------------------------------------
  // Errors are swallowed silently
  // -------------------------------------------------------------------------

  it("swallows sidecar errors silently without throwing", async () => {
    const { invoke } = await import("../sidecarManager");
    vi.mocked(invoke).mockRejectedValue(new Error("Sidecar crashed"));

    const { setProjectPath, notifyMutation } = await import("../autosave");
    setProjectPath("/home/user/game.archetype");
    notifyMutation();

    // Should not throw even when the sidecar rejects.
    await expect(
      vi.advanceTimersByTimeAsync(60_001).then(() => vi.runAllTimersAsync()),
    ).resolves.not.toThrow();
  });

  // -------------------------------------------------------------------------
  // shutdownAutosave cancels pending timer
  // -------------------------------------------------------------------------

  it("shutdownAutosave cancels any pending timer", async () => {
    const { invoke } = await import("../sidecarManager");
    vi.mocked(invoke).mockResolvedValue({ json: "{}" });

    const { setProjectPath, notifyMutation, shutdownAutosave } =
      await import("../autosave");

    setProjectPath("/home/user/game.archetype");
    notifyMutation();

    shutdownAutosave();

    await vi.advanceTimersByTimeAsync(120_000);
    await vi.runAllTimersAsync();

    expect(invoke).not.toHaveBeenCalled();
  });
});
