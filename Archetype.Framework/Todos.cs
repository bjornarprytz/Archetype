namespace Archetype.BasicRules;
 
// TODO: Prune classes and interfaces that are not used
// TODO: Scrap the syntax (it's a game dev concern)

// TODO: What remains?
    /*
     * - Bootstrapping API
     *  - RulesBuilder (Produce the Rules)
     *      - keywords
     *      - order and resolution of phases and steps
     *      - state based effects resolution (e.g. death, game end, etc.)
     *  - ProtoBuilder (Produce the ProtoCards)
     *  - StateBuilder (Produce the initial game state)
     *      - custom atoms (Zones, Units, the player, etc.)
     *      - the initial "game board" (DrawPile, Hand, etc.)
     *
     * - Validate the provided rules and proto cards
     *  - Detect and avoid infinite loops from circular dependencies (a composite keyword that contains itself)
     *  - Detect invalid turn order (phases) (No phases, No allowed actions, and when there are allowed actions, PassTurn should be one of them)
     * - The framework must call into these provided APIs
     * - Initiate the game loop
     * - Provide an API to execute game actions
     *
     * - Core:
     *  - CleanupBlock, which is executed after the ActionBlock
     *  - Make generic GameState (and cast from the IGameState wherever necessary)
     *  - Keywords (with unit tests):
     *      - Conditions
     *      - Compute (MAX, MIN, COUNT)
     *      - Modify keyword instances (e.g. IModifiable { KeywordInstance Modify(int) })
     */
