using Archetype.Framework.Meta;

namespace Archetype.Framework.BaseRules.Keywords;

/*
 * Tags are string-based characteristics. They can be referenced in other rules
 */

[TagCollection]
public class Tags
{
    public string Type { get; }
    public string Subtype { get; }
}