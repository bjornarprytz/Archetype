using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

[Keyword("PROMPT")]
public class Prompt : EffectPrimitiveDefinition
{
    public override string ReminderText => "Prompt the player to make a choice.";

    protected override OperandDeclaration<IEnumerable<IAtom>, int, int, string> OperandDeclaration { get; } = new();

    public override IEvent Resolve(IResolutionContext context, EffectPayload payload)
    {
        var (atoms, min, max, promptText) = OperandDeclaration.Unpack(payload);

        var atomIds = atoms.Select(a => a.Id).ToList();

        return new PromptEvent(payload.Source, payload.Id, atomIds, min, max, promptText);
    }

}

public record PromptEvent(IAtom Source, Guid PromptId, IReadOnlyList<Guid> Options, int MinPicks, int MaxPicks, string PromptText) : EventBase(Source);