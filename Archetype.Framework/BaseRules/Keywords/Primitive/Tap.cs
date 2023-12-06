using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

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