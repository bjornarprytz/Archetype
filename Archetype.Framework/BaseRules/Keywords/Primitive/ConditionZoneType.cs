using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

[ConditionKeyword("ZONE_TYPE", typeof(OperandDeclaration<IAtom, string>))]
public class ConditionZoneType : ConditionDefinition
{
    public override string ReminderText => $"Requires the  type to be of a specific type.";

    protected override OperandDeclaration<IAtom, string> OperandDeclaration { get; } = new();

    public override bool Check(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        var (atom, zoneType) = OperandDeclaration.Unpack(keywordInstance);
        
        return atom.CurrentZone is { } zone && zone.HasCharacteristic("TYPE", zoneType, context);
    }
}