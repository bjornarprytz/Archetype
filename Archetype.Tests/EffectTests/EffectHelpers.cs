using Archetype.Framework.Effects;

namespace Archetype.Tests;

public static class ResultAssertions
{
    public static IEffectResult NoOp(string keyword) => new AtomicResult(keyword, null, false, false);
    
    public static IEffectResult Atomic<T>(string keyword, T result) => new AtomicResult(keyword, result, true, false);
    
    public static IEffectResult Failure(string keyword, string? message=default) => new AtomicResult(keyword, message, false, true);
}