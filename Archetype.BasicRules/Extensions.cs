using System.Text.RegularExpressions;
using Archetype.Framework.Definitions;
using Archetype.Framework.Proto;
using Archetype.Framework.Runtime;

namespace Archetype.BasicRules;

public static class Extensions
{
    public static IDefinitions AddBasicRules(this IDefinitions definitions)
    {
        definitions.Keywords.Add("Move", new EffectDefinition
        {
            Name = "Move",
            ReminderText = "Move a card from one zone to another.",
            Pattern = new Regex("Move (?<source>.+) to (?<destination>.+)"), // NOTE: This is a very simple pattern, but it's enough for now
            Parse = (input) =>
            {
                var match = new Regex("Move (?<source>.+) to (?<destination>.+)").Match(input);
                var source = match.Groups["source"].Value;
                var destination = match.Groups["destination"].Value;
                
                return new EffectInstance()
                {
                    Keyword = "Move",
                    Targets = new List<TargetDescription>
                    {
                        new (0, new Dictionary<string, string>
                        {
                            { "Type", source },
                        }, false),
                        new(1, new Dictionary<string, string>
                        {
                            { "Type", destination },
                        }, false),
                    },
                };
            },
        });
        
        return definitions;
    }
}