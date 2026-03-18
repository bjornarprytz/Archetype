## Context

D9 was written before D14 finalised the player-interaction model. At the time, `IPromptChannel` was the interface through which the engine requested player input. D14 consolidated all player interaction into `IPlayerStrategy` and retired `IPromptChannel` as a standalone type, updating the canonical `ActionResolver` constructor in D14's consequences and addendum. D9's consequences block was left unmodified.

This is a one-line text correction in `docs/architecture.md`. There are no technical decisions to make — the correct signature is already established and stable.

## Goals / Non-Goals

**Goals:**
- Bring D9's documented constructor signature into agreement with the constructor established by D14/A15.
- Make the architecture document self-consistent so an implementer reading it in any order arrives at the same constructor signature.

**Non-Goals:**
- Changing any design decision. The API shape was decided in D14.
- Touching any other section of D9 — the rest of the decision (randomness model, `IRandomSource`, `SeededRandom`, built-in keywords) is correct.

## Decisions

No decisions required. The correct text is:

```
ActionResolver(
  GameDefinition,
  IReadOnlyDictionary<string, IPlayerStrategy>,
  IRandomSource,
  IEngineObserver?)
```

This matches D14's addendum (A15) and the `GameSessionBuilder` fluent API.

## Risks / Trade-offs

- No risk. Pure text change in a document with no dependents other than the implementer's reading comprehension.
- The `add-move-card-primitive` change also carries this fix as task 2.4. If both changes are applied, the second application is a no-op (the line will already be correct). The implementer should check before applying.
