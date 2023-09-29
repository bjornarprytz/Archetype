using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;

namespace Archetype.BasicRules.Primitives;

public class Prompt : EffectPrimitiveDefinition
{
    public override string Name => "PROMPT";
    public override string ReminderText => "Prompt the player to make a choice.";

    public override IReadOnlyList<OperandDescription> Operands { get; } = OperandHelpers.Required(
        KeywordOperandType.Filter, 
        KeywordOperandType.Number, 
        KeywordOperandType.Number,
        KeywordOperandType.String
    ).ToList();

    public override IEvent Resolve(IResolutionContext context, EffectPayload payload)
    {
        var (atoms, min, max, promptText) =  payload.Operands.Deconstruct<IReadOnlyList<Guid>, int, int, string>();

        return new PromptEvent(atoms, min, max, promptText);
    }

}


public record PromptEvent(IReadOnlyList<Guid> Options, int MinPicks, int MaxPicks, string PromptText) : EventBase;