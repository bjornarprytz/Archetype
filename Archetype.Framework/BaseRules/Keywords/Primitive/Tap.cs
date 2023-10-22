using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class Tap : ChangeState<ICard, bool>
{
    public override string Name => "TAP";
    public override string ReminderText => "Tap this card.";
    
    protected override string Property => "TAPPED";
    protected override bool ProduceValue(IResolutionContext context, EffectPayload effectPayload) => true;
}
public class Untap : ChangeState<ICard, bool>
{
    public override string Name => "UNTAP";
    public override string ReminderText => "Untap this card.";
    protected override string Property => "TAPPED";
    protected override bool ProduceValue(IResolutionContext context, EffectPayload effectPayload) => false;
}