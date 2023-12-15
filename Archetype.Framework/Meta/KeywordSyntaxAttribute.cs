
using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.Meta;

public class StaticSyntaxAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordSyntaxAttribute(keyword, operandDeclaration, nOptional) { }
public class TargetSyntaxAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordSyntaxAttribute(keyword, operandDeclaration, nOptional) { }
public class ConditionSyntaxAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordSyntaxAttribute(keyword, operandDeclaration, nOptional) { }
public class ComputedValueSyntaxAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordSyntaxAttribute(keyword, operandDeclaration, nOptional) { }
public class CostSyntaxAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordSyntaxAttribute(keyword, operandDeclaration, nOptional) { }
public class EffectSyntaxAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordSyntaxAttribute(keyword, operandDeclaration, nOptional) { }

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public abstract class KeywordSyntaxAttribute: Attribute
{
    protected KeywordSyntaxAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0)
    {
        Keyword = keyword;
        operandDeclaration ??= typeof(OperandDeclaration);

        Operands = CreateOperandDeclaration(operandDeclaration, nOptional);
    }

    public string Keyword { get; }
    
    public OperandDeclaration Operands { get; }
    
    private static OperandDeclaration CreateOperandDeclaration(Type operandDeclaration, int nOptional)
    {
        if (!operandDeclaration.IsAssignableTo(typeof(OperandDeclaration)))
        {
            throw new InvalidOperationException($"OperandDeclaration must be assignable to {nameof(OperandDeclaration)}");
        }
        
        var genericTypes = operandDeclaration.GetGenericArguments();

        var type = genericTypes.Length switch
        {
            0 => typeof(OperandDeclaration).MakeGenericType(genericTypes),
            1 => typeof(OperandDeclaration<>).MakeGenericType(genericTypes),
            2 => typeof(OperandDeclaration<,>).MakeGenericType(genericTypes),
            3 => typeof(OperandDeclaration<,,>).MakeGenericType(genericTypes),
            4 => typeof(OperandDeclaration<,,,>).MakeGenericType(genericTypes),
            5 => typeof(OperandDeclaration<,,,,>).MakeGenericType(genericTypes),
            _ => throw new InvalidOperationException(
                $"OperandDeclaration<> does not support {genericTypes.Length} generic arguments")
        };

        return (genericTypes.Length) switch
        {
            0 => Activator.CreateInstance(type) as OperandDeclaration,
            1 => Activator.CreateInstance(type, new object?[] { nOptional > 0 }) as OperandDeclaration,
            _ => Activator.CreateInstance(type, nOptional) as OperandDeclaration
        } ?? throw new InvalidOperationException($"Failed to create OperandDeclaration<> of type {type}");
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class OperandsAttribute<T> : Attribute
    where T : OperandDeclaration
{
}