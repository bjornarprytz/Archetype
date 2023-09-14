using Archetype.Rules.Definitions;

namespace Archetype.Runtime.Implementation;

public class Definitions : IDefinitions
{
    public IDictionary<string, KeywordDefinition> Keywords { get; set; }
}