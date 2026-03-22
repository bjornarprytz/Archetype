using Archetype.Core;

namespace Archetype.Engine;

/// <summary>
/// Implements the two-phase <c>CheckLifetimes</c> routine (D6).
/// <para>
/// Called by <c>ActionResolver</c> after every <see cref="BlockExecutor.ExecuteBlock"/>
/// call, including state-based rule blocks and trigger-fired blocks.
/// </para>
/// <list type="bullet">
///   <item><b>Phase 1</b> — Evaluate all while-conditions; remove expired effects and their
///   contributions.  Declarative effects that expire via while-condition go dormant
///   rather than being discarded permanently.</item>
///   <item><b>Phase 2</b> — Re-evaluate dormant declarative effects; activate any whose
///   while-condition is now true.</item>
/// </list>
/// </summary>
internal sealed class LifetimeChecker
{
    private readonly BlockExecutor _executor;

    public LifetimeChecker(BlockExecutor executor) => _executor = executor;

    /// <summary>
    /// Runs the two-phase lifetime check against <paramref name="state"/>.
    /// <paramref name="currentTurn"/> is used to evaluate <see cref="TurnTimer"/>
    /// conditions.
    /// </summary>
    public void CheckLifetimes(GameState state, int currentTurn)
    {
        // Phase 1: expire active effects (cascade until stable).
        bool anyExpired;
        do
        {
            anyExpired = ExpirePass(state, currentTurn);
        } while (anyExpired);

        // Phase 2: activate dormant declarative effects (cascade into Phase 1 if any activate).
        bool anyActivated;
        do
        {
            anyActivated = ActivatePass(state);
            if (anyActivated)
            {
                // Activating may change while-conditions for other effects — re-run Phase 1.
                do
                {
                    anyExpired = ExpirePass(state, currentTurn);
                } while (anyExpired);
            }
        } while (anyActivated);
    }

    // -----------------------------------------------------------------------
    //  Phase 1 — expire active effects
    // -----------------------------------------------------------------------

    private bool ExpirePass(GameState state, int currentTurn)
    {
        var toExpire = new List<StaticEffect>();

        foreach (var se in state.ActiveStaticEffects)
        {
            if (IsExpired(se, state, currentTurn))
                toExpire.Add(se);
        }

        foreach (var se in toExpire)
            Expire(se, state);

        return toExpire.Count > 0;
    }

    private bool IsExpired(StaticEffect se, GameState state, int currentTurn)
    {
        if (se.Lifetime.IsPermanent) return false;

        foreach (var condition in se.Lifetime.Conditions)
        {
            bool satisfied = condition switch
            {
                WhileCondition wc => !EvaluateWhileCondition(wc.Expression, se, state),
                TurnTimer tt      => currentTurn > tt.Turns, // simplified: turns elapsed
                TriggerCount tc   => se.TriggerFireCount >= tc.Count,
                _ => false,
            };
            if (satisfied) return true;
        }
        return false;
    }

    /// <summary>
    /// Removes a static effect and all its contributions.  Classifies the
    /// expiry to decide whether to go dormant (D6).
    /// </summary>
    private void Expire(StaticEffect se, GameState state)
    {
        // Remove all owned contributions.
        foreach (var contribId in se.OwnedContributions)
            RemoveContribution(contribId, state);

        state.ActiveStaticEffects.Remove(se);

        // Classify expiry: terminal vs while-condition only.
        bool hasTerminalCondition = se.Lifetime.Conditions.Any(c =>
            c is TurnTimer or TriggerCount);

        bool isWhileConditionExpiry = !hasTerminalCondition &&
            se.Lifetime.Conditions.Any(c => c is WhileCondition);

        // Dormant only if: declarative, and expiry was while-condition only.
        if (se.IsDeclarative && se.SourceDefinition is not null && isWhileConditionExpiry)
        {
            state.DormantDeclarativeEffects.Add(new DormantDeclarativeEffect
            {
                OwnerAtom          = se.OwnerAtom,
                EffectDef          = se.SourceDefinition,
                CardDefinitionName = se.CardDefinitionName,
                EffectIndex        = se.EffectIndex,
            });
        }
    }

    private static void RemoveContribution(ContributionId id, GameState state)
    {
        if (!state.ContributionRegistry.TryGetValue(id, out var contrib)) return;

        var atom = state.GetAtom(contrib.TargetAtom);
        switch (contrib)
        {
            case ModifierContributionWrapper { Inner: var mc }:
                if (atom.ModifierIndex.TryGetValue(mc.PropertyName, out var mList))
                    mList.RemoveAll(m => m.Id == id);
                break;
            case ConditionContributionWrapper { Inner: var cc }:
                if (atom.ConditionIndex.TryGetValue(cc.ConditionName, out var cList))
                    cList.RemoveAll(c => c.Id == id);
                break;
        }

        state.ContributionRegistry.Remove(id);
    }

    // -----------------------------------------------------------------------
    //  Phase 2 — activate dormant declarative effects
    // -----------------------------------------------------------------------

