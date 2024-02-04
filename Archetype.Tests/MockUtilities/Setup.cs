using Archetype.Framework.Core.Primitives;
using Archetype.Framework.Extensions;
using NSubstitute;

namespace Archetype.Tests.MockUtilities;

public static class Setup
{
    public static IKeywordInstance KeywordInstance(string keyword, params object?[] operands) => KeywordInstance<IKeywordInstance>(keyword, operands);
    public static T KeywordInstance<T>(string keyword, params object?[] operands)
        where T : class, IKeywordInstance
    {
        var keywordInstance = Substitute.For<T>();
        keywordInstance.Keyword.Returns(keyword);
        keywordInstance.Operands.Returns(operands.Select(o => o.ToOperand()).ToArray());
        return keywordInstance;
    }
    
}