using Archetype.Framework.Core.Primitives;
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
        keywordInstances.Should().Contain(k => k.Keyword == keyword && k.Operands.Zip(operands).All(pair => pair.First.IsEquivalentTo(pair.Second, context)));
    }
    
    public static void ShouldContain(this IReadOnlyList<IKeywordInstance> keywordInstances, string keyword, params object?[] operands)
    {
        keywordInstances.Should().Contain(k => k.Keyword == keyword && k.Operands.Zip(operands).All(pair => pair.First.IsEquivalentTo(pair.Second)));
    }
    
    public static void ShouldContain(this IReadOnlyList<IKeywordInstance> keywordInstances, IKeywordInstance keywordInstance)
    {
        keywordInstances.Should().Contain(k => k.IsEquivalentTo(keywordInstance));
    }
    
    public static void ShouldContain(this IReadOnlyList<IKeywordInstance> keywordInstances, IKeywordInstance keywordInstance, IResolutionContext context)
    {
        keywordInstances.Should().Contain(k => k.IsEquivalentTo(keywordInstance, context));
    }
    
    public static bool IsEquivalentTo(this IKeywordInstance keywordInstance, IKeywordInstance other, IResolutionContext context)
    {
        return keywordInstance.Keyword == other.Keyword && keywordInstance.Operands.Zip(other.Operands).All(pair => pair.First.IsEquivalentTo(pair.Second, context));
    }
    
    public static bool IsEquivalentTo(this IKeywordInstance keywordInstance, IKeywordInstance other)
    {
        return keywordInstance.Keyword == other.Keyword && keywordInstance.Operands.Zip(other.Operands).All(pair => pair.First.IsEquivalentTo(pair.Second));
    }
    
    public static bool IsEquivalentTo(this KeywordOperand operand, KeywordOperand other, IResolutionContext context)
    {
        return operand.Type == other.Type && (operand.GetValue(context)?.Equals(other.GetValue(context)) ?? false);
    }
    
    public static bool IsEquivalentTo(this KeywordOperand operand, KeywordOperand other)
    {
        return operand.Type == other.Type && (operand.GetValue(null)?.Equals(other.GetValue(null)) ?? false);
    }
    
    private static bool IsEquivalentTo(this object? operand, object? other)
    {
        return (operand, other) switch
        {
            (KeywordOperand o1, KeywordOperand o2) => o1.IsEquivalentTo(o2),
            (KeywordOperand o1, { } o2) => o2.Equals(o1.GetValue(null)),
            ({ } o1, KeywordOperand o2) => o1.Equals(o2.GetValue(null)),
            _ => operand?.Equals(other) ?? false
        };
    }
    private static bool IsEquivalentTo(this object? operand, object? other, IResolutionContext context)
    {
        return (operand, other) switch
        {
            (KeywordOperand o1, KeywordOperand o2) => o1.IsEquivalentTo(o2, context),
            (KeywordOperand o1, { } o2) => o2.Equals(o1.GetValue(context)),
            ({ } o1, KeywordOperand o2) => o1.Equals(o2.GetValue(context)),
            _ => operand?.Equals(other) ?? false
        };
    }
}