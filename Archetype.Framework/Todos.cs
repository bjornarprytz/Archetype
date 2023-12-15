namespace Archetype.Framework;



// TODO: Finish Framework
    // Add state based effects resolver
    // Figure out how to create the turn order / phases without using the "new" keyword.
    // StartGame action
// TODO: Finish Prototype1
    // Grammar
        // Not all keywords need a keyword attribute, only the ones exposed in the syntax. That's a one-to-one mapping.
        // The rest of the keywords are just primitives that are used to build the composite keywords.
        // So the keywords need an internal ID, and it does not need to be human readable.
        // TODO: Use the connection between syntax and keyword ID to reference the definition
    // Game state init
    

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
     * TODO: Revisit whether cards/abilities need to _be_ an actionBlock, or if they could _have_ an actionBlock.
     *  - Phases are atoms, which means they can have characteristics and currentZone. Is this an affordable quirk?
     */
