# Review: D19 — ComputeAvailableActions (impl/d19-compute-available-actions)

Reviewed 2026-03-05. Single commit: `435490e Implement D19: get-atoms-in-zone + ComputeAvailableActions (43/43 tests)`.

---

## Defects

### [BLOCKER 1] Ability candidates are iterated inside the zone-filtered block — abilities on cards in non-playable zones are never offered — `GameSession.cs:341–387` — violates D19 Step 4

D19 algorithm step 4 states:

> for each card in **(all active player's cards, regardless of zone)**

The implementation places the ability-enumeration loop **inside** the zone-filtered `foreach` body, after the `continue` at line 353. A card in a non-playable zone causes `continue` to skip the entire rest of the loop body, including the ability check at lines 377–386. This means `ActivatableAbilityOption`s are silently dropped for any card that is not in a playable zone — even though the spec says zone membership is irrelevant for ability activation (ability zone restrictions are the game creator's responsibility via `ActivationCondition`).

**Fix**: Move the ability-enumeration loop to a separate pass over all owned cards, outside the PlayCard zone-filter block. The two loops (PlayCard candidates, ability candidates) should be independent.

---

### [BLOCKER 2] Zone filter does not restrict to the active player's own zones — `GameSession.cs:349–353` — violates D19 Step 2

D19 algorithm step 2 specifies:

> `playableZoneIds = zones in state`
> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`where ZoneDefinition.Name ∈ PlayableZoneNames`
> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`AND OwnerName == activePlayer   // restrict to the active player's own zones`

The implementation only checks:
```csharp
!playableZoneDefNames.Contains(zoneDefName)
```
It does not verify that the zone whose `ZoneId` the card holds is **owned by the active player**. In a two-player game where both players have a `"hand"` zone, a card owned by Player 1 but residing in Player 2's hand zone (e.g. after a `move-card` effect) would be incorrectly offered as playable.

**Fix**: After the definition-name lookup, check `_state.GetAtom(card.ZoneId).OwnerId == playerAtomId` before passing the zone filter.

---

### [BLOCKER 3] No test for ability activation when card is in a non-playable zone — violates D16 / D19 core invariant

D19 step 4 is the key distinguishing invariant between the old placeholder and the new implementation. There is no test asserting that `ActivateAbility` options are returned for a card in a non-playable zone. Because BLOCKER 1 above means the invariant is not currently enforced, this test gap also masks the bug.

**Fix**: Add a test where the active player owns a card with an ability in a non-playable zone (e.g., "battlefield") and assert that `ActivatableAbilities` contains an option for that card. This test will fail until BLOCKER 1 is resolved.

---

### [BLOCKER 4] No test for zone-owner restriction — violates D16 / D19 core invariant

D19 step 2's `AND OwnerName == activePlayer` clause is not tested. There is no test placing an owned card in a zone belonging to the opponent and asserting it is **excluded** from `PlayableCards`.

**Fix**: Add a test with two players where p1's card is in p2's `"hand"` zone. Assert that p1's `PlayableCards` is empty (the zone is not p1's).

---

## Minor Fixes Applied Directly

### [MINOR 1] Stale `<summary>` on `ComputeAvailableActions` — `GameSession.cs:316–323`

The doc comment still said "Simplified implementation: enumerates all cards owned by the player as playable (no activation-condition or cost-dry-run checks)." The implementation now performs zone filtering and activation-condition evaluation. **Fixed in place.**

### [MINOR 2] Orphaned `<summary>` block + missing XML doc on `ActionResolver.Create` — `ActionResolver.cs:65–95`

The `<summary>` block intended for the `Create` factory was placed above `EvaluateCondition`; two consecutive `<summary>` blocks appeared on `EvaluateCondition`, and `Create` had no XML doc at all. **Fixed: the factory `<summary>` has been moved directly above `Create`.**

---

## Observations

- **Runtime-created atoms invisible to `ComputeAvailableActions`**: `_atomDefinitionNames` is populated only during `ProvisionManifest`. Cards or zones created at runtime via `create-card` / `create-zone` are not registered there, so they would be silently skipped in `ComputeAvailableActions`. This is a pre-existing gap not introduced by this PR; worth noting in `docs/implementation-status.md` open gaps.
- **`get-atoms-in-zone` handler and primitive**: Correct. The handler uses `GetAllAtoms()`, applies kind-agnostic zone matching, and the zone atom itself is naturally excluded (its `ZoneId == AtomId.None`). The `BuiltInKeywords` declaration, `Kw.GetAtomsInZone`, and the startup `AssertSync` are all consistent.
- **`GameDefinitionBuilder.WithPlayableZones` / `CardBuilder.WithActivationCondition`**: D19 consequences mention these builders, but neither exists in the current codebase. The direct record construction used in tests is a valid workaround for the testing phase, but the validation (`Build()` must reject unknown `PlayableZoneNames`) should be tracked as a gap when `GameDefinitionBuilder` is eventually implemented.

---

## Verdict

**NEEDS REWORK**

Four blockers must be resolved before this module is marked complete: two code correctness defects (ability-loop placement; zone-owner check) and two missing tests for the core invariants those bugs hide.
