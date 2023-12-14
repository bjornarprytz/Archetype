
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
        
        var type = typeof(OperandDeclaration<>).MakeGenericType(genericTypes);

        return (genericTypes.Length) switch
        {
            0 => Activator.CreateInstance(type) as OperandDeclaration,
            1 => Activator.CreateInstance(type, nOptional > 0) as OperandDeclaration,
            _ => Activator.CreateInstance(type, nOptional) as OperandDeclaration
        } ?? throw new InvalidOperationException($"Failed to create OperandDeclaration<> of type {type}");
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class OperandsAttribute<T> : Attribute
    where T : OperandDeclaration
{
}