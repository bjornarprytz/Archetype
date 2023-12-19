using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Design;
using Archetype.Framework.State;
using FluentAssertions;

namespace Archetype.Grammar.Tests;

public static class AssertionExtensions
{
    public static void ShouldContain(this IReadOnlyDictionary<string,IKeywordInstance> characteristics, string key, string value, IResolutionContext? context=null)
    {
        characteristics.Should().ContainKey(key);
        characteristics[key].Operands.Should().ContainSingle();
        characteristics[key].Operands[0].GetValue(context).Should().Be(value);
    }
    
    public static void ShouldContain(this IReadOnlyList<IKeywordInstance> keywordInstances, string keyword, IResolutionContext context, params object?[] operands)
    {
        keywordInstances.Should().Contain(k => k.Keyword == keyword && k.Operands.Select(o => o.GetValue(context)).SequenceEqual(operands));
    }
    
    public static void ShouldContain(this IReadOnlyList<IKeywordInstance> keywordInstances, string keyword, params object?[] operands)
    {
        keywordInstances.Should().Contain(k => k.Keyword == keyword && k.Operands.Select(o => o.GetValue(null)).SequenceEqual(operands));
    }
}