using Archetype.Prototype2Data.Zones;

namespace Archetype.Prototype2Data;

public static class RulesExtensions
{
    internal static void Rout(this IMapNode clearing)
    {
        foreach (var building in clearing.Buildings().ToList())
        {
            building.Destroy();
        }
    }
    
    internal static void Damage(this IMapNode clearing)
    {
        foreach (var building in clearing.Buildings().ToList())
        {
            building.Damage();
        }
    }
}