using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Keywords;

public static class Target
{
    [TargetRequirements("CARD")]
    public static IEffectResult Card(IResolutionContext context)
    {
        var nodes = context.GameState.Zones.Values.Where(z => z.GetTag("TYPE") == "NODE");
        
        var cardsInNodes = nodes.SelectMany(z => z.Atoms).Where(a => a is ICard).Cast<ICard>().ToList();
        
        return new AllowedTargets(cardsInNodes);
    }
    
    [TargetRequirements("NODE")]
    public static IEffectResult Node(IResolutionContext context)
    {
        var nodes = context.GameState.Zones.Values.Where(z => z.GetTag("TYPE") == "NODE").ToList();
        
        return new AllowedTargets(nodes);
    }
}