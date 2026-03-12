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
    environment: "jsdom",
    globals: true,
    setupFiles: ["./src/renderer/__tests__/setup.ts"],
    include: ["src/renderer/**/*.test.{ts,tsx}", "src/renderer/**/__tests__/**/*.test.{ts,tsx}"],
    exclude: ["**/setup.ts", "**/node_modules/**"],
  },
});
