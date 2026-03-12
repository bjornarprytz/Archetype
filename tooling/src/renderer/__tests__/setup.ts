import "@testing-library/jest-dom";
import { vi } from "vitest";

// ---------------------------------------------------------------------------
// Test setup — mocks for Electron APIs that are not available in jsdom
// ---------------------------------------------------------------------------

// The renderer accesses the preload bridge via window.archetype (contextBridge).
// In unit tests there is no Electron process, so we provide a typed mock.
// Individual tests can override specific channels via vi.fn().mockResolvedValue().

const mockArchetypeApi = {
  invoke: vi.fn().mockResolvedValue(undefined),
  onNotification: vi.fn().mockReturnValue(() => undefined), // returns unsubscribe fn
};

// Assign to window before any test or component import.
Object.defineProperty(window, "archetype", {
  value: mockArchetypeApi,
  writable: true, // allow test-level overrides
  configurable: true,
});
