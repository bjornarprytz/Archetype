using Archetype.Core;

namespace Archetype.Tooling.Server;

// ---------------------------------------------------------------------------
//  ReferenceGraph — keyword composition graph (D28)
// ---------------------------------------------------------------------------

/// <summary>
/// Builds and maintains the reverse reference map:
/// <c>usedBy[keyword] = set of entry names that call that keyword</c>.
/// <para>
/// Rebuilt fully on every mutation (D28: incremental optimisation deferred).
/// The graph is stored on <see cref="ProjectState.UsedBy"/> and updated
/// in-place by <see cref="Build"/>.
/// </para>
/// </summary>
public static class ReferenceGraph
{
    /// <summary>
    /// Rebuilds <see cref="ProjectState.UsedBy"/> from all parsed
    /// <see cref="KeywordNode"/> trees in <paramref name="state"/>.
    /// </summary>
    public static void Build(ProjectState state)
    {
        // Start fresh on every build (full rebuild per D28).
        state.UsedBy.Clear();

        // Walk all keyword bodies
        foreach (var (name, kw) in state.Keywords)
        {
            if (kw.BodyNode is not null)
                WalkNode(kw.BodyNode, callerName: name, state);
        }

        // Walk all card effects, conditions, and costs
        foreach (var (name, card) in state.Cards)
        {
            if (card.PrimaryEffectNode is not null)
                WalkBlock(card.PrimaryEffectNode, callerName: name, state);

            if (card.ActivationConditionNode is not null)
                WalkNode(card.ActivationConditionNode, callerName: name, state);

            foreach (var eff in card.AdditionalEffects)
            {
                if (eff.BodyNode is not null)
                    WalkBlock(eff.BodyNode, callerName: name, state);
            }

            foreach (var cost in card.Costs)
            {
                if (cost.BodyNode is not null)
                    WalkBlock(cost.BodyNode, callerName: name, state);
            }
        }

        // Walk phase blocks
        foreach (var phase in state.Phases)
        {
            if (phase.InitNode is not null)    WalkBlock(phase.InitNode,    phase.Name, state);
            if (phase.CleanupNode is not null) WalkBlock(phase.CleanupNode, phase.Name, state);
        }

        // Walk state-based rules
        foreach (var sbr in state.StateBasedRules)
        {
            if (sbr.ConditionNode is not null) WalkNode(sbr.ConditionNode, sbr.Name, state);
            if (sbr.BodyNode is not null)      WalkBlock(sbr.BodyNode,     sbr.Name, state);
        }
    }

    // -----------------------------------------------------------------------
    //  Returns the set of keyword names directly called in the given entry.
    //  Used for ExportGodotClasses signal derivation (D30).
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns the set of keyword names directly invoked in <paramref name="card"/>'s
    /// primary effect, additional effects, and static effects.
    /// </summary>
    public static HashSet<string> DirectRefsFromCard(CardEntry card)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if (card.PrimaryEffectNode is not null)
            CollectDirectRefs(card.PrimaryEffectNode, result);
        foreach (var eff in card.AdditionalEffects)
            if (eff.BodyNode is not null)
                CollectDirectRefs(eff.BodyNode, result);
        foreach (var cost in card.Costs)
            if (cost.BodyNode is not null)
                CollectDirectRefs(cost.BodyNode, result);
        return result;
    }

    // -----------------------------------------------------------------------
    //  Private graph walkers
    // -----------------------------------------------------------------------

    private static void WalkBlock(EffectBlockDef block, string callerName, ProjectState state)
    {
        foreach (var step in block.Steps)
            // Reconstruct the Invocation from the step's keyword name and args.
            WalkNode(new Invocation(step.KeywordName, step.ArgNodes), callerName, state);
    }

    private static void WalkNode(KeywordNode node, string callerName, ProjectState state)
    {
        if (node is Invocation inv)
        {
            // Record that callerName references inv.KeywordName
            if (!state.UsedBy.TryGetValue(inv.KeywordName, out var callers))
            {
                callers = new HashSet<string>(StringComparer.Ordinal);
                state.UsedBy[inv.KeywordName] = callers;
            }
            callers.Add(callerName);

            // Recurse into arguments
            foreach (var arg in inv.Args)
                WalkNode(arg, callerName, state);
        }
    }

    private static void CollectDirectRefs(EffectBlockDef block, HashSet<string> result)
    {
        foreach (var step in block.Steps)
            CollectDirectRefsFromNode(new Invocation(step.KeywordName, step.ArgNodes), result);
    }

    private static void CollectDirectRefsFromNode(KeywordNode node, HashSet<string> result)
    {
        if (node is Invocation inv)
        {
            result.Add(inv.KeywordName);
            foreach (var arg in inv.Args)
                CollectDirectRefsFromNode(arg, result);
        }
    }
}
