using Archetype.Framework.Definitions;

namespace Archetype.Framework.Runtime.Implementation;

public class Definitions : IDefinitions
{
    public IDictionary<string, KeywordDefinition> Keywords { get; set; }
}