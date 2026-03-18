## 1. Architecture Correction (Technical Architect)

- [x] 1.1 In `docs/architecture.md`, locate the D9 consequences block and replace the stale constructor `ActionResolver(GameDefinition, IPromptChannel, long seed, IEngineObserver?)` with `ActionResolver(GameDefinition, IReadOnlyDictionary<string, IPlayerStrategy>, IRandomSource, IEngineObserver?)`
- [x] 1.2 Verify D9's corrected constructor matches the constructor documented in D14's addendum (A15) — both must show the same parameter types in the same order
- [x] 1.3 Update the architecture status block to record this correction

## 2. Overlap Check (Technical Architect)

- [x] 2.1 Confirm that task 2.4 in `openspec/changes/add-move-card-primitive/tasks.md` is marked complete or struck through, to avoid applying the same fix twice
