using System.Reflection;
using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Meta;

namespace Archetype.Framework.Extensions;

public static class KeywordDefinitionExtensions
{
    public static bool TryGetKeywordName(this IKeywordDefinition keywordDefinition, out string? keywordName)
    {
        keywordName = keywordDefinition.GetType().GetCustomAttribute<KeywordAttribute>()?.Keyword;
        return keywordName is not null;
    }
}