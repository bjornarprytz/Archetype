﻿
using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.Meta;

public class StaticKeywordAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordAttribute(keyword, operandDeclaration, nOptional) { }
public class TargetKeywordAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordAttribute(keyword, operandDeclaration, nOptional) { }
public class ConditionKeywordAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordAttribute(keyword, operandDeclaration, nOptional) { }
public class ComputedValueKeywordAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordAttribute(keyword, operandDeclaration, nOptional) { }
public class CostKeywordAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordAttribute(keyword, operandDeclaration, nOptional) { }
public class EffectKeywordAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0) : KeywordAttribute(keyword, operandDeclaration, nOptional) { }

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public abstract class KeywordAttribute: Attribute
{
    protected KeywordAttribute(string keyword, Type? operandDeclaration=null, int nOptional=0)
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