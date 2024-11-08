using System.Reflection;
using System.Runtime.CompilerServices;
using Archetype.Framework.Effects.Atomic;

namespace Archetype.Framework.Effects;

public interface IEffectResult
{
    string Keyword { get; }
    bool Success { get; }
    Dictionary<string, object?[]> Results { get; }
}

public record AtomicResult(string Keyword, object? Result, bool Success = true) : IEffectResult
{
    public string[] NestedKeywords { get; } = Array.Empty<string>();
    public Dictionary<string, object?[]> Results { get; } = new() { { Keyword, new [] { Result } } };
}

public record CompositeResult(string Keyword, object? Result, IEffectResult[] NestedResults,  bool Success = true) : IEffectResult
{
    private Dictionary<string, object?[]> _myResults() => new() { { Keyword, new [] { Result } } };
    public Dictionary<string, object?[]> Results =>
        NestedResults
            .Select(r => r.Results)
            .Append(_myResults())
            .Merge();
}

file static class ResultExtensions
{
    public static Dictionary<string, object?[]> Merge(this IEnumerable<Dictionary<string, object?[]>> results)
    {
        var merged = new Dictionary<string, object?[]>();
        
        foreach (var result in results)
        {
            foreach (var (key, values) in result)
            {
                if (merged.TryGetValue(key, out var mergedValues))
                {
                    merged[key] = mergedValues.Concat(values).ToArray();
                }
                else
                {
                    merged[key] = values;
                }
            }
        }
        
        return merged;
    }
}

public static class ResultFactory
{
    private static Dictionary<string, MethodInfo>? _effectMethods = null;
    
    public static IEffectResult Atomic(object? result, [CallerMemberName]string methodName=default)
    {
        return new AtomicResult(GetKeyword(methodName), result);
    }
    
    public static IEffectResult Composite(object? result, IEffectResult[] nestedResults, [CallerMemberName]string methodName=default!)
    {
        return new CompositeResult(GetKeyword(methodName), result, nestedResults);
    }
    
    public static IEffectResult NoOp([CallerMemberName]string methodName=default!)
    {
        return new AtomicResult(GetKeyword(methodName), null, false);
    }

    private static string GetKeyword(string methodName)
    {
        _effectMethods ??= GetEffectMethods();
        
        if (!_effectMethods!.TryGetValue(methodName, out var method))
        {
            throw new ArgumentException($"No effect method with name {methodName} found.");
        }
        
        return method.GetCustomAttribute<EffectAttribute>()!.Keyword;
    }
    
    private static Dictionary<string, MethodInfo> GetEffectMethods()
    {
        var effectCollections = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetCustomAttribute<EffectCollectionAttribute>() is not null);
        
        var effectMethods = effectCollections
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.GetCustomAttribute<EffectAttribute>() is not null)
            .ToDictionary(m => m.Name);
        
        return effectMethods;
    }
}