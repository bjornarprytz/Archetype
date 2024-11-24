using System.Linq.Expressions;
using System.Reflection;
using Archetype.Framework.Core;
using Archetype.Framework.State;

namespace Archetype.Framework.Parsing;

internal static class ReflectionExtensions
{
    public static Func<TWhence, TValue> CreateAccessor<TWhence, TValue>(this string[] path)
        where TWhence : IValueWhence
    {
        var rootType = typeof(TWhence);
        
        // Use Expression Trees to build a chain of calls to the accessor methods.
        // Then compile the expression tree into a delegate.
        
        var rootExpression = Expression.Parameter(rootType, rootType.Name.ToLower());
        
        Expression currentExpression = rootExpression;
        var currentType = rootType;

        foreach (var part in path)
        {
            var partDetails = part.Split(':');

            var pathPart = partDetails[0];
            var partArgs = partDetails.Length > 1 ? partDetails[1].Split(',') : Array.Empty<string>();
            
            var accessorMethod = currentType.GetPartMethod(pathPart);

            var partArgIndex = 0;
            if (accessorMethod is not null)
            {
                var args = new Expression[accessorMethod.GetParameters().Length];
                
                foreach (var parameter in accessorMethod.GetParameters())
                {
                    if (parameter.ParameterType == rootType)
                    {
                        args[parameter.Position] = rootExpression;
                    }
                    else if (parameter.ParameterType == typeof(int))
                    {
                        if (partArgIndex >= partArgs.Length)
                            throw new InvalidOperationException($"Not enough arguments for {part} of {string.Join('.', path)}");
                        
                        if (!int.TryParse(partArgs[partArgIndex], out var arg))
                            throw new InvalidOperationException($"Invalid type for {partArgs[partArgIndex]} of {string.Join(':', partDetails)} in {string.Join('.', path)}");
                        
                        args[parameter.Position] = Expression.Constant(arg);
                        partArgIndex++;
                    }
                    else if (parameter.ParameterType == typeof(string))
                    {
                        if (partArgIndex >= partArgs.Length)
                            throw new InvalidOperationException($"Not enough arguments for {part} of {string.Join('.', path)}");
                        
                        args[parameter.Position] = Expression.Constant(partArgs[partArgIndex]);
                        partArgIndex++;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid argument type for {parameter.ParameterType} of {part} of {string.Join('.', path)}");
                    }
                }
                
                currentExpression = Expression.Call(currentExpression, accessorMethod, args);
            }
            else if (partArgs.Length != 0)
            {
                throw new InvalidOperationException($"Invalid path: {part} of {string.Join('.', path)}");
            }
            else
            {
                var property = rootType.GetPartProperty(part);
                
                if (property is null)
                    throw new InvalidOperationException($"Invalid path: {part} of {string.Join('.', path)}");
                
                currentExpression = Expression.Property(currentExpression, property);
            }
            
            currentType = currentExpression.Type;
        }
        
        var lambda = Expression.Lambda<Func<TWhence, TValue>>(currentExpression, rootExpression);
        
        return lambda.Compile();
    }
    
    public static Type GetValueType<TWhence>(this string[] path)
        where TWhence : IValueWhence
    {
        var rootType = typeof(TWhence);

        var currentType = rootType;
        
        foreach (var part in path)
        {
            currentType = currentType.GetPartType(part);
            
            if (currentType is null)
                throw new InvalidOperationException($"Invalid path: {part} of {string.Join('.', path)}");
        }
        
        return currentType;
    }
    
    public static Type? GetPartType(this Type type, string part)
    {
        var partAccessorMethod = type.GetPartMethod(part);
        
        if (partAccessorMethod?.ReturnType is { } returnType)
            return returnType;

        var partProperty = type.GetPartProperty(part);
        
        return partProperty?.PropertyType;
    }
    
    private static MethodInfo? GetPartMethod(this Type type, string part)
    {
        var methods = type.GetMethodsNested(); 
        
        return methods
            .FirstOrDefault(m => m.GetCustomAttributes()
                .Any(a => a is PathPartAttribute { Name: { } name } && name == part));
    }
    
    private static PropertyInfo? GetPartProperty(this Type type, string part)
    {
        return type.GetPropertiesNested()
            .FirstOrDefault(p => p.GetCustomAttributes()
                .Any(a => a is PathPartAttribute { Name: { } name } && name == part));
    }
    
    private static IEnumerable<MethodInfo> GetMethodsNested(this Type type)
    {
        var methods = type.GetMethods();
        
        var nestedTypes = type.GetNestedTypes().Concat(type.GetInterfaces());

        return nestedTypes.Aggregate(methods, (current, nestedType) => current.Concat(nestedType.GetMethodsNested()).ToArray());
    }
    
    private static IEnumerable<PropertyInfo> GetPropertiesNested(this Type type)
    {
        var properties = type.GetProperties();
        
        var nestedTypes = type.GetNestedTypes().Concat(type.GetInterfaces());

        return nestedTypes.Aggregate(properties, (current, nestedType) => current.Concat(nestedType.GetPropertiesNested()).ToArray());
    }
}