namespace Archetype.Framework;
 


// TODO: Target is a card concept. Keywords should not have a concept of targets. The operands should be bound at the card level.
// TODO: Revisit wether cards/abilities need to _be_ an actionBlock, or if they could _have_ an actionBlock.


// TODO: Future Scope
    /*
     * Refactor and rename classes so it reflects the structure of the framework. 
     * - Future Scope:
     *  - Unit test game actions
     *  - Provide an API to execute game actions
     *  - Validate the provided rules and proto cards
     *      - Detect and avoid infinite loops from circular dependencies (a composite keyword that contains itself)
     *      - Detect invalid turn order (phases) (No phases, No allowed actions, and when there are allowed actions, PassTurn should be one of them)
     *  - The framework must call into these provided APIs
     *  - Initiate the game loop
     *  - ComputeCount for specific values of a characteristic (e.g. ComputeCount("Health", 10)) 
     */
