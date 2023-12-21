using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Prototype1.Keywords;


[TargetSyntax("T_ANY")]
public class TargetAny : Target<ICard>
{
    public override bool Filter(IAtom atom, IResolutionContext context, IKeywordInstance keywordInstance)
    {
        return atom.CurrentZone is { } zone && zone.HasCharacteristic("TYPE", "NODE", context);
    }
}