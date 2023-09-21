using System.Text.RegularExpressions;
using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;

namespace Archetype.BasicRules;

public static class Extensions
{
    public static IDefinitions AddBasicRules(this IDefinitions definitions)
    {
        definitions.Keywords.Add("Move", new EffectPrimitiveDefinition
        {
            Name = "Move",
            ReminderText = "Move a card from one zone to another.",
        });
        
        return definitions;
    }
}