import React from "react";
import ReactDOM from "react-dom/client";
import { App } from "./App";

// React 18 concurrent root — no StrictMode in production to avoid double-invocation
// of effects in dev, which would create two sidecar connections.
// (StrictMode can be added back once the sidecar manager handles idempotent re-connects.)
const rootElement = document.getElementById("root");
if (rootElement === null) {
  throw new Error("Root element #root not found in DOM. Check index.html.");
}

ReactDOM.createRoot(rootElement).render(<App />);
