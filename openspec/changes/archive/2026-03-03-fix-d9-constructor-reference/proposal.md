## Why

D14 (and its A15 addendum) retired `IPromptChannel` as a standalone interface and consolidated all player interaction through `IPlayerStrategy`, updating the `ActionResolver` constructor accordingly. D9's consequences block was never updated to match, leaving it with a stale signature that contradicts the established API surface. Any implementer reading D9 before D14 will build to the wrong constructor.

## What Changes

- The `ActionResolver` constructor listed in D9's consequences block is corrected from the obsolete `(GameDefinition, IPromptChannel, long seed, IEngineObserver?)` to the current `(GameDefinition, IReadOnlyDictionary<string, IPlayerStrategy>, IRandomSource, IEngineObserver?)`.
- The architecture status block is updated to record this correction.

## Capabilities

### New Capabilities

*(none — documentation correction only)*

### Modified Capabilities

*(none — no spec-level requirement changes; the API contract was established by D14/A15 and is already correct there)*

## Non-goals

- Changing any design decision. D14 already made the right call; this only aligns D9's text with it.
- Updating any code — there is no implementation yet.

## Impact

- **`docs/architecture.md`**: D9 consequences block corrected; status block updated.
- No other files are affected.

## Personas

The **technical architect** owns this correction.
