using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using Archetype.Framework.Meta;

namespace Archetype.Framework.BaseRules.Keywords;

[ComputeCollection]
public static class Compute
{
    [Compute("MAX")]
    public static int Max(IResolutionContext context, IAtomProvider atomProvider, string statKey)
    {
        return atomProvider.ProvideAtoms(context).Max(a => a.GetStatValue(statKey)) ?? 0;
    }
    
    [Compute("MIN")]
    public static int Min(IResolutionContext context, IAtomProvider atomProvider, string statKey)
    {
        return atomProvider.ProvideAtoms(context).Min(a => a.GetStatValue(statKey)) ?? 0;
    }
    
    [Compute("SUM")]
    public static int Sum(IResolutionContext context, IAtomProvider atomProvider, string statKey)
    {
        return atomProvider.ProvideAtoms(context).Sum(a => a.GetStatValue(statKey)) ?? 0;
    }
    
    [Compute("COUNT")]
    public static int Count(IResolutionContext context, IAtomProvider atomProvider, string statKey)
    {
        return atomProvider.ProvideAtoms(context).Count(a => a.GetStatValue(statKey) != null);
    }
}