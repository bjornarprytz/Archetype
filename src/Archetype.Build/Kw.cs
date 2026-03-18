using Archetype.Core;

namespace Archetype.Build;

// Bring OnFail and NotifyFlag into scope for the Assert shorthand.
using OnFail = Archetype.Core.OnFail;
using NotifyFlag = Archetype.Core.NotifyFlag;

/// <summary>
/// Static factory class for <see cref="KeywordNode"/> expression trees.
/// <para>
/// Every built-in keyword has a corresponding typed shorthand here; one
/// shorthand per built-in, following a mechanical pattern.  Writing
/// <c>Kw.TakeDamage(Kw.Param("target"), Kw.Num(3))</c> is more readable
/// and more refactoring-safe than constructing <c>Invocation</c> nodes by
/// hand.
/// </para>
/// <para>
/// <c>Kw</c> is a thin façade with no engine logic.  Shorthands produce
/// <see cref="Invocation"/> nodes validated at authoring time during
/// <c>GameDefinitionBuilder.Build()</c>.
/// </para>
/// </summary>
public static class Kw
{
    // -----------------------------------------------------------------------
    //  Generic node constructors
    // -----------------------------------------------------------------------

    /// <summary>Creates a <see cref="ParameterRef"/> node that resolves to the named parameter.</summary>
    public static ParameterRef Param(string name) => new(name);

    /// <summary>Creates a <see cref="Literal"/> node for a numeric value.</summary>
    public static Literal Num(double value) => new(value);

    /// <summary>Creates a <see cref="Literal"/> node for a boolean value.</summary>
    public static Literal Bool(bool value) => new(value);

    /// <summary>Creates a <see cref="Literal"/> node for a string value.</summary>
    public static Literal Str(string value) => new(value);

    /// <summary>Creates a <see cref="Literal"/> node for an atom reference.</summary>
    public static Literal Atom(AtomId id) => new(id);

    /// <summary>
    /// Creates a raw <see cref="Invocation"/> node for any keyword name.
    /// Use the typed shorthands below for built-ins.
    /// </summary>
    public static Invocation Invoke(string keyword, params KeywordNode[] args) =>
        new(keyword, args);

    // -----------------------------------------------------------------------
    //  §9.1  Mutation primitive shorthands (D12)
    // -----------------------------------------------------------------------

    /// <summary>
    /// <c>modify-accumulator(atom, name, delta)</c> — adds <paramref name="delta"/>
    /// to the named accumulator on <paramref name="atom"/>.
    /// </summary>
    public static Invocation ModifyAccumulator(KeywordNode atom, KeywordNode name, KeywordNode delta) =>
        new("modify-accumulator", atom, name, delta);

    /// <summary>
    /// <c>apply-modifier(atom, name, kind, value, lifetime)</c> — applies a
    /// modifier contribution and returns its <see cref="Core.ContributionId"/>.
    /// </summary>
    public static Invocation ApplyModifier(
        KeywordNode atom, KeywordNode name, KeywordNode kind,
        KeywordNode value, KeywordNode lifetime) =>
        new("apply-modifier", atom, name, kind, value, lifetime);

    /// <summary><c>remove-modifier(id)</c></summary>
    public static Invocation RemoveModifier(KeywordNode id) =>
        new("remove-modifier", id);

    /// <summary>
    /// <c>apply-condition(atom, name, lifetime)</c> — applies a condition
    /// contribution and returns its <see cref="Core.ContributionId"/>.
    /// </summary>
    public static Invocation ApplyCondition(KeywordNode atom, KeywordNode name, KeywordNode lifetime) =>
        new("apply-condition", atom, name, lifetime);

    /// <summary><c>remove-condition(atom, name)</c></summary>
    public static Invocation RemoveCondition(KeywordNode atom, KeywordNode name) =>
        new("remove-condition", atom, name);

    /// <summary>
    /// <c>create-card(zone, definition-name, owner)</c> — instantiates a
    /// card atom and returns it.
    /// </summary>
    public static Invocation CreateCard(KeywordNode zone, KeywordNode definitionName, KeywordNode owner) =>
        new("create-card", zone, definitionName, owner);

    /// <summary>
    /// <c>copy-card(source, destination-zone, owner)</c> — copies a card
    /// atom and returns the new atom.
    /// </summary>
    public static Invocation CopyCard(KeywordNode source, KeywordNode destinationZone, KeywordNode owner) =>
        new("copy-card", source, destinationZone, owner);

    /// <summary>
    /// <c>create-zone(owner, definition-name)</c> — instantiates a zone
    /// atom and returns it.
    /// </summary>
    public static Invocation CreateZone(KeywordNode owner, KeywordNode definitionName) =>
        new("create-zone", owner, definitionName);