    private bool ActivatePass(GameState state)
    {
        var toActivate = new List<DormantDeclarativeEffect>();

        foreach (var dormant in state.DormantDeclarativeEffects)
        {
            if (DormantConditionTrue(dormant, state))
                toActivate.Add(dormant);
        }

        foreach (var dormant in toActivate)
        {
            state.DormantDeclarativeEffects.Remove(dormant);
            InstantiateStaticEffect(dormant.EffectDef, dormant.OwnerAtom, state,
                cardDefinitionName: dormant.CardDefinitionName,
                effectIndex: dormant.EffectIndex);
        }

        return toActivate.Count > 0;
    }

    private bool DormantConditionTrue(DormantDeclarativeEffect dormant, GameState state)
    {
        // A dormant effect becomes active when its while-condition evaluates to true.
        foreach (var condition in dormant.EffectDef.Lifetime.Conditions)
        {
            if (condition is WhileCondition wc)
            {
                var bindings = new Dictionary<string, object> { ["source"] = dormant.OwnerAtom };
                return _executor.EvaluateCondition(wc.Expression, state, bindings);
            }
        }
        // No while-condition — something went wrong in classification; activate anyway.
        return true;
    }

    // -----------------------------------------------------------------------
    //  Shared helper — instantiate a static effect (D6)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Provisions a declarative static effect at card-creation time (D6).
    /// <para>
    /// Evaluates the <c>WhileCondition</c> (if any) against the current state.
    /// If the condition is absent or currently true, the effect is instantiated
    /// immediately.  If the condition is currently false, the effect is added to
    /// <see cref="GameState.DormantDeclarativeEffects"/> to be activated later.
    /// </para>
    /// <para>
    /// This shared helper ensures <c>GameSession</c> manifest provisioning and
    /// <c>GameStateBuilder.WithStaticEffect</c> apply identical logic (D6
    /// consequence).
    /// </para>
    /// </summary>
    /// <param name="def">The static effect definition to provision.</param>
    /// <param name="ownerAtom">The atom that owns this static effect.</param>
    /// <param name="state">Current game state; mutated if the effect activates immediately.</param>
    /// <param name="cardDefinitionName">
    /// The name of the <see cref="CardDefinition"/> that owns this effect.
    /// Stored on the <see cref="StaticEffect"/> so snapshot serialization can
    /// produce a <see cref="StaticEffectDefRef"/> without a linear scan (D17).
    /// </param>
    /// <param name="effectIndex">
    /// 0-based index of this effect in <c>CardDefinition.StaticEffects</c>.
    /// </param>
    public void ProvisionDeclarativeEffect(
        StaticEffectDef def,
        AtomId ownerAtom,
        GameState state,
        string? cardDefinitionName = null,
        int effectIndex = 0)
    {
        var whileConditions = def.Lifetime.Conditions.OfType<WhileCondition>().ToList();
        bool hasWhileCondition = whileConditions.Count > 0;

        // Evaluate each while-condition; all must be true for immediate activation.
        bool allTrue = !hasWhileCondition || whileConditions.All(wc =>
        {
            var bindings = new Dictionary<string, object> { ["source"] = ownerAtom };
            return _executor.EvaluateCondition(wc.Expression, state, bindings);
        });

        if (allTrue)
        {
            InstantiateStaticEffect(def, ownerAtom, state,
                cardDefinitionName: cardDefinitionName, effectIndex: effectIndex);
        }
        else
        {
            state.DormantDeclarativeEffects.Add(new DormantDeclarativeEffect
            {
                OwnerAtom = ownerAtom,
                EffectDef = def,
                CardDefinitionName = cardDefinitionName,
                EffectIndex = effectIndex,
            });
        }
    }

    /// <summary>
    /// Creates a new <see cref="StaticEffect"/> instance from a definition.
    /// Allocates fresh IDs; sets <c>TriggerFireCount = 0</c> and
    /// <c>TriggerHighWaterMark = 0</c>.  Called from Phase 2 and card provisioning.
    /// </summary>
    /// <param name="def">The static effect definition to instantiate.</param>
    /// <param name="ownerAtom">The atom that owns this static effect.</param>
    /// <param name="state">Current game state; used to allocate fresh IDs.</param>
    /// <param name="cardDefinitionName">
    /// Optional: the card definition name for declarative effects (D17 snapshot).
    /// </param>
    /// <param name="effectIndex">
    /// Optional: 0-based index in <c>CardDefinition.StaticEffects</c> (D17 snapshot).
    /// </param>
    public StaticEffect InstantiateStaticEffect(
        StaticEffectDef def,
        AtomId ownerAtom,
        GameState state,
        string? cardDefinitionName = null,
        int effectIndex = 0)
    {
        var se = new StaticEffect
        {
            Id               = state.NextStaticEffectId(),
            OwnerAtom        = ownerAtom,
            IsDeclarative    = true,
            SourceDefinition = def,
            CardDefinitionName = cardDefinitionName,
            EffectIndex      = effectIndex,
            Lifetime         = def.Lifetime,
            Trigger          = def.Trigger,
            ParameterModification = def.ParameterModification,
        };

        state.ActiveStaticEffects.Add(se);
        return se;
    }

    // -----------------------------------------------------------------------
    //  While-condition evaluation
    // -----------------------------------------------------------------------

    private bool EvaluateWhileCondition(
        KeywordNode expression,
        StaticEffect se,
        GameState state)
    {
        // The "source" reserved name resolves to the owning atom.
        var bindings = new Dictionary<string, object> { ["source"] = se.OwnerAtom };
        return _executor.EvaluateCondition(expression, state, bindings);
    }
}
