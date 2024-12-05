using System.Linq.Expressions;
using System.Reflection;
using Archetype.Framework.Core;
using Archetype.Framework.Resolution;
using Archetype.Framework.State;

namespace Archetype.Framework.Parsing;

internal static class ReflectionExtensions
{
    public static bool Compare<T>(this ComparisonOperator comparisonOperator, T? left, T? right)
    {
        comparisonOperator.ValidateOrThrow<T>();
        
        if (left is null || right is null)
            return false;

        return (left, right) switch
        {
            (int l, int r) => CompareInt(l, r),
            (string l, string r) => CompareString(l, r),
            (IEnumerable<IAtom>l, IAtom r) => CompareGroup(l, r),
            _ => throw new InvalidOperationException($"Invalid comparison operator: {comparisonOperator}")
        };

        bool CompareInt(int l, int r)
        {
            return comparisonOperator switch
            {
                ComparisonOperator.Equal => l == r,
                ComparisonOperator.NotEqual => l != r,
                ComparisonOperator.GreaterThan => l > r,
                ComparisonOperator.GreaterThanOrEqual => l >= r,
                ComparisonOperator.LessThan => l < r,
                ComparisonOperator.LessThanOrEqual => l <= r,
                _ => throw new InvalidOperationException($"Invalid comparison operator: {comparisonOperator}")
            };
        }
        
        bool CompareString(string l, string r)
        {
            return comparisonOperator switch
            {
                ComparisonOperator.Equal => string.Equals(l, r, StringComparison.InvariantCultureIgnoreCase),
                ComparisonOperator.NotEqual => !string.Equals(l, r, StringComparison.InvariantCultureIgnoreCase),
                ComparisonOperator.Contains => l.Contains(r, StringComparison.InvariantCultureIgnoreCase),
                ComparisonOperator.NotContains => !l.Contains(r, StringComparison.InvariantCultureIgnoreCase),
                _ => throw new InvalidOperationException($"Invalid comparison operator: {comparisonOperator}")
            };
        }
        
        bool CompareGroup(IEnumerable<IAtom> l, IAtom r)
        {
            return comparisonOperator switch
            {
                ComparisonOperator.Contains => l.Contains(r),
                ComparisonOperator.NotContains => !l.Contains(r),
                _ => throw new InvalidOperationException($"Invalid comparison operator: {comparisonOperator}")
            };
        }
    }
    
    public static TAttribute GetRequiredAttribute<TAttribute>(this MemberInfo member)
        where TAttribute : Attribute
    {
        var attribute = member.GetCustomAttribute<TAttribute>();

        if (attribute is null)
            throw new InvalidOperationException($"Missing required attribute: {typeof(TAttribute).Name} on {member.Name}");

        return attribute;
    }
    
   public static Func<TWhence, TValue?> CreateAccessor<TWhence, TValue>(this string[] path)
    where TWhence : IValueWhence
    {
        var rootType = typeof(TWhence);
        
        var parameterExpression = Expression.Parameter(rootType, rootType.Name.ToLower());

        var currentType = rootType;
        Expression bodyExpression = parameterExpression;
        foreach (var (pathPart, partArgs) in path.Select(part => part.SplitPart()))
        {
            var accessorMethod = currentType.GetPartMethod(pathPart);

            var partArgIndex = 0;
            Expression nextExpression;

            if (accessorMethod is not null)
            {
                var args = new Expression[accessorMethod.GetParameters().Length];
                foreach (var parameter in accessorMethod.GetParameters())
                {
                    if (parameter.ParameterType == rootType)
                    {
                        args[parameter.Position] = parameterExpression;
                    }
                    else if (parameter.ParameterType == typeof(int))
                    {
                        if (partArgIndex >= partArgs.Length)
                            throw new InvalidOperationException($"Not enough arguments for {pathPart} of {string.Join('.', path)}");

                        if (!int.TryParse(partArgs[partArgIndex], out var number))
                            throw new InvalidOperationException($"Invalid type for {partArgs[partArgIndex]} of {string.Join(',', partArgs)} in {string.Join('.', path)}");

                        args[parameter.Position] = Expression.Constant(number);
                        partArgIndex++;
                    }
                    else if (parameter.ParameterType == typeof(string))
                    {
                        if (partArgs == null || partArgIndex >= partArgs.Length || partArgs[partArgIndex] is not { Length: > 0 } word)
                            throw new InvalidOperationException($"Not enough arguments for {pathPart} of {string.Join('.', path)}");

                        args[parameter.Position] = Expression.Constant(word);
                        partArgIndex++;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid argument type for {parameter.ParameterType} of {pathPart} of {string.Join('.', path)}");
                    }
                }

                nextExpression = Expression.Call(bodyExpression, accessorMethod, args);
            }
            else if (partArgs.Length != 0)
            {
                throw new InvalidOperationException($"Invalid path: {pathPart} of {string.Join('.', path)}");
            }
            else
            {
                var property = currentType.GetPartProperty(pathPart);
                if (property is null)
                    throw new InvalidOperationException($"Invalid path: {pathPart} of {string.Join('.', path)}");

                nextExpression = Expression.Property(bodyExpression, property);
            }

            bodyExpression = bodyExpression.AddNullCheck(nextExpression);

            currentType = nextExpression.Type;
        }

        var lambda = Expression.Lambda<Func<TWhence, TValue>>(
            bodyExpression.ConvertIfNonNullable(),
            parameterExpression
        );  

        return lambda.Compile();
    }

    
    public static Type GetValueType<TWhence>(this string[] path)
        where TWhence : IValueWhence
    {
        var rootType = typeof(TWhence);

        var currentType = rootType;
        
        foreach (var (pathPart, _) in path.Select(part => part.SplitPart()))
        {
            currentType = currentType.GetPartType(pathPart);
            
            if (currentType is null)
                throw new InvalidOperationException($"Invalid path: {pathPart} of {string.Join('.', path)}");
        }
        
        return currentType.EnsureNullable();
    }

    private static Type? GetPartType(this Type type, string part)
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
            .FirstOrDefault(m => m.GetCustomAttribute<PathPartAttribute>() is { } attribute 
                                 && string.Equals(attribute.Name, part, StringComparison.InvariantCultureIgnoreCase));
    }
    
    private static PropertyInfo? GetPartProperty(this Type type, string part)
    {
        return type.GetPropertiesNested()
            .FirstOrDefault(p => p.GetCustomAttribute<PathPartAttribute>() is { } attribute 
                                 && string.Equals(attribute.Name, part, StringComparison.InvariantCultureIgnoreCase));
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
    
    private static ConditionalExpression AddNullCheck(this Expression currentExpression, Expression nextExpression)
    {
        return Expression.Condition(
            Expression.Equal(currentExpression, Expression.Constant(null, currentExpression.Type)),
            Expression.Default(nextExpression.Type.EnsureNullable()),
            nextExpression.ConvertIfNonNullable());
    }
    
    private static Expression ConvertIfNonNullable(this Expression expression)
    {
        return expression.Type.IsNonNullableValueType()
            ? Expression.Convert(expression, typeof(Nullable<>).MakeGenericType(expression.Type))
            : expression;
    }
    
    private static Type EnsureNullable(this Type type)
    {
        return type.IsNonNullableValueType() ? typeof(Nullable<>).MakeGenericType(type) : type;
    }
    
    private static bool IsNonNullableValueType(this Type type)
    {
        return !type.IsNullable();
    }
    private static bool IsNullable(this Type type)
    {
        return !type.IsValueType || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}