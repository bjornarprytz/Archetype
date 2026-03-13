// @vitest-environment node
//
// Tests for fileHandlers.ts (task 5.3).
//
// All file system calls are mocked; no real files are read or written.
// `vi.mock` factories may not reference variables declared outside them
// (they are hoisted to module scope).  We use vi.hoisted() for shared refs.

import { describe, it, expect, vi, beforeEach } from "vitest";

// ---------------------------------------------------------------------------
// Hoisted mock stubs — available inside vi.mock factories AND in test bodies.
// ---------------------------------------------------------------------------

const mocks = vi.hoisted(() => ({
  readFileSync: vi.fn(),
  writeFileSync: vi.fn(),
  mkdirSync: vi.fn(),
  getPath: vi.fn((name: string): string => {
    const map: Record<string, string> = {
      home: "/home/user",
      userData: "/home/user/.config/archetype",
      temp: "/tmp",
      documents: "/home/user/Documents",
      desktop: "/home/user/Desktop",
    };
    return map[name] ?? "/home/user";
  }),
  showOpenDialog: vi.fn(),
  showSaveDialog: vi.fn(),
}));

vi.mock("fs", () => ({
  default: {
    readFileSync: mocks.readFileSync,
    writeFileSync: mocks.writeFileSync,
    mkdirSync: mocks.mkdirSync,
  },
  readFileSync: mocks.readFileSync,
  writeFileSync: mocks.writeFileSync,
  mkdirSync: mocks.mkdirSync,
}));

vi.mock("electron", () => ({
  app: {
    getPath: mocks.getPath,
  },
  dialog: {
    showOpenDialog: mocks.showOpenDialog,
    showSaveDialog: mocks.showSaveDialog,
  },
}));

// Import the module under test after mocks are registered.
import {
  readFile,
  writeFile,
  showOpenDialog,
  showSaveDialog,
  getUserDataPath,
} from "../fileHandlers";

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe("fileHandlers", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Restore the default getPath implementation after each test.
    mocks.getPath.mockImplementation((name: string): string => {
      const map: Record<string, string> = {
        home: "/home/user",
        userData: "/home/user/.config/archetype",
        temp: "/tmp",
        documents: "/home/user/Documents",
        desktop: "/home/user/Desktop",
      };
      return map[name] ?? "/home/user";
    });
  });

  // -------------------------------------------------------------------------
  // readFile
  // -------------------------------------------------------------------------

  it("readFile: reads and returns file contents", () => {
    mocks.readFileSync.mockReturnValue("file content");

    const result = readFile("/home/user/my-game.archetype");

    expect(result).toBe("file content");
    expect(mocks.readFileSync).toHaveBeenCalledWith(
      "/home/user/my-game.archetype",
      "utf-8",
    );
  });

  it("readFile: rejects paths outside allowed directories", () => {
    expect(() => readFile("/etc/passwd")).toThrow(/outside allowed/i);
  });

  it("readFile: rejects relative paths", () => {
    expect(() => readFile("relative/path.archetype")).toThrow(
      /relative paths/i,
    );
  });

  it("readFile: rejects directory traversal attempts", () => {
    // /home/user/../../etc/passwd normalises to /etc/passwd — outside roots.
    expect(() => readFile("/home/user/../../etc/passwd")).toThrow(
      /outside allowed/i,
    );
  });

  // -------------------------------------------------------------------------
  // writeFile
  // -------------------------------------------------------------------------

  it("writeFile: writes content to the given path", () => {
    writeFile("/home/user/Documents/my-game.archetype", "{ json }");

    expect(mocks.writeFileSync).toHaveBeenCalledWith(
      "/home/user/Documents/my-game.archetype",
      "{ json }",
      "utf-8",
    );
  });

  it("writeFile: creates intermediate directories", () => {
    writeFile("/home/user/projects/new/game.archetype", "{}");

    expect(mocks.mkdirSync).toHaveBeenCalledWith("/home/user/projects/new", {
      recursive: true,
    });
  });

  it("writeFile: rejects paths outside allowed directories", () => {
    expect(() => writeFile("/var/log/evil.txt", "payload")).toThrow(
      /outside allowed/i,
    );
  });

  // -------------------------------------------------------------------------
  // showOpenDialog
  // -------------------------------------------------------------------------

  it("showOpenDialog: returns the selected file path", async () => {
    mocks.showOpenDialog.mockResolvedValue({
      canceled: false,
      filePaths: ["/home/user/game.archetype"],
    });

    const result = await showOpenDialog({ title: "Open project" });

    expect(result).toBe("/home/user/game.archetype");
  });

  it("showOpenDialog: returns null when cancelled", async () => {
    mocks.showOpenDialog.mockResolvedValue({
      canceled: true,
      filePaths: [],
    });

    const result = await showOpenDialog({});

    expect(result).toBeNull();
  });

  // -------------------------------------------------------------------------
  // showSaveDialog
  // -------------------------------------------------------------------------

  it("showSaveDialog: returns the chosen save path", async () => {
    mocks.showSaveDialog.mockResolvedValue({
      canceled: false,
      filePath: "/home/user/Documents/newgame.archetype",
    });

    const result = await showSaveDialog({ title: "Save project" });

    expect(result).toBe("/home/user/Documents/newgame.archetype");
  });

  it("showSaveDialog: returns null when cancelled", async () => {
    mocks.showSaveDialog.mockResolvedValue({
      canceled: true,
      filePath: undefined,
    });

    const result = await showSaveDialog({});

    expect(result).toBeNull();
  });

  // -------------------------------------------------------------------------
  // getUserDataPath
  // -------------------------------------------------------------------------

  it("getUserDataPath: returns the electron userData path", () => {
    const result = getUserDataPath();
    expect(result).toBe("/home/user/.config/archetype");
  });
});
