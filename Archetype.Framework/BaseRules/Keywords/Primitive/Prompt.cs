﻿using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class Prompt : EffectPrimitiveDefinition
{
    public override string Name => "PROMPT";
    public override string ReminderText => "Prompt the player to make a choice.";

    protected override OperandDeclaration<IEnumerable<IAtom>, int, int, string> OperandDeclaration { get; } = new();

    public override IEvent Resolve(IResolutionContext context, EffectPayload payload)
    {
        var (atoms, min, max, promptText) = OperandDeclaration.UnpackOperands(payload);

        var atomIds = atoms.Select(a => a.Id).ToList();

        return new PromptEvent(payload.Source, payload.Id, atomIds, min, max, promptText);
    }

}

public record PromptEvent(IAtom Source, Guid PromptId, IReadOnlyList<Guid> Options, int MinPicks, int MaxPicks, string PromptText) : EventBase(Source);