    /// <summary>
    /// <c>move-card(card, destination)</c> — moves an existing card to
    /// the specified zone.
    /// <para>
    /// <paramref name="card"/> must resolve to a <see cref="AtomKind.Card"/>
    /// atom at runtime; <paramref name="destination"/> must resolve to an
    /// active <see cref="AtomKind.Zone"/> atom.  The card's owner,
    /// accumulators, modifiers, conditions, and active static effects are
    /// unchanged.  Self-move is valid.
    /// </para>
    /// </summary>
    public static Invocation MoveCard(KeywordNode card, KeywordNode destination) =>
        new("move-card", card, destination);

    // -----------------------------------------------------------------------
    //  §9.2  Read primitive shorthands
    // -----------------------------------------------------------------------

    /// <summary><c>get-state(atom, name)</c> — accumulated value of a named property.</summary>
    public static Invocation GetState(KeywordNode atom, KeywordNode name) =>
        new("get-state", atom, name);

    /// <summary><c>get-property(atom, name)</c> — modifier-adjusted value.</summary>
    public static Invocation GetProperty(KeywordNode atom, KeywordNode name) =>
        new("get-property", atom, name);

    /// <summary><c>in-zone(card, zone)</c> — true if the card is in the zone.</summary>
    public static Invocation InZone(KeywordNode card, KeywordNode zone) =>
        new("in-zone", card, zone);

    /// <summary><c>has-condition(atom, name)</c> — true if the condition is active.</summary>
    public static Invocation HasCondition(KeywordNode atom, KeywordNode name) =>
        new("has-condition", atom, name);

    /// <summary>
    /// <c>owner-of(atom)</c> — returns the owner atom.
    /// Argument must resolve to <see cref="AtomKind.Card"/> or <see cref="AtomKind.Zone"/>.
    /// </summary>
    public static Invocation OwnerOf(KeywordNode atom) =>
        new("owner-of", atom);

    /// <summary><c>session()</c> — returns the singleton session atom.</summary>
    public static Invocation Session() =>
        new("session");

    // -----------------------------------------------------------------------
    //  §9.3  Arithmetic shorthands
    // -----------------------------------------------------------------------

    /// <summary><c>add(a, b)</c></summary>
    public static Invocation Add(KeywordNode a, KeywordNode b)      => new("add",      a, b);
    /// <summary><c>subtract(a, b)</c></summary>
    public static Invocation Subtract(KeywordNode a, KeywordNode b) => new("subtract", a, b);
    /// <summary><c>multiply(a, b)</c></summary>
    public static Invocation Multiply(KeywordNode a, KeywordNode b) => new("multiply", a, b);
    /// <summary><c>max(a, b)</c></summary>
    public static Invocation Max(KeywordNode a, KeywordNode b)      => new("max",      a, b);
    /// <summary><c>min(a, b)</c></summary>
    public static Invocation Min(KeywordNode a, KeywordNode b)      => new("min",      a, b);

    // -----------------------------------------------------------------------
    //  §9.3  Boolean / comparison shorthands
    // -----------------------------------------------------------------------

    /// <summary><c>and(a, b)</c></summary>
    public static Invocation And(KeywordNode a, KeywordNode b)         => new("and",          a, b);
    /// <summary><c>or(a, b)</c></summary>
    public static Invocation Or(KeywordNode a, KeywordNode b)          => new("or",           a, b);
    /// <summary><c>not(p)</c></summary>
    public static Invocation Not(KeywordNode p)                        => new("not",          p);
    /// <summary><c>less-than(a, b)</c></summary>
    public static Invocation LessThan(KeywordNode a, KeywordNode b)    => new("less-than",    a, b);
    /// <summary><c>greater-than(a, b)</c></summary>
    public static Invocation GreaterThan(KeywordNode a, KeywordNode b) => new("greater-than", a, b);
    /// <summary><c>at-least(a, b)</c></summary>
    public static Invocation AtLeast(KeywordNode a, KeywordNode b)     => new("at-least",     a, b);
    /// <summary><c>at-most(a, b)</c></summary>
    public static Invocation AtMost(KeywordNode a, KeywordNode b)      => new("at-most",      a, b);
    /// <summary><c>equal-to(a, b)</c></summary>
    public static Invocation EqualTo(KeywordNode a, KeywordNode b)     => new("equal-to",     a, b);

    // -----------------------------------------------------------------------
    //  §9.2  Randomness shorthands
    // -----------------------------------------------------------------------

    /// <summary><c>random-int(min, max)</c></summary>
    public static Invocation RandomInt(KeywordNode min, KeywordNode max) => new("random-int", min, max);

    /// <summary>
    /// <c>event-arg(event, name)</c> — extracts a named argument from a trigger event reference.
    /// Use <c>Kw.Param("trigger_event")</c> as the <paramref name="ev"/> argument inside fired blocks.
    /// </summary>
    public static Invocation EventArg(KeywordNode ev, KeywordNode name) => new("event-arg", ev, name);

    // -----------------------------------------------------------------------
    //  Player lookup shorthands
    // -----------------------------------------------------------------------

    // -----------------------------------------------------------------------
    //  Zone query shorthands (D19)
    // -----------------------------------------------------------------------

