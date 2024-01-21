using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using NSubstitute;

namespace Archetype.Grammar.Tests;

public static class Mock
{
    public static IKeywordInstance KeywordInstance(string keyword, params object?[] operands) => KeywordInstance<IKeywordInstance>(keyword, operands);
    public static T KeywordInstance<T>(string keyword, params object?[] operands)
        where T : class, IKeywordInstance
    {
        var keywordInstance = Substitute.For<T>();
        keywordInstance.ResolveFuncName.Returns(keyword);
        keywordInstance.Operands.Returns(Declare.Operands(operands.Select(Declare.Operand).ToArray()));
        return keywordInstance;
    }
}