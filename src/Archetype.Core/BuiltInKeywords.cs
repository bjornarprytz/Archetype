namespace Archetype.Core;

/// <summary>
/// Static registry of all built-in keyword names, signatures, and metadata.
/// <para>
/// <b>No C# implementations live here</b> — only names and
/// <see cref="ParameterDecl"/> arrays.  Implementations are registered in
/// <c>Archetype.Engine</c> at startup via the built-in keyword dispatch table.
/// </para>
/// <para>
/// <c>Archetype.Build</c> reads this registry at authoring time to validate
/// keyword references.  The engine reads it at startup and asserts that
/// every name here has a registered implementation and no extra names are
/// registered (D15 sync invariant).
/// </para>
/// </summary>
public static class BuiltInKeywords
{
    // -----------------------------------------------------------------------
    //  §9.1  Mutation primitives (D12)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Accumulates a numeric delta into a named accumulator on an atom.
    /// <para>Signature: <c>modify-accumulator(atom: Atom, name: PropertyName, delta: Number) → void</c></para>
    /// </summary>
    public static readonly KeywordDefinition ModifyAccumulator = new(
        Name: "modify-accumulator",
        Parameters: [
            new("atom",  TypeName.Atom,         AtomKindRestriction: null),
            new("name",  TypeName.PropertyName,  AtomKindRestriction: null),
            new("delta", TypeName.Number,         AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.ContributionId,
        Description: "Adds delta to a named accumulator on the target atom.",
        PrimitiveSentinel: "modify-accumulator");

    /// <summary>
    /// Applies a numeric modifier (additive or multiplicative) to a named
    /// property on an atom and returns the contribution ID.
    /// <para>Signature: <c>apply-modifier(atom: Atom, name: PropertyName, kind: Number, value: Number, lifetime: Lifetime) → ContributionId</c></para>
    /// </summary>
    public static readonly KeywordDefinition ApplyModifier = new(
        Name: "apply-modifier",
        Parameters: [
            new("atom",     TypeName.Atom,         AtomKindRestriction: null),
            new("name",     TypeName.PropertyName,  AtomKindRestriction: null),
            new("kind",     TypeName.Number,         AtomKindRestriction: null), // 0=additive,1=multiplicative
            new("value",    TypeName.Number,         AtomKindRestriction: null),
            new("lifetime", TypeName.Lifetime,       AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.ContributionId,
        Description: "Applies a modifier contribution to a property on the target atom.",
        PrimitiveSentinel: "apply-modifier");

    /// <summary>
    /// Removes a modifier contribution by ID.
    /// <para>Signature: <c>remove-modifier(id: ContributionId) → void</c></para>
    /// </summary>
    public static readonly KeywordDefinition RemoveModifier = new(
        Name: "remove-modifier",
        Parameters: [
            new("id", TypeName.ContributionId, AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.Boolean, // void modelled as void-like; returns nothing meaningful
        Description: "Removes a modifier contribution by ID.",
        PrimitiveSentinel: "remove-modifier");

    /// <summary>
    /// Applies a named condition to an atom and returns the contribution ID.
    /// <para>Signature: <c>apply-condition(atom: Atom, name: ConditionName, lifetime: Lifetime) → ContributionId</c></para>
    /// </summary>
    public static readonly KeywordDefinition ApplyCondition = new(
        Name: "apply-condition",
        Parameters: [
            new("atom",     TypeName.Atom,          AtomKindRestriction: null),
            new("name",     TypeName.ConditionName,  AtomKindRestriction: null),
            new("lifetime", TypeName.Lifetime,        AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.ContributionId,
        Description: "Applies a condition contribution to the target atom.",
        PrimitiveSentinel: "apply-condition");

    /// <summary>
    /// Removes all contributions of a named condition from an atom.
    /// <para>Signature: <c>remove-condition(atom: Atom, name: ConditionName) → void</c></para>
    /// </summary>
    public static readonly KeywordDefinition RemoveCondition = new(
        Name: "remove-condition",
        Parameters: [
            new("atom", TypeName.Atom,         AtomKindRestriction: null),
            new("name", TypeName.ConditionName, AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.Boolean,
        Description: "Removes all condition contributions of the given name from the target atom.",
        PrimitiveSentinel: "remove-condition");

    /// <summary>
    /// Creates a new card atom from the named definition and places it in the
    /// specified zone with the given owner.  Returns the new atom's ID.
    /// <para>Signature: <c>create-card(zone: Zone, definition-name: CardDefinitionName, owner: Player) → Atom</c></para>
    /// </summary>
    public static readonly KeywordDefinition CreateCard = new(
        Name: "create-card",
        Parameters: [
            new("zone",            TypeName.Zone,              new[] { AtomKind.Zone }),
            new("definition-name", TypeName.CardDefinitionName, AtomKindRestriction: null),
            new("owner",           TypeName.Player,             new[] { AtomKind.Player }),
        ],
        ReturnType:  TypeName.Atom,
        Description: "Instantiates a new card from the named definition and places it in the specified zone.",
        PrimitiveSentinel: "create-card");

    /// <summary>
    /// Creates a copy of a card (same definition, no runtime state copied) and
    /// places it in the destination zone.  Returns the new atom's ID.
    /// <para>Signature: <c>copy-card(source: Atom, destination-zone: Zone, owner: Player) → Atom</c></para>
    /// </summary>
    public static readonly KeywordDefinition CopyCard = new(
        Name: "copy-card",
        Parameters: [
            new("source",           TypeName.Card,  new[] { AtomKind.Card }),
            new("destination-zone", TypeName.Zone,  new[] { AtomKind.Zone }),
            new("owner",            TypeName.Player, new[] { AtomKind.Player }),
        ],
        ReturnType:  TypeName.Atom,
        Description: "Instantiates a card using the same definition as source; places it in destination-zone with no runtime state from source.",
        PrimitiveSentinel: "copy-card");

    /// <summary>
    /// Creates a new zone atom from the named definition.
    /// Returns the new atom's ID.
    /// <para>Signature: <c>create-zone(owner: Player, definition-name: ZoneDefinitionName) → Atom</c></para>
    /// </summary>
    public static readonly KeywordDefinition CreateZone = new(
        Name: "create-zone",
        Parameters: [
            new("owner",           TypeName.Player,            new[] { AtomKind.Player }),
            new("definition-name", TypeName.ZoneDefinitionName, AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.Atom,
        Description: "Instantiates a new zone from the named definition.",
        PrimitiveSentinel: "create-zone");

    /// <summary>
    /// Moves a card to a different zone.  Captures <c>origin = card.ZoneId</c>
    /// before mutation, updates <c>card.ZoneId = destination</c>, and logs a
    /// <c>move-card</c> event with <c>{ card, origin, destination }</c>.
    /// <para>
    /// The card's owner, accumulators, modifiers, conditions, and active static
    /// effects are unchanged.  Post-block <see cref="Archetype.Core.WhileCondition"/>
    /// re-evaluation via <c>CheckLifetimes</c> handles any <c>in-zone</c> lifetime
    /// conditions naturally — no special handling inside the primitive.
    /// </para>
    /// <para>
    /// If <c>destination</c> does not resolve to an active zone atom in the
    /// current game state, a runtime <see cref="EngineException"/> is thrown.
    /// Self-move (destination == current zone) is valid and still logs an event.
    /// </para>
    /// <para>Signature: <c>move-card(card: Card, destination: Zone) → void</c></para>
    /// </summary>
    public static readonly KeywordDefinition MoveCard = new(
        Name: "move-card",
        Parameters: [
            new("card",        TypeName.Card, new[] { AtomKind.Card }),
            new("destination", TypeName.Zone, new[] { AtomKind.Zone }),
        ],
        ReturnType:  TypeName.Boolean, // void — return value is meaningless
        Description: "Moves a card to the specified destination zone, logging a move-card event with origin captured before mutation.",
        PrimitiveSentinel: "move-card");

    // -----------------------------------------------------------------------
    //  §9.2  Read primitives (property keywords — no side effects)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Reads the current accumulated value of a named property on an atom.
    /// <para>Signature: <c>get-state(atom: Atom, name: PropertyName) → Number</c></para>
    /// </summary>
    public static readonly KeywordDefinition GetState = new(
        Name: "get-state",
        Parameters: [
            new("atom", TypeName.Atom,         AtomKindRestriction: null),
            new("name", TypeName.PropertyName,  AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.Number,
        Description: "Returns the accumulated value of the named property on the atom.",
        PrimitiveSentinel: "get-state");

    /// <summary>
    /// Returns the modifier-adjusted computed value of a named property on an atom.
    /// <para>Signature: <c>get-property(atom: Atom, name: PropertyName) → Number</c></para>
    /// </summary>
    public static readonly KeywordDefinition GetProperty = new(
        Name: "get-property",
        Parameters: [
            new("atom", TypeName.Atom,         AtomKindRestriction: null),
            new("name", TypeName.PropertyName,  AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.Number,
        Description: "Returns the modifier-adjusted value of the named property on the atom.",
        PrimitiveSentinel: "get-property");

    /// <summary>
    /// Returns <c>true</c> if the card is currently in the specified zone.
    /// <para>Signature: <c>in-zone(card: Card, zone: Zone) → Boolean</c></para>
    /// </summary>
    public static readonly KeywordDefinition InZone = new(
        Name: "in-zone",
        Parameters: [
            new("card", TypeName.Card, new[] { AtomKind.Card }),
            new("zone", TypeName.Zone, new[] { AtomKind.Zone }),
        ],
        ReturnType:  TypeName.Boolean,
        Description: "Returns true if the card currently occupies the specified zone.",
        PrimitiveSentinel: "in-zone");

    /// <summary>
    /// Returns <c>true</c> if the named condition is active on the atom.
    /// <para>Signature: <c>has-condition(atom: Atom, name: ConditionName) → Boolean</c></para>
    /// </summary>
    public static readonly KeywordDefinition HasCondition = new(
        Name: "has-condition",
        Parameters: [
            new("atom", TypeName.Atom,          AtomKindRestriction: null),
            new("name", TypeName.ConditionName,  AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.Boolean,
        Description: "Returns true if the named condition is active on the atom.",
        PrimitiveSentinel: "has-condition");

    /// <summary>
    /// Returns the owner atom of a card or zone.
    /// <para>Signature: <c>owner-of(atom: Atom) → Atom</c></para>
    /// </summary>
    public static readonly KeywordDefinition OwnerOf = new(
        Name: "owner-of",
        Parameters: [
            new("atom", TypeName.Atom, new[] { AtomKind.Card, AtomKind.Zone }),
        ],
        ReturnType:  TypeName.Atom,
        Description: "Returns the owner of a card or zone atom.",
        PrimitiveSentinel: "owner-of");

    // -----------------------------------------------------------------------
    //  §9.3  Arithmetic primitives (A12)
    // -----------------------------------------------------------------------

    /// <summary><c>add(a, b) → Number</c></summary>
    public static readonly KeywordDefinition Add = Arithmetic("add", "Returns a + b.");
    /// <summary><c>subtract(a, b) → Number</c></summary>
    public static readonly KeywordDefinition Subtract = Arithmetic("subtract", "Returns a - b.");
    /// <summary><c>multiply(a, b) → Number</c></summary>
    public static readonly KeywordDefinition Multiply = Arithmetic("multiply", "Returns a × b.");
    /// <summary><c>max(a, b) → Number</c></summary>
    public static readonly KeywordDefinition Max = Arithmetic("max", "Returns the larger of a and b.");
    /// <summary><c>min(a, b) → Number</c></summary>
    public static readonly KeywordDefinition Min = Arithmetic("min", "Returns the smaller of a and b.");

    // -----------------------------------------------------------------------
    //  §9.3  Boolean primitives
    // -----------------------------------------------------------------------

    /// <summary><c>and(a, b) → Boolean</c></summary>
    public static readonly KeywordDefinition And = Bool2("and", "Returns a AND b.");
    /// <summary><c>or(a, b) → Boolean</c></summary>
    public static readonly KeywordDefinition Or  = Bool2("or",  "Returns a OR b.");
    /// <summary><c>not(p) → Boolean</c></summary>
    public static readonly KeywordDefinition Not = new(
        Name: "not",
        Parameters: [ new("p", TypeName.Boolean, AtomKindRestriction: null) ],
        ReturnType:  TypeName.Boolean,
        Description: "Returns NOT p.",
        PrimitiveSentinel: "not");

    // -----------------------------------------------------------------------
    //  §9.3  Comparison primitives
    // -----------------------------------------------------------------------

    /// <summary><c>less-than(a, b) → Boolean</c></summary>
    public static readonly KeywordDefinition LessThan    = Compare("less-than",    "Returns a < b.");
    /// <summary><c>greater-than(a, b) → Boolean</c></summary>
    public static readonly KeywordDefinition GreaterThan = Compare("greater-than", "Returns a > b.");
    /// <summary><c>at-least(a, b) → Boolean</c></summary>
    public static readonly KeywordDefinition AtLeast     = Compare("at-least",     "Returns a >= b.");
    /// <summary><c>at-most(a, b) → Boolean</c></summary>
    public static readonly KeywordDefinition AtMost      = Compare("at-most",      "Returns a <= b.");
    /// <summary><c>equal-to(a, b) → Boolean</c></summary>
    public static readonly KeywordDefinition EqualTo     = Compare("equal-to",     "Returns a == b.");

    // -----------------------------------------------------------------------
    //  §9.2  Randomness read primitives (A4)
    // -----------------------------------------------------------------------

    /// <summary><c>random-int(min, max) → Number</c></summary>
    public static readonly KeywordDefinition RandomInt = new(
        Name: "random-int",
        Parameters: [
            new("min", TypeName.Number, AtomKindRestriction: null),
            new("max", TypeName.Number, AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.Number,
        Description: "Returns a uniformly random integer in [min, max] inclusive.",
        PrimitiveSentinel: "random-int");

    // -----------------------------------------------------------------------
    //  §9.2  Event access primitives (D8 — trigger_event binding)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Extracts a named argument value from a trigger event reference.
    /// <para>
    /// The reserved binding <c>trigger_event</c> is always pre-populated
    /// in trigger-fired blocks.  This primitive lets blocks read individual
    /// args without declaring explicit <see cref="EventBinding"/> entries.
    /// </para>
    /// <para>Signature: <c>event-arg(event: EventRef, name: string) → value</c></para>
    /// </summary>
    public static readonly KeywordDefinition EventArg = new(
        Name: "event-arg",
        Parameters: [
            new("event", TypeName.EventRef, AtomKindRestriction: null),
            new("name",  TypeName.PropertyName, AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.Atom, // return type varies; modelled as Atom (loosely typed at runtime)
        Description: "Extracts a named bound argument from a GameEvent reference.",
        PrimitiveSentinel: "event-arg");

    // -----------------------------------------------------------------------
    //  Session reference
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns the singleton session atom.  Takes no parameters.
    /// <para>Signature: <c>session() → Atom</c></para>
    /// </summary>
    public static readonly KeywordDefinition Session = new(
        Name: "session",
        Parameters: [],
        ReturnType:  TypeName.Atom,
        Description: "Returns the singleton session atom.",
        PrimitiveSentinel: "session");

    // -----------------------------------------------------------------------
    //  Game outcome primitives
    // -----------------------------------------------------------------------

    /// <summary>
    /// Declares a winner and ends the game.  Typically called from a
    /// state-based rule body.
    /// <para>
    /// After this executes the session loop exits after the current
    /// post-action sequence finishes.  No further actions are processed.
    /// </para>
    /// <para>Signature: <c>declare-winner(player: Player) → void</c></para>
    /// </summary>
    /// <remarks>
    /// <b>Open gap (flagged):</b> the architecture (D14) states that
    /// state-based rules "produce an outcome" but does not specify the
    /// mechanism.  This primitive is the engine's resolution of that gap.
    /// Confirm with the architect before relying on this in game definitions.
    /// </remarks>
    public static readonly KeywordDefinition DeclareWinner = new(
        Name: "declare-winner",
        Parameters: [
            new("player", TypeName.Player, new[] { AtomKind.Player }),
        ],
        ReturnType:  TypeName.Boolean, // void — return value is meaningless
        Description: "Declares the given player as the winner, ending the game.",
        PrimitiveSentinel: "declare-winner");

    /// <summary>
    /// Declares a draw and ends the game.  Takes no parameters.
    /// <para>Signature: <c>declare-draw() → void</c></para>
    /// </summary>
    public static readonly KeywordDefinition DeclareDraw = new(
        Name: "declare-draw",
        Parameters: [],
        ReturnType:  TypeName.Boolean, // void
        Description: "Declares a draw, ending the game with no winner.",
        PrimitiveSentinel: "declare-draw");

    // -----------------------------------------------------------------------
    //  Registry — the complete set (used for sync assertion at startup)
    // -----------------------------------------------------------------------

    /// <summary>
    /// All built-in keyword definitions.  The engine asserts at startup that
    /// every name here has a registered implementation and no extra names are
    /// registered (D15 sync invariant).
    /// </summary>
    public static readonly IReadOnlyList<KeywordDefinition> All = [
        ModifyAccumulator,
        ApplyModifier,
        RemoveModifier,
        ApplyCondition,
        RemoveCondition,
        CreateCard,
        CopyCard,
        CreateZone,
        MoveCard,
        GetState,
        GetProperty,
        InZone,
        HasCondition,
        OwnerOf,
        Add,
        Subtract,
        Multiply,
        Max,
        Min,
        And,
        Or,
        Not,
        LessThan,
        GreaterThan,
        AtLeast,
        AtMost,
        EqualTo,
        RandomInt,
        EventArg,
        Session,
        DeclareWinner,
        DeclareDraw,
    ];

    // -----------------------------------------------------------------------
    //  Private helpers for concise definition of symmetric primitives
    // -----------------------------------------------------------------------

    private static KeywordDefinition Arithmetic(string name, string desc) => new(
        Name: name,
        Parameters: [
            new("a", TypeName.Number, AtomKindRestriction: null),
            new("b", TypeName.Number, AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.Number,
        Description: desc,
        PrimitiveSentinel: name);

    private static KeywordDefinition Compare(string name, string desc) => new(
        Name: name,
        Parameters: [
            new("a", TypeName.Number, AtomKindRestriction: null),
            new("b", TypeName.Number, AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.Boolean,
        Description: desc,
        PrimitiveSentinel: name);

    private static KeywordDefinition Bool2(string name, string desc) => new(
        Name: name,
        Parameters: [
            new("a", TypeName.Boolean, AtomKindRestriction: null),
            new("b", TypeName.Boolean, AtomKindRestriction: null),
        ],
        ReturnType:  TypeName.Boolean,
        Description: desc,
        PrimitiveSentinel: name);
}