    /// <summary>
    /// <c>get-atoms-in-zone(zone)</c> — returns all atoms currently in the
    /// given zone as a <see cref="TypeName.Collection"/> of atom IDs.
    /// <para>Pure read; no state mutation or event logging.</para>
    /// </summary>
    public static Invocation GetAtomsInZone(KeywordNode zone) => new("get-atoms-in-zone", zone);

    // -----------------------------------------------------------------------
    //  Player lookup shorthands
    // -----------------------------------------------------------------------

    /// <summary>
    /// <c>player-by-name(name)</c> — returns the player atom registered under
    /// the given name string.
    /// <para>
    /// The idiomatic way to pass a player reference to
    /// <c>declare-winner</c> from a state-based rule body, where the atom ID
    /// is not known at definition time.
    /// </para>
    /// </summary>
    public static Invocation PlayerByName(KeywordNode name) => new("player-by-name", name);

    // -----------------------------------------------------------------------
    //  Game outcome shorthands
    // -----------------------------------------------------------------------

    /// <summary>
    /// <c>declare-winner(player)</c> — ends the game with the given player atom
    /// as the winner.  Typically used inside a state-based rule body.
    /// </summary>
    public static Invocation DeclareWinner(KeywordNode player) => new("declare-winner", player);

    /// <summary>
    /// <c>declare-draw()</c> — ends the game as a draw.
    /// Typically used inside a state-based rule body.
    /// </summary>
    public static Invocation DeclareDraw() => new("declare-draw");

    // -----------------------------------------------------------------------
    //  assert shorthand (D20)
    // -----------------------------------------------------------------------

    /// <summary>
    /// <c>assert(condition, on_fail, notify)</c> — evaluates <paramref name="condition"/>
    /// and signals a failure according to <paramref name="onFail"/> and
    /// <paramref name="notify"/> when the condition is false.
    /// <para>
    /// <b>Cost-body behaviour:</b> when executed inside a cost body
    /// (<c>IsCostBody = true</c>), the engine ignores <paramref name="onFail"/>
    /// and <paramref name="notify"/> and always raises <see cref="Core.EngineException"/>
    /// silently (panic / off).
    /// </para>
    /// <para>
    /// <b>Outside cost bodies</b> — failure behaviour:
    /// <list type="bullet">
    ///   <item><see cref="OnFail.Continue"/> / <see cref="NotifyFlag.On"/>: calls <c>OnDiagnostic</c>, continues.</item>
    ///   <item><see cref="OnFail.Stop"/> / <see cref="NotifyFlag.On"/>: calls <c>OnDiagnostic</c>, halts block.</item>
    ///   <item><see cref="OnFail.Panic"/> / <see cref="NotifyFlag.On"/>: calls <c>OnDiagnostic</c>, raises exception.</item>
    ///   <item>Any + <see cref="NotifyFlag.Off"/>: skips <c>OnDiagnostic</c>, applies <paramref name="onFail"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// <c>assert</c> NEVER appends to the event log under any outcome.
    /// </para>
    /// </summary>
    /// <param name="condition">The boolean condition to test.</param>
    /// <param name="onFail">
    /// What to do on failure.  Defaults to <see cref="OnFail.Continue"/>.
    /// </param>
    /// <param name="notify">
    /// Whether to call <c>OnDiagnostic</c>.  Defaults to <see cref="NotifyFlag.On"/>.
    /// </param>
    public static Invocation Assert(
        KeywordNode condition,
        OnFail onFail = OnFail.Continue,
        NotifyFlag notify = NotifyFlag.On) =>
        new("assert",
            condition,
            new Literal((double)(int)onFail),
            new Literal((double)(int)notify));

    // -----------------------------------------------------------------------
    //  Ownership condition shorthand (D24)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns a condition that is true when the <c>source</c> binding is owned
    /// by the current active player.
    /// <para>
    /// Expands to:
    /// <code>Kw.EqualTo(Kw.OwnerOf(Kw.Param("source")), Kw.GetState(Kw.Session(), "active-player"))</code>
    /// </para>
    /// <para>
    /// <b>Requirement:</b> the game must declare a session state field named
    /// <c>"active-player"</c> whose value is the atom ID of the currently active
    /// player.  If this field is absent at runtime, <c>EvaluateCondition</c> will
    /// throw.  Use this shorthand together with game-level initialization that sets
    /// the session <c>"active-player"</c> field at the start of each turn.
    /// </para>
    /// <para>
    /// This shorthand is the migration path from the old implicit ownership filter
    /// that was removed from <c>ComputeAvailableActions</c> in D24.  Add it to
    /// <see cref="Core.CardDefinition.ActivationCondition"/> or
    /// <see cref="Core.NamedEffectBlockDef.ActivationCondition"/> on any card or
    /// ability that should only be playable by the card's owner.
    /// </para>
    /// </summary>
    public static Invocation OwnedByActivePlayer() =>
        EqualTo(
            OwnerOf(Param("source")),
            GetState(Session(), Str("active-player")));
}
