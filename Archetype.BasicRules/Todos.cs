namespace Archetype.BasicRules;

// TODO: Prompt responses should be scoped, in order to avoid collision between contexts. OR maybe I could figure something out with GUIDs
// TODO: Create stronger abstraction around zones etc. in order to let implementations decide how to handle them. E.g. change zone should not give keyword resolvers access to the list directly. The DrawPile is a good example of this, where the order of cards should be hidden. 
// TODO: Polish Syntax
// TODO: Prune classes and interfaces that are not used

// TODO: Reconsider GameAPI (does it work? Is the caller presented with enough information to act?) (e.g. the prompt API)
// TODO: Player object

// TODO: Conditions
// TODO: Pluggable protobuilder
// TODO: Compute (MAX, MIN, COUNT)
// TODO: Modify keyword instances (e.g. IModifiable { KeywordInstance Modify(int) })


// TODO: Add state based effects resolver
// TODO: Stop game loop when victory condition is met

/*
 * TODO: Have a defined validation step for rules and cards, in order to avoid runtime errors
 * TODO: Detect and avoid infinite loops from circular dependencies (a composite keyword that contains itself)
 * TODO: Detect invalid turn order (phases) (No phases, No allowed actions, and when there are allowed actions, PassTurn should be one of them)
 */
