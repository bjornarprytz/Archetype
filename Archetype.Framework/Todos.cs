namespace Archetype.Framework;

// TODO: Finish the DependencyInjection tools
    // - Factories, implement and add to DI
// TODO: Add state based effects resolver
// TODO: Write a parser and put it in DI somewhere
// TODO: Keyword definitions should not have static names, but GUIDs. The parser should refer to them by type, and the internal logic can use the GUIDs, it doesn't have to be human readable. 
// TODO: Revisit wether cards/abilities need to _be_ an actionBlock, or if they could _have_ an actionBlock.
// TODO: Phases are atoms, which means they can have characteristics and currentZone. Is this an affordable quirk?


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
