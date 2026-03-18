import React from "react";
import ReactDOM from "react-dom/client";
import { loader } from "@monaco-editor/react";
import * as monaco from "monaco-editor";
import { App } from "./App";

// Point @monaco-editor/react at the locally installed monaco-editor package
// rather than the default CDN (cdn.jsdelivr.net).  Without this the wrapper
// tries to load loader.js from jsDelivr, which Electron's "script-src 'self'"
// CSP blocks.  Configuring before any <Editor> component is mounted is
// required — the loader is initialised lazily on first render.
loader.config({ monaco });

// React 18 concurrent root — no StrictMode in production to avoid double-invocation
// of effects in dev, which would create two sidecar connections.
// (StrictMode can be added back once the sidecar manager handles idempotent re-connects.)
const rootElement = document.getElementById("root");
if (rootElement === null) {
  throw new Error("Root element #root not found in DOM. Check index.html.");
}

ReactDOM.createRoot(rootElement).render(<App />);
