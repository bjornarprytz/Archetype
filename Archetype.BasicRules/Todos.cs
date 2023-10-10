namespace Archetype.BasicRules;

// TODO: Polish Syntax

// TODO: Game Phase
// TODO: Reconsider GameAPI (does it work? Is the caller presented with enough information to act?) (e.g. the prompt API)
// TODO: Structure events properly
// TODO: Maybe need a way to structure nested keywords in the resolution context in order to connect the events when the action is finally resolved
// TODO: Player object

// TODO: Conditions
// TODO: Pluggable protobuilder
// TODO: Compute (MAX, MIN, COUNT)
// TODO: Modify keyword instances (e.g. IModifiable { KeywordInstance Modify(int) })
// TODO: Unit tests for a realistic rules implementation

// TODO: Prompt responses should be scoped, in order to avoid collision between contexts. OR maybe I could figure something out with GUIDs
// TODO: Figure out if payment check could include the context, because some characteristics could depend on the context (e.g. the target of a payment). Or maybe

// TODO: Add state based effects resolver
// TODO: Stop game loop when victory condition is met
// TODO: Create stronger abstraction around zones etc. in order to let implementations decide how to handle them. E.g. change zone should not give keyword resolvers access to the list directly. The DrawPile is a good example of this, where the order of cards should be hidden. 
// TODO: Have a defined validation step for rules and cards, in order to avoid runtime errors
/*
 * TODO: Detect and avoid infinite loops from circular dependencies (a composite keyword that contains itself)
 * TODO: Detect invalid turn order (phases) (No phases, No allowed actions, and when there are allowed actions, PassTurn should be one of them)
 * 
 */
