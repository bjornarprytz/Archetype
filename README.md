# Archetype

## Introduction
The goal for this project is to provide a useful framework for a card game.

## TODO
- [ ] Avoid using null in card zone-transitions
- [ ] Avoid using null in game logic
- [ ] Counters (Ongoing effects on a Card or a Unit)
- [x] Refactor Effects, EffectTemplates and TargetRequirements.
- [x] Allow non-unit targets for cards and actions

- [ ] Add more Action Types
	- [ ] Gain / Drain Mana
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
	- [ ] Unit turn order
	- [ ] Limbo Zone (keep cards temporarily here while their actions resolve to avoid them referring to themselves)
- [x] Refactor Cards to be 100% data
	- [x] Make Card Data (de)serializable
	- [x] (De)Serialization of concrete action data (damageData, drawData, etc.)
- [ ] Additional Predicates
	- [ ] For Cards
		- [ ] Cost
		- [ ] HasActionType (e.g. DealsDamage)
	- [ ] For Units
		- [ ] Health (e.g. health < 100)
- [ ] CardData
	- [x] Card types (e.g. Instant, Sorcery)
	- [x] Rarity
	- [x] Color
	- [ ] Triggers (e.g. when a card is discarded, trigger an event)
		- [x] Context-less triggers (can't handle various TriggerArgs)
		- [ ] Zone triggers
			- [ ] Resolve ambiguity between ZoneChange and ZoneChangeTrigger EventArgs.
		- [ ] Card triggers
			- [ ] Tests
		- [x] Unit triggers
			- [ ] Tests
		- [x] AttachTrigger
		- [x] DetachTrigger
		- [x] Use Event Args from (e.g. Deal Damage equal to Cost of the discarded card)
	- [x] Referrential Values (e.g. deal damage equal to half your health)
	- [ ] Static effects (e.g. Cards cost 1 less while this card is in the DiscardPile)
	- [ ] Conditional Actions (e.g. if target health is below 10, deal 2 damage)
- [x] Port to .NET Core
- [ ] Tests
	- [ ] Data Transfer Objects (serialization & data mapping(?))
	- [ ] Game Logic
	- [ ] Mechanics
	- [ ] Game Pieces
	- [ ] General (?)
## Future
- [ ] ActionHandlers (e.g. responding to an action, with its results taken into consideration)
- [ ] Card Context (e.g. actions on the same card can share some memory)