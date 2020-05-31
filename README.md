# Archetype

## Introduction
The goal for this project is to provide a useful framework for a card game.

## TODO
- [x] Ability to make custom effects (not just keyworded ones)
- [x] Clean up the transaction logic in ResourcePool
- [ ] Avoid using null in card zone-transitions
- [ ] Avoid using null in game logic
- [ ] Unit Tests
- [ ] Counters (Ongoing effects on a Card or a Unit)
- [x] Refactor Effects, EffectTemplates and TargetRequirements.
- [ ] Allow non-unit targets for cards and actions
	- [ ] Retrieve action (DiscardPile -> Hand)
	- [ ] Steal action (from zone with different owner -> Hand)
- [ ] Events
	- [ ] Applying / Removing counters
	- [ ] Start / End turn
- [x] Separate the targeting decision from the resolution of EffectArgs (Make it possible to create effects with pre-chosen targets).
- [x] Chaos Bag Mechanic
	- [ ] Chaos tokens
- [ ] Deck Building
	- [ ] Player Card Pool
	- [ ] Unit Roster
- [ ] Flesh out the GameState
	- [ ] Game loop
	- [ ] Prompter
	- [ ] ActionQueue

### Card Sets
- [ ] API
- [ ] Rules text on cards