using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;
using Archetype.Framework.State;

namespace Archetype.Framework.BaseRules.Keywords.Keywords;

[KeywordCollection]
public static class Target
{
    [TargetRequirements("CARD")]
    public static IEnumerable<ICard> Card(IResolutionContext context, IAtomProvider atomProvider)
    {
        var nodes = context.GameState.Zones.Values.Where(z => z.GetTag("TYPE") == "NODE");
        
        var cardsInNodes = nodes.SelectMany(z => z.Atoms).Where(a => a is ICard).Cast<ICard>().ToList();
        
        return cardsInNodes;
    }
    
    [TargetRequirements("NODE")]
    public static IEnumerable<IZone> Node(IResolutionContext context)
    {
        var nodes = context.GameState.Zones.Values.Where(z => z.GetTag("TYPE") == "NODE").ToList();
        
        return nodes;
    }
}