namespace Archetype.BasicRules;

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
// TODO: Polish Syntax


// TODO: Prompt responses should be scoped, in order to avoid collision between contexts. OR maybe I could figure something out with GUIDs
// TODO: Figure out if payment check could include the context, because some characteristics could depend on the context (e.g. the target of a payment). Or maybe

// TODO: Add state based effects resolver
// TODO: Stop game loop when victory condition is met