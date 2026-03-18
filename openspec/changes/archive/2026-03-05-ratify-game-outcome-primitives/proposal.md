## Why

D14 of `docs/architecture.md` specifies that the game loop should "repeat until a state-based rule produces an outcome," but does not define how a state-based rule communicates a terminal outcome to `GameSession`. During Tier 4 implementation, two built-in primitives (`declare-winner` and `declare-draw`) were invented to fill this gap and are already shipping in the codebase. The architecture must be updated to ratify these decisions so they are not treated as implementation accidents.

## What Changes

- Add `declare-winner(player: Player)` as an official built-in primitive that sets the winning player and flags the game as over.
- Add `declare-draw()` as an official built-in primitive that flags the game as over with no winner.
- Specify the `GameState.DeclareOutcome` first-call-wins invariant: only the first call to either primitive has effect; subsequent calls during the same cascade are silently ignored.
- Specify the `GameIsOver` propagation contract: `GameSession.RunAsync` checks `GameIsOver` after every `ResolveAction` call, the cascade loop breaks on `GameIsOver`, and `RunStateBasedRules` exits early when `GameIsOver` is already true (preventing infinite loops from always-true terminal SBRs).
- Document `player-by-name(name: PropertyName) → Player` as the canonical way to resolve a player atom reference from a static `KeywordNode` tree (where atom IDs are not known at authoring time).

## Capabilities

### New Capabilities

- `game-outcome-primitives`: Specification for the `declare-winner`, `declare-draw`, and `player-by-name` built-in primitives, and the `GameIsOver` propagation contract that connects them to the game loop.

### Modified Capabilities

None. These are entirely new capabilities with no existing spec to delta against.

## Non-goals

- Changing the existing D7/D8 trigger/SBR lifecycle — `GameIsOver` is a terminal condition, not a new phase in the lifecycle.
- Multi-winner outcomes (e.g., team wins) — `declare-winner` takes a single player atom; multi-player victory is out of scope.
- Specifying how the game layer (Godot) surfaces the outcome to the player — that is a presentation concern outside the engine.

## Impact

- `docs/architecture.md`: D14 (and potentially D5) must be updated to include these primitives and propagation contract.
- `Archetype.Core/Keywords.cs`: `BuiltInKeywords.All` already includes these 3 entries (33 total).
- `Archetype.Engine/BuiltInHandlers.cs`: Handlers already implemented.
- `Archetype.Engine/GameState.cs`: `DeclareOutcome`, `GameIsOver`, `PendingWinner` already implemented.
- `Archetype.Engine/ActionResolver.cs`: Early-exit on `GameIsOver` already implemented.
- `Archetype.Engine/GameSession.cs`: Post-`ResolveAction` check already implemented.
- `Archetype.Build/Kw.cs`: `Kw.DeclareWinner`, `Kw.DeclareDraw`, `Kw.PlayerByName` already implemented.

**Owner**: Technical Architect (to amend `docs/architecture.md`); Implementer work is already complete.
