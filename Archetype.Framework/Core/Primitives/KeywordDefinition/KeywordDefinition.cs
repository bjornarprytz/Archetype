using Archetype.Framework.Extensions;

namespace Archetype.Framework.Core.Primitives;

public interface IKeywordDefinition
{
    string Keyword { get; } // ID
    IReadOnlyList<IOperandDescription> Operands { get; }
    IKeywordInstance CreateInstance(params object[] operands);
}


public abstract class KeywordDefinition : IKeywordDefinition
{
    protected KeywordDefinition()
    {
        Keyword =  GetType().Name;
    }
    public string Keyword { get; }
    protected virtual OperandDeclaration OperandDeclaration { get; } = new();
    public IReadOnlyList<IOperandDescription> Operands => OperandDeclaration;

    public IKeywordInstance CreateInstance(params object[] operands)
    {
        var operandList = operands.Select(o => o.ToOperand()).ToList();
        
        if (!OperandDeclaration.Validate(operandList))
        {
            throw new InvalidOperationException($"Invalid operands for keyword ({(this.TryGetKeywordName(out var keywordName) ? keywordName : "Unknown")})");
        }
        
        return new KeywordInstance 
        {
            Keyword = Keyword,
            Operands = operandList,
        };
    }
}


