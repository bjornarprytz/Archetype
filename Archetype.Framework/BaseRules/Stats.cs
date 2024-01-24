using Archetype.Framework.Meta;

namespace Archetype.Framework.BaseRules.Keywords;


/*
 * Stats are number-based characteristics.
 */

[StatCollection]
public class Stats
{
    public int Health { get; }
    public int Strength { get; }
    public int ResourceValue { get; }
}