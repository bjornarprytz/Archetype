import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

// Vite config for the renderer process only.
// Main and preload processes are compiled by tsc directly (see tsconfig.main.json,
// tsconfig.preload.json) — Vite does not bundle Node.js process code.
export default defineConfig({
  plugins: [react()],

  resolve: {
    alias: {
      "@shared": path.resolve(__dirname, "src/shared"),
    },
  },

  // Output to dist/renderer so Electron can find index.html at a stable path.
  build: {
    outDir: "dist/renderer",
    emptyOutDir: true,
  },

  // Dev server port; main process reads VITE_DEV_SERVER_URL in dev mode.
  server: {
    port: 5173,
    strictPort: true,
  },
});
