# Archetype

This is a card game engine. The purpose is to make it easy to create and iterate on rules and content for a card game.

## Design

### Low level

- Whole game state as JSON
- State modifiers
  - JSONPatch
- Reading state: JSONata expressions
  - Getting the modified state might prove challanging here because the JSONata path syntax differs from the JSONPointer syntax used in JSONPatch
- Rules syntax

### Atomic Level

- Atomic Effect implementations
- 

### Game Level

- 