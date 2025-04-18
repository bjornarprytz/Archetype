using Archetype.Framework.Effects;

namespace Archetype.Tests;

public static class ResultAssertions
{
    public static EffectResult NoOp(string keyword, params IReadOnlyList<object> parameters) => new ()
    {
        Keyword = keyword,
        Parameters = parameters,
        NoOp = true
    };
    
    public static EffectResult Atomic(string keyword, params IReadOnlyList<object> parameters) => new()
    {
        Keyword = keyword,
        Parameters = parameters
    };
}