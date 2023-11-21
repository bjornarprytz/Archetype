using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.BasicRules.Primitives;

public class ConditionZoneType<TZone> : ConditionDefinition
    where TZone : IZone
{
    public override string Name => "CONDITION_ZONE_TYPE";
    public override string ReminderText => $"Requires Zone type to be {typeof(TZone).Name}";
    
    public override bool Check(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        return context.Source.CurrentZone is TZone;
    }
}