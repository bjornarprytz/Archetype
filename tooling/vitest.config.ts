import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import path from "path";

// Vitest config for renderer unit tests.
// Uses jsdom environment to simulate the browser DOM.
// IPC / contextBridge are mocked at the test level — real Electron APIs are
// never imported in unit tests.
export default defineConfig({
  plugins: [react()],

  resolve: {
    alias: {
      "@shared": path.resolve(__dirname, "src/shared"),
    },
  },

  test: {
    // Default environment for renderer tests.
    environment: "jsdom",
    globals: true,
    // Renderer setup (mocks window.archetype) only applies when running in
    // jsdom.  Main-process tests run in the node environment and must not
    // load the renderer setup file.
    setupFiles: ["./src/renderer/__tests__/setup.ts"],
    include: [
      "src/renderer/**/*.test.{ts,tsx}",
      "src/renderer/**/__tests__/**/*.test.{ts,tsx}",
      // Main-process tests run in the Node environment (no DOM/window).
      "src/main/**/*.test.ts",
    ],
    exclude: ["**/setup.ts", "**/node_modules/**"],
    // Override the environment for main-process test files so they run in
    // Node without the jsdom globals (no `window`, no `document`).
    environmentMatchGlobs: [
      ["src/main/**/*.test.ts", "node"],
    ],
  },
});
