using Archetype.Framework.Definitions;
using Archetype.Framework.Runtime;

namespace Archetype.BasicRules.Primitives;

public class Prompt : EffectPrimitiveDefinition
{
    public override string Name => "PROMPT";
    public override string ReminderText => "Prompt the player to make a choice.";

    public override IReadOnlyList<OperandDescription> Operands { get; } = OperandHelpers.Required(
        KeywordOperandType.String, 
        KeywordOperandType.String, 
        KeywordOperandType.Integer, 
        KeywordOperandType.Integer
    ).ToList();

    public override IEvent Resolve(IResolutionContext context, Effect payload)
    {
        var (promptText, filterString, min, max) =  payload.Operands.Deconstruct<string, string, int, int>();
        
        var filter = Filter.Parse(filterString);

        var atoms = filter.Filter(context).Select(a => a.Id).ToList();
        
        return new PromptEvent(atoms, min, max, promptText);
    }

}


public record PromptEvent(IReadOnlyList<Guid> Options, int MinPicks, int MaxPicks, string PromptText) : EventBase;