using Archetype.Core;
using Archetype.Engine;

namespace Archetype.Tests.Helpers;

/// <summary>
/// Constructs a <see cref="GameState"/> directly — bypasses manifest
/// provisioning to give Layer 1 and Layer 2 tests a fast, isolated starting
/// point (D16).
/// <para>
/// Allocates real <see cref="AtomId"/>s from a fresh counter.  The
/// resulting state is equivalent to what manifest provisioning would produce
/// for the declared atoms.
/// </para>
/// <para>Internal because it exposes <c>Archetype.Engine</c> internal types.</para>
/// </summary>
internal sealed class GameStateBuilder
{
    private readonly GameState _state = new();
    private readonly Dictionary<string, AtomId> _playersByName = new(StringComparer.Ordinal);
    private readonly Dictionary<string, AtomId> _zonesByName   = new(StringComparer.Ordinal);

    // -----------------------------------------------------------------------
    //  Fluent builder methods
    // -----------------------------------------------------------------------

    /// <summary>Adds a session atom.  Call at most once; auto-called by <see cref="Build"/> if omitted.</summary>
    public GameStateBuilder WithSession(out AtomId id)
    {
        id = _state.CreateSessionAtom();
        return this;
    }

    /// <summary>Adds a player atom keyed by name.</summary>
    public GameStateBuilder WithPlayer(string name, out AtomId id)
    {
        id = _state.CreateAtom(AtomKind.Player);
        _playersByName[name] = id;
        return this;
    }

    /// <summary>
    /// Adds a zone atom owned by the named player.
    /// </summary>
    public GameStateBuilder WithZone(string defName, string ownerPlayerName, out AtomId id)
    {
        var ownerId = GetPlayer(ownerPlayerName);
        id = _state.CreateAtom(AtomKind.Zone, ownerId: ownerId);
        _zonesByName[defName] = id;
        return this;
    }

    /// <summary>
    /// Adds a card atom placed in the given zone, owned by the named player.
    /// </summary>
    public GameStateBuilder WithCard(string defName, AtomId zone, string ownerPlayerName, out AtomId id)
    {
        var ownerId = GetPlayer(ownerPlayerName);
        id = _state.CreateAtom(AtomKind.Card, ownerId: ownerId, zoneId: zone);
        return this;
    }

    /// <summary>Pre-sets an accumulator value on an atom.</summary>
    public GameStateBuilder WithAccumulator(AtomId atom, string name, double value)
    {
        _state.GetAtom(atom).Accumulators[name] = value;
        return this;
    }

    /// <summary>Pre-applies a condition to an atom.</summary>
    public GameStateBuilder WithCondition(AtomId atom, string conditionName)
    {
        var id = _state.NextContributionId();
        var contribution = new ConditionContribution(
            id, new AtomSource(atom), atom, conditionName);

        var a = _state.GetAtom(atom);
        if (!a.ConditionIndex.TryGetValue(conditionName, out var list))
            a.ConditionIndex[conditionName] = list = new List<ConditionContribution>();
        list.Add(contribution);

        return this;
    }

    /// <summary>
    /// Pre-instantiates a static effect definition on the given owner atom.
    /// The while-condition (if any) is evaluated against the current state;
    /// if true the effect is active, otherwise it is dormant.
    /// </summary>
    public GameStateBuilder WithStaticEffect(
        StaticEffectDef def,
        AtomId ownerAtom,
        BlockExecutor executor)
    {
        var bindings = new Dictionary<string, object> { ["source"] = ownerAtom };
        bool whileTrue = def.Lifetime.Conditions
            .OfType<WhileCondition>()
            .All(wc => executor.EvaluateCondition(wc.Expression, _state, bindings));

        bool hasWhileCondition = def.Lifetime.Conditions.OfType<WhileCondition>().Any();

        if (!hasWhileCondition || whileTrue)
        {
            // Instantiate immediately as an active effect.
            var se = new StaticEffect
            {
                Id               = _state.NextStaticEffectId(),
                OwnerAtom        = ownerAtom,
                IsDeclarative    = true,
                SourceDefinition = def,
                Lifetime         = def.Lifetime,
                Trigger          = def.Trigger,
                ParameterModification = def.ParameterModification,
            };
            _state.ActiveStaticEffects.Add(se);
        }
        else
        {
            // Start dormant — while-condition is currently false.
            _state.DormantDeclarativeEffects.Add(new DormantDeclarativeEffect
            {
                OwnerAtom = ownerAtom,
                EffectDef = def,
            });
        }

        return this;
    }

    /// <summary>Builds the <see cref="GameState"/>.</summary>
    public GameState Build()
    {
        // Auto-create session atom if not explicitly added.
        if (_state.SessionAtomId == AtomId.None)
            _state.CreateSessionAtom();

        return _state;
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private AtomId GetPlayer(string name)
    {
        if (!_playersByName.TryGetValue(name, out var id))
            throw new InvalidOperationException(
                $"Player '{name}' not registered.  Call WithPlayer before using the player name.");
        return id;
    }
}
