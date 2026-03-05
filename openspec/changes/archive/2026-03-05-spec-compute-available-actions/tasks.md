## 1. Architecture & Spec Sign-off (Technical Architect)

- [x] 1.1 Review and ratify the `available-actions-contract` spec — confirm zone-filtering and activation-condition evaluation rules match intended design
      Decision: `GameDefinition.PlayableZoneNames: IReadOnlyList<string>?` — a list of zone definition names from which cards may be played. `null` = no zone filter. Game-level policy set once; per-card exceptions handled via `CardDefinition.ActivationCondition`. Zone-role annotation (`ZoneDefinition.Role = Hand`) rejected: closed enum, too rigid for game-specific zone taxonomies.
- [x] 1.2 Decide whether `GameDefinition` should declare the hand zone name explicitly or whether zone-role annotation (e.g., `ZoneDefinition.Role = Hand`) is the right model — update spec accordingly
- [x] 1.3 Review and ratify the `get-atoms-in-zone` spec — confirm it belongs in the built-in keyword table in `docs/architecture.md`
      Decision: Ratified as a built-in property keyword in D19. `ComputeAvailableActions` uses the equivalent internal state read directly for performance; the primitive exists for use in keyword trees (activation conditions, SBRs, trigger conditions).
- [x] 1.4 Decide whether ability-activation zone restrictions are declared per zone, per card, or per ability — update spec accordingly
      Decision: Per ability, via the existing `ActivationCondition: KeywordNode?` on `NamedEffectBlockDef`. No separate zone-restriction field added to `GameDefinition`. Game creators write `in-zone(source, ...)` in the condition when zone-gating is needed.
- [x] 1.5 Amend `docs/architecture.md` to add the available-actions contract and `get-atoms-in-zone` primitive
      Done: D19 added. `CardDefinition.ActivationCondition` and `GameDefinition.PlayableZoneNames` added to D14 data structures. `get-atoms-in-zone` added to D15 `BuiltInKeywords` description.

## 2. Core Implementation (Implementer)

- [x] 2.1 Add `get-atoms-in-zone` to `BuiltInKeywords.All` in `Archetype.Core/Keywords.cs`
- [x] 2.2 Implement `get-atoms-in-zone` handler in `Archetype.Engine/BuiltInHandlers.cs` — pure read over `GameState`, returns `IEnumerable<AtomId>`, validates zone kind
- [x] 2.3 Add `Kw.GetAtomsInZone(zone)` shorthand in `Archetype.Build/Kw.cs`
- [x] 2.4 Rewrite `GameSession.ComputeAvailableActions` to apply zone filter using `get-atoms-in-zone` logic
- [x] 2.5 Apply activation-condition filtering in `ComputeAvailableActions` using `BlockExecutor.EvaluateCondition` — **important**: manually inject `["source"] = cardAtomId` into the bindings dictionary before calling `EvaluateCondition`. The `source` reserved name (D13) is normally populated by `StaticEffect` on the `WhileCondition` path, but `ActivationCondition` has no `StaticEffect` wrapper, so `ComputeAvailableActions` must supply it explicitly. Forgetting this will cause conditions that reference `ParameterRef("source")` to silently fail or throw.
- [x] 2.6 Ensure `Pass` is always appended to the result

## 3. Tests (Implementer)

- [x] 3.1 Unit test: `get-atoms-in-zone` returns correct atoms for a populated zone
- [x] 3.2 Unit test: `get-atoms-in-zone` returns empty for an empty zone
- [x] 3.3 Unit test: `get-atoms-in-zone` throws on non-zone argument
- [x] 3.4 Integration test: `ComputeAvailableActions` excludes cards not in hand zone
- [x] 3.5 Integration test: `ComputeAvailableActions` excludes cards with false activation condition
- [x] 3.5a Integration test: `ComputeAvailableActions` correctly evaluates an activation condition that references `source` (the card atom) — verifies the `["source"]` binding is injected before `EvaluateCondition` is called (architect note, D13)
- [x] 3.6 Integration test: `ComputeAvailableActions` always includes `Pass`
