using System.Reflection;

namespace Archetype.Framework.Extensions;

public static class ReflectionExtensions
{
    public static bool Implements<T>(this Type type)
    {
        return typeof(T).IsAssignableFrom(type);
    }

    public static T? Construct<T>(this Type type)
        where T : class
    {
        return type.GetConstructor(Type.EmptyTypes)?.Invoke(Array.Empty<object>()) as T;
    }
    
    public static bool HasAttribute<T>(this MethodInfo methodInfo)
        where T : Attribute
    {
        return methodInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault() is T;
    }
}