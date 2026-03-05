## 1. Architecture Doc Update (Technical Architect)

- [x] 1.1 Amend D14 in `docs/architecture.md` to specify the terminal-flag pattern: `declare-winner` and `declare-draw` are the mechanism by which a state-based rule signals game-over
- [x] 1.2 Add `declare-winner(player: Player)`, `declare-draw()`, and `player-by-name(name: PropertyName) → Player` to the built-in keyword table in `docs/architecture.md`
- [x] 1.3 Document the `GameIsOver` propagation contract (post-`ResolveAction` check, cascade-loop break, `RunStateBasedRules` early-exit) in `docs/architecture.md`
- [x] 1.4 Document the first-call-wins invariant for `DeclareOutcome` in `docs/architecture.md`
- [x] 1.5 Note `player-by-name` as the canonical authoring-time → runtime player reference bridge in `docs/architecture.md`

## 2. Implementation Verification (Implementer — already complete, verify only)

- [x] 2.1 Confirm `declare-winner`, `declare-draw`, and `player-by-name` are present in `BuiltInKeywords.All` (33 entries) and have handlers in `BuiltInHandlers`
- [x] 2.2 Confirm `GameState.DeclareOutcome` enforces first-call-wins (no-op on second call)
- [x] 2.3 Confirm `RunStateBasedRules` exits early when `GameIsOver` is true at the top of each fixpoint pass
- [x] 2.4 Confirm cascade loop in `ActionResolver` breaks on `GameIsOver`
- [x] 2.5 Confirm `GameSession.RunAsync` checks `GameIsOver` after every `ResolveAction` call
- [x] 2.6 Confirm existing tests cover the `declare-winner` via `player-by-name` end-to-end path (`DeclareWinner_ViaPlayerByName_ReturnsCorrectWinnerName`)
