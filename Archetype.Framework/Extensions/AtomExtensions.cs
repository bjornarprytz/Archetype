using Archetype.Framework.Core.Primitives;
using Archetype.Framework.State;

namespace Archetype.Framework.Extensions;

public static class AtomExtensions
{
    public static IAtomProvider ToAtomProvider(this IZone zone)
    {
        return new AtomProvider(_ => zone.Atoms);
    }
    
}