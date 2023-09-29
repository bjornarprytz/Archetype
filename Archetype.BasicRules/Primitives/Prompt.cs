using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;

namespace Archetype.BasicRules.Primitives;

public class Prompt : EffectPrimitiveDefinition
{
    public override string Name => "PROMPT";
    public override string ReminderText => "Prompt the player to make a choice.";

    protected override OperandDeclaration<Filter, int, int, string> OperandDeclaration { get; } = new();

    public override IEvent Resolve(IResolutionContext context, EffectPayload payload)
    {
        var (filter, min, max, promptText) = OperandDeclaration.UnpackOperands(payload);

        var atoms = filter.ProvideAtoms(context).Select(a => a.Id).ToList();

        return new PromptEvent(atoms, min, max, promptText);
    }

}

public record PromptEvent(IReadOnlyList<Guid> Options, int MinPicks, int MaxPicks, string PromptText) : EventBase;