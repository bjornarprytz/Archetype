# Archetype

## Introduction
The goal for this project is to provide a useful framework for a card game.

## TODO
- [x] Ability to make custom effects (not just keyworded ones)
- [x] Clean up the transaction logic in ResourcePool
- [ ] Avoid using null in card zone-transitions
- [ ] Avoid using null in game logic
- [ ] Unit Tests
	- [ ] GameState
	- [ ] Data (De)Serialization
- [ ] Counters (Ongoing effects on a Card or a Unit)
- [x] Refactor Effects, EffectTemplates and TargetRequirements.
- [x] Allow non-unit targets for cards and actions
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
	- [ ] Unit turn order
- [ ] Refactor Cards to be 100% data
	- [ ] Make Card Data (de)serializable
	- [ ] (De)Serialization of concrete action data (damageData, drawData, etc.)
- [ ] Additional Predicates
	- [ ] For Cards
		- [ ] Cost
		- [ ] HasActionType (e.g. DealsDamage)
	- [ ] For Units
		- [ ] Health (e.g. health < 100)
- [ ] CardData
	- [ ] Card types (e.g. Instant, Sorcery)
	- [ ] Rarity
	- [ ] Color
	- [ ] Card Context (e.g. actions on the same card can share some memory)
	- [ ] Triggers (e.g. when a card is discarded, trigger an event)
	- [ ] Referrential Values (e.g. deal damage equal to half your health)
	- [ ] Static effects (e.g. Cards cost 1 less while this card is in the DiscardPile)

### Card Sets
- [ ] API
- [ ] Rules text on cards