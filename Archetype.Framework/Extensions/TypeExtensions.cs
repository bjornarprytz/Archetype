namespace Archetype.Framework.Extensions;

public static class TypeExtensions
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
}