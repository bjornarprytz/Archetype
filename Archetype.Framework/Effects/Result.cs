using System.Reflection;
using System.Runtime.CompilerServices;
using Archetype.Framework.Effects.Atomic;

namespace Archetype.Framework.Effects;

public record EffectResult
{
    public required string Keyword { get; init; }
    public IReadOnlyList<object> Parameters { get; init; } = [];
    public bool NoOp { get; init; }
}

internal static class ResultFactory
{
    public static EffectResult Atomic(List<object> parameters, [CallerMemberName]string methodName=null!)
    {
        return new EffectResult
        {
            Keyword = EffectMethods.GetKeyword(methodName),
            Parameters = parameters
        };
    }

    public static EffectResult NoOp(List<object> parameters, [CallerMemberName] string methodName = null!)
    {
     
        return new EffectResult
        {
            Keyword = EffectMethods.GetKeyword(methodName),
            Parameters = parameters,
            NoOp = true
        };
    }
}