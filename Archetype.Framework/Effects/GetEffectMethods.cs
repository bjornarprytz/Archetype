using System.Reflection;

namespace Archetype.Framework.Effects;

public static class EffectMethods
{
    // MethodName -> MethodInfo
    private static Dictionary<string, MethodInfo>? _effectMethodsByMethodName = null;
    private static Dictionary<string, MethodInfo>? _effectMethodsByKeyword = null;
    
    
    public static string GetKeyword(string methodName)
    {
        _effectMethodsByMethodName ??= GetEffectMethodsByMethodName();
        
        if (!_effectMethodsByMethodName.TryGetValue(methodName, out var method))
        {
            throw new ArgumentException($"No effect method with name {methodName} found.");
        }
        
        return method.GetCustomAttribute<EffectAttribute>()!.Keyword;
    }
    
    public static MethodInfo ByKeyword(string keyword)
    {
        _effectMethodsByKeyword ??= GetEffectMethodsByKeyword();
        
        if (!_effectMethodsByKeyword.TryGetValue(keyword, out var method))
        {
            throw new ArgumentException($"No effect method for keyword {keyword} found.");
        }
        
        return method;
    }
    
    private static Dictionary<string, MethodInfo> GetEffectMethodsByKeyword()
    {
        _effectMethodsByMethodName ??= GetEffectMethodsByMethodName();
        _effectMethodsByKeyword ??= _effectMethodsByMethodName
            .ToDictionary(kvp => kvp.Value.GetCustomAttribute<EffectAttribute>()!.Keyword, kvp => kvp.Value);
        
        return _effectMethodsByKeyword;
    }
    
    private static Dictionary<string, MethodInfo> GetEffectMethodsByMethodName()
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