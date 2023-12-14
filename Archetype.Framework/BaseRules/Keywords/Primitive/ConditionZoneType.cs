using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

[ConditionKeyword("ZONE_TYPE", typeof(OperandDeclaration<IZone>))]
public class ConditionZoneType<TZone> : ConditionDefinition
    where TZone : IZone
{
    public override string ReminderText => $"Requires Zone type to be {typeof(TZone).Name}";
    
    public override bool Check(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        return context.Source.CurrentZone is TZone;
    }
}