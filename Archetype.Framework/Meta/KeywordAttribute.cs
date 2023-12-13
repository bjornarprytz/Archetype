using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.Meta;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class KeywordAttribute(string keyword, Type operandDeclaration) : Attribute
{
    
    
    public string Keyword => keyword;
    public OperandDeclaration? OperandDeclaration { get; } = Activator.CreateInstance(operandDeclaration) as OperandDeclaration;
}

[AttributeUsage(AttributeTargets.Class)]
public class OperandsAttribute<T> : Attribute
    where T : OperandDeclaration
{
}