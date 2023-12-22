using Archetype.Framework.BaseRules.Keywords.Primitive;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;

namespace Archetype.Prototype1.Keywords;

[ComputedValueSyntax("HAND_SIZE")]
public class HandSize : ComputedValueDefinition
{
    public override int Compute(IResolutionContext context, IKeywordInstance keywordInstance)
    {
        var player = context.GameState.Player;
        return player.Hand.Atoms.Count;
    }
}