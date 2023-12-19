using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Primitive;

public class TargetCardInNode : Target<ICard>
{
    public override bool Filter(IAtom atom, IResolutionContext context, IKeywordInstance keywordInstance)
    {
        return atom.CurrentZone is { } zone && zone.HasCharacteristic("TYPE", "NODE", context);
    }
}