## Why

`GameSession.ComputeAvailableActions` is responsible for telling `IPlayerStrategy` which actions are legal on a given turn. The current implementation is a placeholder: it returns every card owned by the active player with no filtering. The architecture does not specify what "legal" means in detail — no cost pre-flight, no activation-condition check, no zone filtering (e.g., only cards in hand are playable). This gap means game authors cannot rely on the engine to enforce basic play legality, and `IPlayerStrategy` implementations receive actions that may fail or be nonsensical.

## What Changes

- Specify the contract for `ComputeAvailableActions`: what makes a card playable, what makes an ability activatable, and what a `Pass` action always represents.
- Specify the primitives needed to support filtering: specifically, a way to query which atoms are in a given zone (a "get atoms in zone" query).
- Define how activation conditions and costs are evaluated during action computation (dry-run vs. best-effort).

## Capabilities

### New Capabilities

- `available-actions-contract`: Specification for the `ComputeAvailableActions` contract — zone filtering, activation-condition evaluation, and cost pre-flight rules.
- `get-atoms-in-zone`: Specification for a `get-atoms-in-zone(zone: Zone) → Atom[]` built-in (or equivalent query mechanism) that returns all atoms currently in a given zone. Required for hand-filtering in `ComputeAvailableActions`.

### Modified Capabilities

None.

## Non-goals

- Specifying the complete rules for any particular game's play conditions — those live in `CardDefinition.ActivationCondition`, which is game-author territory.
- Implementing undo/redo of speculative cost evaluation.
- Specifying how the UI presents available actions to the player.

## Impact

- `docs/architecture.md`: Needs a section or decision entry covering the available-actions contract.
- `Archetype.Core/Keywords.cs`: May need `get-atoms-in-zone` added to `BuiltInKeywords.All`.
- `Archetype.Engine/GameSession.cs`: `ComputeAvailableActions` must be rewritten from the placeholder.
- `Archetype.Engine/BuiltInHandlers.cs`: Handler for `get-atoms-in-zone` if added as a primitive.
- `Archetype.Build/Kw.cs`: Shorthand for `get-atoms-in-zone` if added.

**Owner**: Technical Architect (to specify contract and primitives); Implementer (to implement once specced).
