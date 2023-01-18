using Archetype.Core.Effects;

namespace Archetype.Core.Triggers;

public interface ITrigger
{
    bool ConditionMet(IContext context, IEffectResult @event); // This must not have side effects
    IResult Resolve(IContext context); // This should have side effects
    string ContextualRulesText(IContext context);
    string StaticRulesText { get; }
}