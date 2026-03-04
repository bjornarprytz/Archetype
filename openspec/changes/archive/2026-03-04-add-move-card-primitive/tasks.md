## 1. Domain Model Amendment (Domain Modeler)

- [x] 1.1 Add `move-card(card: Card, destination: Zone) → void` to §9.1 Mutation Primitives in `docs/domain-model.md` as amendment A16; include signature, return type, description, and event logged (`{ card, origin, destination }`)
- [x] 1.2 Update the domain model status block to record A16 and re-sign off

## 2. Architecture Updates (Technical Architect)

- [x] 2.1 Add `move-card` row to the D12 primitives table in `docs/architecture.md` (alongside `create-card`, `copy-card`, `create-zone`)
- [x] 2.2 Add `move-card` to the `BuiltInKeywords` entry in the D15 `Archetype.Core` contents list
- [x] 2.3 Add `Kw.MoveCard(card, destination)` to the `Kw` factory listing in D14
- [x] 2.4 Correct the stale `ActionResolver` constructor in D9 consequences: replace `IPromptChannel` with `IReadOnlyDictionary<string, IPlayerStrategy>`
- [x] 2.5 Update the architecture status block to record the D12/D14/D15 amendments and the D9 correction

## 3. Core Data (Implementer — Archetype.Core)

- [x] 3.1 Add `move-card` entry to `BuiltInKeywords` with `ParameterDecl[]`: `card: Card`, `destination: Zone`; return type `void`; description string

## 4. Build / Authoring (Implementer — Archetype.Build)

- [x] 4.1 Add `Kw.MoveCard(KeywordNode card, KeywordNode destination) → Invocation` shorthand to the `Kw` static factory class

## 5. Engine Implementation (Implementer — Archetype.Engine)

- [x] 5.1 Implement the `move-card` mutation handler: capture `origin = card.ZoneId` before mutation; update `card.ZoneId = destination`; log `GameEvent("move-card", { card, origin, destination })`
- [x] 5.2 Add runtime validation: if `destination` does not resolve to an active zone atom in `GameState`, throw `EngineException`
- [x] 5.3 Register the `move-card` implementation in the built-in keyword dispatch table at startup
- [x] 5.4 Add a startup assertion verifying `move-card` appears in `BuiltInKeywords.All` and has a registered implementation (consistent with D15's sync invariant)

## 6. Tests (Implementer — Archetype.Tests)

- [x] 6.1 Layer 1 unit test: `move-card` updates `AtomSnapshot.ZoneId` to the destination zone
- [x] 6.2 Layer 1 unit test: `move-card` logs event with correct `card`, `origin`, and `destination` bound args; `origin` reflects the zone at call time, not after
- [x] 6.3 Layer 1 unit test: self-move (destination == origin) completes without error and still logs an event
- [x] 6.4 Layer 1 unit test: invalid destination raises `EngineException`
- [x] 6.5 Layer 2 block test: a `while-condition: in-zone(source, zone-X)` static effect expires on `CheckLifetimes` after the card is moved out of zone-X via `move-card`
- [x] 6.6 Layer 2 block test: a dormant declarative static effect with `while-condition: in-zone(source, zone-X)` re-activates on `CheckLifetimes` Phase 2 after `move-card` places the card in zone-X
- [x] 6.7 Layer 2 block test: composite keyword calling `move-card` produces a nested event tree with the `move-card` event as a child of the composite event; a trigger on `EventKeyword: "move-card"` fires
